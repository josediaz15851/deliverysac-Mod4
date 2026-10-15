import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { PedidosService } from '../features/pedidos/pedidos.service';
import { ListadoDiaComponent } from '../features/listado-dia/listado-dia.component';

describe('Responsive 320px', () => {
  it('el panel del día no desborda el ancho de 320px', async () => {
    const service = jasmine.createSpyObj<PedidosService>('PedidosService', ['listarDia']);
    service.listarDia.and.returnValue(
      of({
        items: [
          {
            id: 1,
            clienteNombre: 'Ana Torres',
            direccion: 'Av. Muy Larga 1234, Distrito Extenso, Ciudad',
            fechaReparto: '2026-10-15',
            estado: 'Reprogramado',
            motivo: 'El cliente pidió otro día',
            fechaHoraCambio: '2026-10-15T10:30:00-05:00',
            repartidorNombre: 'repartidor',
          },
        ],
        total: 1,
      }),
    );

    await TestBed.configureTestingModule({
      imports: [ListadoDiaComponent],
      providers: [provideNoopAnimations(), MessageService, { provide: PedidosService, useValue: service }],
    }).compileComponents();

    const fixture: ComponentFixture<ListadoDiaComponent> = TestBed.createComponent(ListadoDiaComponent);
    const host = fixture.nativeElement as HTMLElement;
    host.style.width = '320px';
    host.style.display = 'block';
    fixture.detectChanges();
    fixture.detectChanges();

    expect(host.scrollWidth).toBeLessThanOrEqual(321);

    fixture.destroy();
  });
});
