import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';
import { DatePipe } from '@angular/common';
import { Pedido } from '../../pedidos/pedidos.service';
import { AsignacionesService, AsignarPedido, Repartidor, Unidad } from '../asignaciones.service';
import { DataStateComponent } from '../../../shared/data-state/data-state.component';

@Component({
  selector: 'app-asignacion-form',
  imports: [FormsModule, TableModule, ButtonModule, DialogModule, SelectModule, TagModule, DatePipe, DataStateComponent],
  templateUrl: './asignacion-form.component.html',
  styleUrl: './asignacion-form.component.scss',
})
export class AsignacionFormComponent implements OnInit {
  private readonly asignacionesService = inject(AsignacionesService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  candidatos: Pedido[] = [];
  repartidores: Repartidor[] = [];
  unidades: Unidad[] = [];
  total = 0;
  loading = false;
  size = 10;
  dialogVisible = false;
  pedidoSeleccionado: Pedido | null = null;
  repartidorSeleccionadoId: number | null = null;
  unidadSeleccionadaId: number | null = null;

  private page = 1;

  ngOnInit(): void {
    this.cargarCatalogos();
    this.cargarCandidatos();
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows ?? this.size;
    this.size = rows;
    this.page = Math.floor((event.first ?? 0) / rows) + 1;
    this.cargarCandidatos();
  }

  cargarCandidatos(): void {
    this.loading = true;
    this.asignacionesService.listarCandidatos(this.page, this.size).subscribe({
      next: (resultado) => {
        this.candidatos = resultado.items;
        this.total = resultado.total;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  seleccionarPedido(pedido: Pedido): void {
    this.pedidoSeleccionado = pedido;
    this.repartidorSeleccionadoId = null;
    this.unidadSeleccionadaId = null;
    this.dialogVisible = true;
  }

  confirmarAsignacion(asignacion?: AsignarPedido): void {
    const data: AsignarPedido =
      asignacion ?? {
        repartidorId: this.repartidorSeleccionadoId!,
        unidadId: this.unidadSeleccionadaId!,
      };

    if (this.pedidoSeleccionado?.estado === 'Reprogramado') {
      this.confirmationService.confirm({
        header: 'Confirmar reasignación',
        message: 'Este pedido fue reprogramado. ¿Desea reasignarlo?',
        icon: 'pi pi-exclamation-triangle',
        accept: () => this.enviar(data),
      });
      return;
    }

    this.enviar(data);
  }

  get puedeAsignar(): boolean {
    return this.repartidorSeleccionadoId !== null && this.unidadSeleccionadaId !== null;
  }

  private enviar(asignacion: AsignarPedido): void {
    if (!this.pedidoSeleccionado) {
      return;
    }

    this.asignacionesService.asignar(this.pedidoSeleccionado.id, asignacion).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Pedido asignado',
          detail: 'La asignación se registró correctamente',
        });
        this.dialogVisible = false;
        this.cargarCandidatos();
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo asignar el pedido',
        });
      },
    });
  }

  private cargarCatalogos(): void {
    this.asignacionesService.listarRepartidores().subscribe((repartidores) => (this.repartidores = repartidores));
    this.asignacionesService.listarUnidades().subscribe((unidades) => (this.unidades = unidades));
  }
}
