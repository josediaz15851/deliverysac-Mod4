import { Component, Input } from '@angular/core';
import { TagModule } from 'primeng/tag';

type EstadoSeverity = 'success' | 'info' | 'warn' | 'danger' | 'secondary';

@Component({
  selector: 'app-estado-tag',
  imports: [TagModule],
  template: `<p-tag [value]="etiqueta" [severity]="severity"></p-tag>`,
})
export class EstadoTagComponent {
  @Input() estado = '';

  get severity(): EstadoSeverity {
    switch (this.estado) {
      case 'Entregado':
        return 'success';
      case 'Asignado':
      case 'EnRuta':
        return 'info';
      case 'NoEntregado':
        return 'danger';
      case 'Reprogramado':
        return 'secondary';
      default:
        return 'warn';
    }
  }

  get etiqueta(): string {
    switch (this.estado) {
      case 'EnRuta':
        return 'En ruta';
      case 'NoEntregado':
        return 'No entregado';
      default:
        return this.estado;
    }
  }
}
