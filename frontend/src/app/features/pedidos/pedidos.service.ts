import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../../core/config/config.service';
import { PagedResult } from '../clientes/clientes.service';

export interface PedidoLinea {
  producto: string;
  cantidad: number;
  precioUnitario: number;
  descripcion: string;
}

export interface CrearPedido {
  clienteId: number;
  fechaReparto: string;
  comentario?: string;
  lineas: PedidoLinea[];
}

export interface Pedido {
  id: number;
  clienteNombre: string;
  fechaReparto: string;
  estado: string;
  total: number;
}

export interface PedidoLineaDetalle extends PedidoLinea {
  id: number;
}

export interface PedidoDetalle extends Pedido {
  comentario?: string;
  lineas: PedidoLineaDetalle[];
}

export interface PedidoDia {
  id: number;
  clienteNombre: string;
  direccion: string;
  fechaReparto: string;
  estado: string;
  motivo?: string;
  fechaHoraCambio?: string;
  repartidorNombre?: string;
}

@Injectable({ providedIn: 'root' })
export class PedidosService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);
  private readonly baseUrl = `${this.config.apiUrl}/api/pedidos`;

  crear(pedido: CrearPedido): Observable<Pedido> {
    return this.http.post<Pedido>(this.baseUrl, pedido);
  }

  listar(page: number, size: number): Observable<PagedResult<Pedido>> {
    const params = new HttpParams().set('page', page).set('size', size);
    return this.http.get<PagedResult<Pedido>>(this.baseUrl, { params });
  }

  obtener(id: number): Observable<PedidoDetalle> {
    return this.http.get<PedidoDetalle>(`${this.baseUrl}/${id}`);
  }

  listarDia(page: number, size: number): Observable<PagedResult<PedidoDia>> {
    const params = new HttpParams().set('page', page).set('size', size);
    return this.http.get<PagedResult<PedidoDia>>(`${this.baseUrl}/dia`, { params });
  }
}
