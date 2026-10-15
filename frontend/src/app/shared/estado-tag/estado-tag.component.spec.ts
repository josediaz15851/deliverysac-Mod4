import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EstadoTagComponent } from './estado-tag.component';

describe('EstadoTagComponent', () => {
  let fixture: ComponentFixture<EstadoTagComponent>;
  let component: EstadoTagComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EstadoTagComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EstadoTagComponent);
    component = fixture.componentInstance;
  });

  it('asigna una severidad distinta por cada estado', () => {
    component.estado = 'Pendiente';
    expect(component.severity).toBe('warn');

    component.estado = 'Asignado';
    expect(component.severity).toBe('info');

    component.estado = 'EnRuta';
    expect(component.severity).toBe('info');

    component.estado = 'Entregado';
    expect(component.severity).toBe('success');

    component.estado = 'NoEntregado';
    expect(component.severity).toBe('danger');

    component.estado = 'Reprogramado';
    expect(component.severity).toBe('secondary');
  });

  it('muestra una etiqueta legible para cada estado', () => {
    component.estado = 'EnRuta';
    expect(component.etiqueta).toBe('En ruta');

    component.estado = 'NoEntregado';
    expect(component.etiqueta).toBe('No entregado');

    component.estado = 'Entregado';
    expect(component.etiqueta).toBe('Entregado');
  });
});
