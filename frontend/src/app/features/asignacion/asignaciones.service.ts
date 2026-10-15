import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../../core/config/config.service';
import { PagedResult } from '../clientes/clientes.service';
import { Pedido } from '../pedidos/pedidos.service';

export interface Repartidor {
  id: number;
  nombreCompleto: string;
  nombreUsuario: string;
}

export interface Unidad {
  id: number;
  identificador: string;
  tipo: string;
}

export interface AsignarPedido {
  repartidorId: number;
  unidadId: number;
}

@Injectable({ providedIn: 'root' })
export class AsignacionesService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  listarCandidatos(page: number, size: number): Observable<PagedResult<Pedido>> {
    const params = new HttpParams().set('page', page).set('size', size);
    return this.http.get<PagedResult<Pedido>>(`${this.config.apiUrl}/api/pedidos/candidatos`, { params });
  }

  listarRepartidores(): Observable<Repartidor[]> {
    return this.http.get<Repartidor[]>(`${this.config.apiUrl}/api/repartidores`);
  }

  listarUnidades(): Observable<Unidad[]> {
    return this.http.get<Unidad[]>(`${this.config.apiUrl}/api/unidades`);
  }

  asignar(pedidoId: number, asignacion: AsignarPedido): Observable<void> {
    return this.http.post<void>(`${this.config.apiUrl}/api/pedidos/${pedidoId}/asignaciones`, asignacion);
  }
}
