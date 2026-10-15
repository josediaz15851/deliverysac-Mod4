import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { ClientesService } from '../clientes.service';
import { ClientesListComponent } from './clientes-list.component';
import { ClienteFormComponent } from '../cliente-form/cliente-form.component';

describe('ClientesListComponent', () => {
  let fixture: ComponentFixture<ClientesListComponent>;
  let service: jasmine.SpyObj<ClientesService>;

  beforeEach(async () => {
    service = jasmine.createSpyObj<ClientesService>('ClientesService', ['listar', 'buscar', 'crear', 'actualizar']);
    service.listar.and.returnValue(of({ items: [], total: 0 }));

    await TestBed.configureTestingModule({
      imports: [ClientesListComponent],
      providers: [
        provideRouter([]),
        MessageService,
        ConfirmationService,
        { provide: ClientesService, useValue: service },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ClientesListComponent);
  });

  it('carga y muestra los clientes en la tabla', async () => {
    service.listar.and.returnValue(
      of({ items: [{ id: 1, nombre: 'Ana Torres', direccion: 'Calle 1' }], total: 1 }),
    );

    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(service.listar).toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Ana Torres');
  });
});

describe('ClienteFormComponent', () => {
  let fixture: ComponentFixture<ClienteFormComponent>;
  let component: ClienteFormComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClienteFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ClienteFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('no emite el guardado si el formulario es inválido', () => {
    const emitSpy = spyOn(component.guardado, 'emit');

    component.submit();

    expect(emitSpy).not.toHaveBeenCalled();
    expect(component.form.invalid).toBeTrue();
  });

  it('emite el cliente cuando los datos son válidos', () => {
    const emitSpy = spyOn(component.guardado, 'emit');
    component.form.setValue({ nombre: 'Ana', direccion: 'Calle 1' });

    component.submit();

    expect(emitSpy).toHaveBeenCalledWith({ nombre: 'Ana', direccion: 'Calle 1' });
  });
});
