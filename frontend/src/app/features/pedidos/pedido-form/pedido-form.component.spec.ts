import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { ClientesService } from '../../clientes/clientes.service';
import { PedidosService } from '../pedidos.service';
import { PedidoFormComponent } from './pedido-form.component';

describe('PedidoFormComponent', () => {
  let fixture: ComponentFixture<PedidoFormComponent>;
  let component: PedidoFormComponent;
  let pedidosService: jasmine.SpyObj<PedidosService>;
  let clientesService: jasmine.SpyObj<ClientesService>;

  beforeEach(async () => {
    pedidosService = jasmine.createSpyObj<PedidosService>('PedidosService', ['crear', 'listar', 'obtener']);
    clientesService = jasmine.createSpyObj<ClientesService>('ClientesService', ['listar', 'buscar', 'crear', 'actualizar']);
    clientesService.buscar.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [PedidoFormComponent],
      providers: [
        provideRouter([]),
        MessageService,
        ConfirmationService,
        { provide: PedidosService, useValue: pedidosService },
        { provide: ClientesService, useValue: clientesService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PedidoFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('calcula el total como la suma de cantidad por precio', () => {
    component.agregarLinea({ producto: 'A', cantidad: 2, precioUnitario: 2.5, descripcion: 'dos A' });
    component.agregarLinea({ producto: 'B', cantidad: 1, precioUnitario: 7, descripcion: 'uno B' });

    expect(component.total).toBe(12);
  });

  it('no envía el pedido si el formulario es inválido', () => {
    component.submit();

    expect(pedidosService.crear).not.toHaveBeenCalled();
    expect(component.form.invalid).toBeTrue();
  });

  it('envía el pedido con las líneas y el total calculado cuando es válido', () => {
    pedidosService.crear.and.returnValue(of({ id: 1, clienteNombre: 'Ana', fechaReparto: '2026-10-15', estado: 'Pendiente', total: 12 }));

    component.form.controls.clienteId.setValue(1);
    component.form.controls.fechaReparto.setValue(new Date('2026-10-15'));
    component.agregarLinea({ producto: 'A', cantidad: 2, precioUnitario: 2.5, descripcion: 'dos A' });
    component.agregarLinea({ producto: 'B', cantidad: 1, precioUnitario: 7, descripcion: 'uno B' });

    component.submit();

    expect(pedidosService.crear).toHaveBeenCalled();
    const enviado = pedidosService.crear.calls.mostRecent().args[0];
    expect(enviado.lineas.length).toBe(2);
    expect(enviado.clienteId).toBe(1);
  });
});
