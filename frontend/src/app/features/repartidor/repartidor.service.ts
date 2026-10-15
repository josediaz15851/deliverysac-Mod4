import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../../core/config/config.service';

export interface PedidoRuta {
  id: number;
  clienteNombre: string;
  direccion: string;
  estado: string;
  fechaReparto: string;
  total: number;
}

@Injectable({ providedIn: 'root' })
export class RepartidorService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  listarRuta(): Observable<PedidoRuta[]> {
    return this.http.get<PedidoRuta[]>(`${this.config.apiUrl}/api/repartidor/pedidos`);
  }
}
