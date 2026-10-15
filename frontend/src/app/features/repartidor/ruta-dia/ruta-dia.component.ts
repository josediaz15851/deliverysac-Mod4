import { Component, OnInit, inject } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextarea } from 'primeng/inputtextarea';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CambioEstadoRuta, PedidoRuta, RepartidorService } from '../repartidor.service';
import { EstadoTagComponent } from '../../../shared/estado-tag/estado-tag.component';
import { DataStateComponent } from '../../../shared/data-state/data-state.component';
import { OfflineService } from '../../../core/offline/offline.service';

const ESTADOS_CON_MOTIVO = ['NoEntregado', 'Reprogramado'];
const ESTADOS_FINALES = ['Entregado'];

@Component({
  selector: 'app-ruta-dia',
  imports: [CurrencyPipe, FormsModule, ButtonModule, DialogModule, InputTextarea, EstadoTagComponent, DataStateComponent],
  templateUrl: './ruta-dia.component.html',
  styleUrl: './ruta-dia.component.scss',
})
export class RutaDiaComponent implements OnInit {
  private readonly repartidorService = inject(RepartidorService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly offlineService = inject(OfflineService);

  pedidos: PedidoRuta[] = [];
  loading = false;

  motivoVisible = false;
  motivo = '';
  motivoError = false;

  private pedidoSeleccionado: PedidoRuta | null = null;
  private estadoSeleccionado = '';

  ngOnInit(): void {
    this.offlineService.iniciarSincronizacionAutomatica();
    this.cargar();
  }

  cargar(): void {
    this.loading = true;
    this.repartidorService.listarRuta().subscribe({
      next: (pedidos) => {
        this.pedidos = pedidos;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  solicitarCambio(pedido: PedidoRuta, estado: string): void {
    if (ESTADOS_CON_MOTIVO.includes(estado)) {
      this.pedidoSeleccionado = pedido;
      this.estadoSeleccionado = estado;
      this.motivo = '';
      this.motivoError = false;
      this.motivoVisible = true;
      return;
    }

    if (ESTADOS_FINALES.includes(estado)) {
      this.confirmationService.confirm({
        header: 'Confirmar cambio de estado',
        message: 'Esta acción es definitiva. ¿Desea continuar?',
        icon: 'pi pi-exclamation-triangle',
        accept: () => this.enviarCambio(pedido, estado),
      });
      return;
    }

    this.enviarCambio(pedido, estado);
  }

  confirmarMotivo(): void {
    if (!this.pedidoSeleccionado || this.motivo.trim().length === 0) {
      this.motivoError = true;
      return;
    }

    const pedido = this.pedidoSeleccionado;
    const estado = this.estadoSeleccionado;
    const motivo = this.motivo.trim();

    this.motivoVisible = false;
    this.enviarCambio(pedido, estado, motivo);
  }

  accionesDisponibles(estado: string): string[] {
    switch (estado) {
      case 'Asignado':
        return ['EnRuta'];
      case 'EnRuta':
        return ['Entregado', 'NoEntregado', 'Reprogramado'];
      default:
        return [];
    }
  }

  etiquetaAccion(estado: string): string {
    switch (estado) {
      case 'EnRuta':
        return 'Iniciar ruta';
      case 'Entregado':
        return 'Entregado';
      case 'NoEntregado':
        return 'No entregado';
      case 'Reprogramado':
        return 'Reprogramar';
      default:
        return estado;
    }
  }

  private enviarCambio(pedido: PedidoRuta, estado: string, motivo?: string): void {
    const cambio: CambioEstadoRuta = { estadoNuevo: estado };
    if (motivo) {
      cambio.motivo = motivo;
    }

    this.repartidorService.cambiarEstado(pedido.id, cambio).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Estado actualizado',
          detail: 'El pedido se actualizó correctamente',
        });
        this.cargar();
      },
      error: (error) => {
        if ((error as { status?: number }).status === 0) {
          void this.offlineService
            .encolar({ pedidoId: pedido.id, estadoNuevo: estado, motivo })
            .then(() =>
              this.messageService.add({
                severity: 'info',
                summary: 'Guardado sin conexión',
                detail: 'El cambio se enviará automáticamente al recuperar la conexión.',
              }),
            );
          return;
        }

        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo actualizar el estado',
        });
      },
    });
  }
}
