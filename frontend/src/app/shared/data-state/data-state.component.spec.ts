import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DataStateComponent } from './data-state.component';

describe('DataStateComponent', () => {
  let fixture: ComponentFixture<DataStateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataStateComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DataStateComponent);
  });

  it('muestra el skeleton durante la carga', () => {
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.data-state__skeleton')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('.data-state__empty')).toBeNull();
  });

  it('muestra el estado vacío cuando no hay datos', () => {
    fixture.componentRef.setInput('loading', false);
    fixture.componentRef.setInput('empty', true);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.data-state__empty')).not.toBeNull();
    expect(fixture.nativeElement.textContent).toContain('No se encontraron registros');
  });

  it('proyecta el contenido cuando hay datos', () => {
    fixture.componentRef.setInput('loading', false);
    fixture.componentRef.setInput('empty', false);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.data-state__skeleton')).toBeNull();
    expect(fixture.nativeElement.querySelector('.data-state__empty')).toBeNull();
  });
});
