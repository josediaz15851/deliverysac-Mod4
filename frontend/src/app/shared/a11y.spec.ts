import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { OfflineService } from '../core/offline/offline.service';
import { PedidoRuta, RepartidorService } from '../features/repartidor/repartidor.service';
import { RutaDiaComponent } from '../features/repartidor/ruta-dia/ruta-dia.component';

describe('Accesibilidad de modales', () => {
  let fixture: ComponentFixture<RutaDiaComponent>;
  let component: RutaDiaComponent;
  let service: jasmine.SpyObj<RepartidorService>;

  const pedido: PedidoRuta = {
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

    const offline = jasmine.createSpyObj<OfflineService>('OfflineService', [
      'iniciarSincronizacionAutomatica',
      'encolar',
    ]);

    await TestBed.configureTestingModule({
      imports: [RutaDiaComponent],
      providers: [
        provideNoopAnimations(),
        MessageService,
        ConfirmationService,
        { provide: RepartidorService, useValue: service },
        { provide: OfflineService, useValue: offline },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RutaDiaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('asocia la etiqueta al campo de motivo', () => {
    component.solicitarCambio(pedido, 'NoEntregado');
    fixture.detectChanges();

    const label = fixture.nativeElement.querySelector('label[for="motivo"]') as HTMLLabelElement;
    const textarea = fixture.nativeElement.querySelector('#motivo') as HTMLTextAreaElement;

    expect(label).toBeTruthy();
    expect(textarea).toBeTruthy();
  });

  it('guarda el motivo al presionar Enter', () => {
    component.solicitarCambio(pedido, 'NoEntregado');
    component.motivo = 'Cliente ausente';
    fixture.detectChanges();

    const textarea = fixture.nativeElement.querySelector('#motivo') as HTMLTextAreaElement;
    textarea.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));

    expect(service.cambiarEstado).toHaveBeenCalledWith(1, {
      estadoNuevo: 'NoEntregado',
      motivo: 'Cliente ausente',
    });
  });
});
