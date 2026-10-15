import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { PedidoDia, PedidosService } from '../pedidos/pedidos.service';
import { DataStateComponent } from '../../shared/data-state/data-state.component';
import { EstadoTagComponent } from '../../shared/estado-tag/estado-tag.component';

@Component({
  selector: 'app-listado-dia',
  imports: [TableModule, DatePipe, DataStateComponent, EstadoTagComponent],
  templateUrl: './listado-dia.component.html',
  styleUrl: './listado-dia.component.scss',
})
export class ListadoDiaComponent implements OnInit, OnDestroy {
  private readonly pedidosService = inject(PedidosService);

  pedidos: PedidoDia[] = [];
  total = 0;
  loading = false;
  size = 10;
  sinConexion = typeof navigator !== 'undefined' && !navigator.onLine;

  private page = 1;
  private intervalId?: ReturnType<typeof setInterval>;

  private readonly alRecuperarConexion = () => {
    this.sinConexion = false;
    this.cargar();
  };

  private readonly alPerderConexion = () => {
    this.sinConexion = true;
  };

  ngOnInit(): void {
    this.cargar();

    this.intervalId = setInterval(() => {
      if (!this.sinConexion) {
        this.cargar();
      }
    }, 5000);

    window.addEventListener('online', this.alRecuperarConexion);
    window.addEventListener('offline', this.alPerderConexion);
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }

    window.removeEventListener('online', this.alRecuperarConexion);
    window.removeEventListener('offline', this.alPerderConexion);
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows ?? this.size;
    this.size = rows;
    this.page = Math.floor((event.first ?? 0) / rows) + 1;
    this.cargar();
  }

  private cargar(): void {
    this.loading = true;
    this.pedidosService.listarDia(this.page, this.size).subscribe({
      next: (resultado) => {
        this.pedidos = resultado.items;
        this.total = resultado.total;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
