import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { PedidoRuta, RepartidorService } from '../repartidor.service';
import { RutaDiaComponent } from './ruta-dia.component';

describe('RutaDiaComponent - cambio de estado', () => {
  let fixture: ComponentFixture<RutaDiaComponent>;
  let component: RutaDiaComponent;
  let service: jasmine.SpyObj<RepartidorService>;
  let confirmationService: ConfirmationService;

  const pedidoEnRuta: PedidoRuta = {
    id: 1,
    clienteNombre: 'Ana Torres',
    direccion: 'Calle 1',
    estado: 'EnRuta',
    fechaReparto: '2026-10-15',
    total: 12,
  };

  beforeEach(async () => {
    service = jasmine.createSpyObj<RepartidorService>('RepartidorService', ['listarRuta', 'cambiarEstado']);
    service.listarRuta.and.returnValue(of([]));
    service.cambiarEstado.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [RutaDiaComponent],
      providers: [
        MessageService,
        ConfirmationService,
        { provide: RepartidorService, useValue: service },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RutaDiaComponent);
    component = fixture.componentInstance;
    confirmationService = TestBed.inject(ConfirmationService);
    fixture.detectChanges();
  });

  it('pide motivo para NO_ENTREGADO y no envía sin motivo', () => {
    component.solicitarCambio(pedidoEnRuta, 'NoEntregado');

    expect(component.motivoVisible).toBeTrue();

    component.confirmarMotivo();

    expect(service.cambiarEstado).not.toHaveBeenCalled();
  });

  it('envía el cambio con motivo válido', () => {
    component.solicitarCambio(pedidoEnRuta, 'NoEntregado');
    component.motivo = 'El cliente no se encontraba';

    component.confirmarMotivo();

    expect(service.cambiarEstado).toHaveBeenCalledWith(1, {
      estadoNuevo: 'NoEntregado',
      motivo: 'El cliente no se encontraba',
    });
  });

  it('pide confirmación antes de marcar un estado final', () => {
    const confirmSpy = spyOn(confirmationService, 'confirm').and.callFake((options) => {
      options.accept?.();
      return confirmationService;
    });

    component.solicitarCambio(pedidoEnRuta, 'Entregado');

    expect(confirmSpy).toHaveBeenCalled();
    expect(service.cambiarEstado).toHaveBeenCalledWith(1, { estadoNuevo: 'Entregado' });
  });
});
