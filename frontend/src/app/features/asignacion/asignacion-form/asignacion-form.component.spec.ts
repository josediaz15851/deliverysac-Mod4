import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { AsignacionesService } from '../asignaciones.service';
import { AsignacionFormComponent } from './asignacion-form.component';

describe('AsignacionFormComponent', () => {
  let fixture: ComponentFixture<AsignacionFormComponent>;
  let component: AsignacionFormComponent;
  let service: jasmine.SpyObj<AsignacionesService>;

  beforeEach(async () => {
    service = jasmine.createSpyObj<AsignacionesService>('AsignacionesService', [
      'listarCandidatos',
      'listarRepartidores',
      'listarUnidades',
      'asignar',
    ]);
    service.listarCandidatos.and.returnValue(of({ items: [], total: 0 }));
    service.listarRepartidores.and.returnValue(of([{ id: 5, nombreCompleto: 'Luis', nombreUsuario: 'luis' }]));
    service.listarUnidades.and.returnValue(of([{ id: 3, identificador: 'MOTO-01', tipo: 'MotoLineal' }]));
    service.asignar.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [AsignacionFormComponent],
      providers: [
        provideRouter([]),
        MessageService,
        ConfirmationService,
        { provide: AsignacionesService, useValue: service },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AsignacionFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('carga los candidatos pendientes y reprogramados', () => {
    service.listarCandidatos.and.returnValue(
      of({
        items: [{ id: 1, clienteNombre: 'Ana', fechaReparto: '2026-10-15', estado: 'Pendiente', total: 12 }],
        total: 1,
      }),
    );

    component.cargarCandidatos();
    fixture.detectChanges();

    expect(component.candidatos.length).toBe(1);
    expect(component.candidatos[0].estado).toBe('Pendiente');
  });

  it('envía la asignación con repartidor y unidad seleccionados', () => {
    component.seleccionarPedido({ id: 10, clienteNombre: 'Ana', fechaReparto: '2026-10-15', estado: 'Pendiente', total: 12 });
    component.confirmarAsignacion({ repartidorId: 5, unidadId: 3 });

    expect(service.asignar).toHaveBeenCalledWith(10, { repartidorId: 5, unidadId: 3 });
  });
});
