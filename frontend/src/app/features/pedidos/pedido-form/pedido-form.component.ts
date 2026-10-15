import { Component, inject } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CurrencyPipe, formatDate } from '@angular/common';
import { AutoCompleteCompleteEvent, AutoCompleteModule } from 'primeng/autocomplete';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { TableModule } from 'primeng/table';
import { MessageService } from 'primeng/api';
import { Cliente, ClientesService } from '../../clientes/clientes.service';
import { CrearPedido, PedidoLinea, PedidosService } from '../pedidos.service';

@Component({
  selector: 'app-pedido-form',
  imports: [
    ReactiveFormsModule,
    AutoCompleteModule,
    ButtonModule,
    CalendarModule,
    InputNumberModule,
    InputTextModule,
    InputTextarea,
    TableModule,
    CurrencyPipe,
  ],
  templateUrl: './pedido-form.component.html',
  styleUrl: './pedido-form.component.scss',
})
export class PedidoFormComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly pedidosService = inject(PedidosService);
  private readonly clientesService = inject(ClientesService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  clientesSugeridos: Cliente[] = [];

  readonly form = this.formBuilder.group({
    clienteId: this.formBuilder.control<number | null>(null, [Validators.required]),
    fechaReparto: this.formBuilder.control<Date | null>(null, [Validators.required]),
    comentario: this.formBuilder.control<string>(''),
    lineas: this.formBuilder.array<FormGroup>([]),
  });

  get lineas(): FormArray {
    return this.form.controls.lineas;
  }

  get total(): number {
    return this.lineas.controls.reduce((suma, linea) => {
      const value = linea.value as PedidoLinea;
      return suma + (value.cantidad ?? 0) * (value.precioUnitario ?? 0);
    }, 0);
  }

  buscarCliente(event: AutoCompleteCompleteEvent): void {
    this.clientesService.buscar(event.query).subscribe((clientes) => {
      this.clientesSugeridos = clientes;
    });
  }

  onClienteSeleccionado(cliente: Cliente | null): void {
    this.form.controls.clienteId.setValue(cliente?.id ?? null);
  }

  agregarLinea(data?: Partial<PedidoLinea>): void {
    this.lineas.push(
      this.formBuilder.group({
        producto: this.formBuilder.control(data?.producto ?? '', [Validators.required]),
        cantidad: this.formBuilder.control(data?.cantidad ?? 1, [Validators.required, Validators.min(1)]),
        precioUnitario: this.formBuilder.control(data?.precioUnitario ?? 0, [Validators.required, Validators.min(0)]),
        descripcion: this.formBuilder.control(data?.descripcion ?? '', [Validators.required]),
      }),
    );
  }

  quitarLinea(index: number): void {
    this.lineas.removeAt(index);
  }

  asFormGroup(control: AbstractControl): FormGroup {
    return control as FormGroup;
  }

  submit(): void {
    if (this.form.invalid || this.lineas.length === 0) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload: CrearPedido = {
      clienteId: value.clienteId!,
      fechaReparto: formatDate(value.fechaReparto!, 'yyyy-MM-dd', 'en-US'),
      comentario: value.comentario || undefined,
      lineas: this.lineas.getRawValue() as PedidoLinea[],
    };

    this.pedidosService.crear(payload).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Pedido creado',
          detail: 'El pedido se registró correctamente',
        });
        void this.router.navigate(['/pedidos']);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo crear el pedido',
        });
      },
    });
  }
}
