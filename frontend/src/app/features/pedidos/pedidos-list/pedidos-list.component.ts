import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CurrencyPipe } from '@angular/common';
import { Pedido, PedidosService } from '../pedidos.service';
import { DataStateComponent } from '../../../shared/data-state/data-state.component';
import { EstadoTagComponent } from '../../../shared/estado-tag/estado-tag.component';

@Component({
  selector: 'app-pedidos-list',
  imports: [TableModule, ButtonModule, DatePipe, CurrencyPipe, DataStateComponent, EstadoTagComponent],
  templateUrl: './pedidos-list.component.html',
  styleUrl: './pedidos-list.component.scss',
})
export class PedidosListComponent implements OnInit {
  private readonly pedidosService = inject(PedidosService);
  private readonly router = inject(Router);

  pedidos: Pedido[] = [];
  total = 0;
  loading = false;
  size = 10;
  private page = 1;

  ngOnInit(): void {
    this.cargar();
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows ?? this.size;
    this.size = rows;
    this.page = Math.floor((event.first ?? 0) / rows) + 1;
    this.cargar();
  }

  nuevo(): void {
    void this.router.navigate(['/pedidos/nuevo']);
  }

  private cargar(): void {
    this.loading = true;
    this.pedidosService.listar(this.page, this.size).subscribe({
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
