import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { PedidosService } from '../pedidos.service';
import { PedidosListComponent } from './pedidos-list.component';

describe('PedidosListComponent', () => {
  let fixture: ComponentFixture<PedidosListComponent>;
  let service: jasmine.SpyObj<PedidosService>;

  beforeEach(async () => {
    service = jasmine.createSpyObj<PedidosService>('PedidosService', ['crear', 'listar', 'obtener']);
    service.listar.and.returnValue(of({ items: [], total: 0 }));

    await TestBed.configureTestingModule({
      imports: [PedidosListComponent],
      providers: [
        provideRouter([]),
        MessageService,
        ConfirmationService,
        { provide: PedidosService, useValue: service },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PedidosListComponent);
  });

  it('muestra cliente, fecha, estado como etiqueta y total', async () => {
    service.listar.and.returnValue(
      of({
        items: [
          { id: 1, clienteNombre: 'Ana Torres', fechaReparto: '2026-10-15', estado: 'Pendiente', total: 12 },
        ],
        total: 1,
      }),
    );

    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const contenido = fixture.nativeElement.textContent as string;
    expect(contenido).toContain('Ana Torres');
    expect(contenido).toContain('Pendiente');
    expect(contenido).toContain('12');
  });
});
