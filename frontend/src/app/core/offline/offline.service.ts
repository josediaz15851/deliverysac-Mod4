import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { MessageService } from 'primeng/api';
import { ConfigService } from '../config/config.service';
import { OfflineStore, OperacionPendiente } from './offline.models';

export interface CambioEstado {
  pedidoId: number;
  estadoNuevo: string;
  motivo?: string;
}

@Injectable({ providedIn: 'root' })
export class OfflineService {
  private readonly store = inject(OfflineStore);
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);
  private readonly messageService = inject(MessageService);

  private autoSyncIniciado = false;
  private readonly retrasoBaseMs = 3000;
  private readonly maxIntentos = 5;

  async encolar(cambio: CambioEstado, operacionId: string = this.generarId()): Promise<string> {
    const existentes = await this.store.getAll();
    if (existentes.some((operacion) => operacion.operacionId === operacionId)) {
      return operacionId;
    }

    await this.store.add({
      operacionId,
      pedidoId: cambio.pedidoId,
      estadoNuevo: cambio.estadoNuevo,
      motivo: cambio.motivo,
      encoladaEn: new Date().toISOString(),
    });

    return operacionId;
  }

  pendientes(): Promise<OperacionPendiente[]> {
    return this.store.getAll();
  }

  iniciarSincronizacionAutomatica(): void {
    if (this.autoSyncIniciado || typeof window === 'undefined') {
      return;
    }

    this.autoSyncIniciado = true;
    window.addEventListener('online', () => void this.reintentarConBackoff(0));
  }

  async reintentarConBackoff(intento: number): Promise<void> {
    await this.sincronizar();

    const restantes = await this.store.getAll();
    if (restantes.length > 0 && intento < this.maxIntentos) {
      const retraso = this.retrasoBaseMs * 2 ** intento;
      setTimeout(() => void this.reintentarConBackoff(intento + 1), retraso);
    }
  }

  async sincronizar(): Promise<number> {
    const operaciones = await this.store.getAll();
    let enviadas = 0;

    for (const operacion of operaciones) {
      try {
        await firstValueFrom(this.http.post(this.url(operacion.pedidoId), this.cuerpo(operacion)));
        await this.store.remove(operacion.operacionId);
        enviadas += 1;
      } catch (error) {
        const status = (error as { status?: number }).status;
        if (status === 409 || status === 400) {
          await this.store.remove(operacion.operacionId);
          this.messageService.add({
            severity: 'warn',
            summary: 'Cambio descartado',
            detail: 'El pedido cambió en el servidor; se descartó el cambio local.',
          });
        }
      }
    }

    return enviadas;
  }

  private url(pedidoId: number): string {
    return `${this.config.apiUrl}/api/pedidos/${pedidoId}/estados`;
  }

  private cuerpo(operacion: OperacionPendiente): Record<string, unknown> {
    const body: Record<string, unknown> = {
      operacionId: operacion.operacionId,
      estadoNuevo: operacion.estadoNuevo,
    };

    if (operacion.motivo) {
      body['motivo'] = operacion.motivo;
    }

    return body;
  }

  private generarId(): string {
    return globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(16).slice(2)}`;
  }
}
