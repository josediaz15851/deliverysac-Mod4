import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { PedidoDia, PedidosService } from '../pedidos/pedidos.service';
import { ListadoDiaComponent } from './listado-dia.component';

describe('ListadoDiaComponent', () => {
  let fixture: ComponentFixture<ListadoDiaComponent>;
  let component: ListadoDiaComponent;
  let service: jasmine.SpyObj<PedidosService>;

  const pedido: PedidoDia = {
    id: 1,
    clienteNombre: 'Ana Torres',
    direccion: 'Calle 1',
    fechaReparto: '2026-10-15',
    estado: 'Reprogramado',
    motivo: 'El cliente pidió otro día',
    fechaHoraCambio: '2026-10-15T10:30:00-05:00',
    repartidorNombre: 'repartidor',
  };

  beforeEach(async () => {
    service = jasmine.createSpyObj<PedidosService>('PedidosService', ['listarDia']);
    service.listarDia.and.returnValue(of({ items: [pedido], total: 1 }));

    await TestBed.configureTestingModule({
      imports: [ListadoDiaComponent],
      providers: [MessageService, { provide: PedidosService, useValue: service }],
    }).compileComponents();

    fixture = TestBed.createComponent(ListadoDiaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('muestra estado, motivo y hora de los pedidos del día', () => {
    expect(component.pedidos.length).toBe(1);

    const texto = fixture.nativeElement.textContent as string;
    expect(texto).toContain('Reprogramado');
    expect(texto).toContain('El cliente pidió otro día');
  });

  it('refresca automáticamente cada 5 segundos', () => {
    jasmine.clock().install();
    fixture.destroy();

    fixture = TestBed.createComponent(ListadoDiaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    service.listarDia.calls.reset();

    jasmine.clock().tick(5000);

    expect(service.listarDia.calls.count()).toBe(1);
    jasmine.clock().uninstall();
  });

  it('muestra el banner SIN CONEXIÓN al perder la conexión', () => {
    window.dispatchEvent(new Event('offline'));
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).toContain('SIN CONEXIÓN');

    window.dispatchEvent(new Event('online'));
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).not.toContain('SIN CONEXIÓN');
  });
});
