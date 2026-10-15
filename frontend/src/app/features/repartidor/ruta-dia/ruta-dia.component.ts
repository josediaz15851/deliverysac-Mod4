import { Component, OnInit, inject } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { PedidoRuta, RepartidorService } from '../repartidor.service';
import { EstadoTagComponent } from '../../../shared/estado-tag/estado-tag.component';
import { DataStateComponent } from '../../../shared/data-state/data-state.component';

@Component({
  selector: 'app-ruta-dia',
  imports: [CurrencyPipe, EstadoTagComponent, DataStateComponent],
  templateUrl: './ruta-dia.component.html',
  styleUrl: './ruta-dia.component.scss',
})
export class RutaDiaComponent implements OnInit {
  private readonly repartidorService = inject(RepartidorService);

  pedidos: PedidoRuta[] = [];
  loading = false;

  ngOnInit(): void {
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
}
