import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../../core/config/config.service';

export interface Cliente {
  id: number;
  nombre: string;
  direccion: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
}

@Injectable({ providedIn: 'root' })
export class ClientesService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);
  private readonly baseUrl = `${this.config.apiUrl}/api/clientes`;

  listar(page: number, size: number): Observable<PagedResult<Cliente>> {
    const params = new HttpParams().set('page', page).set('size', size);
    return this.http.get<PagedResult<Cliente>>(this.baseUrl, { params });
  }

  buscar(q: string): Observable<Cliente[]> {
    const params = new HttpParams().set('q', q);
    return this.http.get<Cliente[]>(`${this.baseUrl}/buscar`, { params });
  }

  crear(cliente: Omit<Cliente, 'id'>): Observable<Cliente> {
    return this.http.post<Cliente>(this.baseUrl, cliente);
  }

  actualizar(id: number, cliente: Omit<Cliente, 'id'>): Observable<Cliente> {
    return this.http.put<Cliente>(`${this.baseUrl}/${id}`, cliente);
  }
}
