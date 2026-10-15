import { Component, OnInit, inject } from '@angular/core';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';
import { Cliente, ClientesService } from '../clientes.service';
import { ClienteFormComponent } from '../cliente-form/cliente-form.component';
import { DataStateComponent } from '../../../shared/data-state/data-state.component';

@Component({
  selector: 'app-clientes-list',
  imports: [TableModule, ButtonModule, DialogModule, ClienteFormComponent, DataStateComponent],
  templateUrl: './clientes-list.component.html',
  styleUrl: './clientes-list.component.scss',
})
export class ClientesListComponent implements OnInit {
  private readonly clientesService = inject(ClientesService);
  private readonly messageService = inject(MessageService);

  clientes: Cliente[] = [];
  total = 0;
  loading = false;
  dialogVisible = false;
  selected: Cliente | null = null;
  private page = 1;
  size = 10;

  ngOnInit(): void {
    this.cargar();
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows ?? this.size;
    this.size = rows;
    this.page = Math.floor((event.first ?? 0) / rows) + 1;
    this.cargar();
  }

  abrirNuevo(): void {
    this.selected = null;
    this.dialogVisible = true;
  }

  abrirEditar(cliente: Cliente): void {
    this.selected = cliente;
    this.dialogVisible = true;
  }

  cerrarDialogo(): void {
    this.dialogVisible = false;
    this.selected = null;
  }

  guardar(cliente: Omit<Cliente, 'id'>): void {
    const operacion = this.selected
      ? this.clientesService.actualizar(this.selected.id, cliente)
      : this.clientesService.crear(cliente);

    operacion.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Cliente guardado',
          detail: 'El cliente se registró correctamente',
        });
        this.cerrarDialogo();
        this.cargar();
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo guardar el cliente',
        });
      },
    });
  }

  private cargar(): void {
    this.loading = true;
    this.clientesService.listar(this.page, this.size).subscribe({
      next: (resultado) => {
        this.clientes = resultado.items;
        this.total = resultado.total;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
