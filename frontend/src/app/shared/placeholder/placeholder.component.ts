import { Component } from '@angular/core';

@Component({
  selector: 'app-placeholder',
  template: `
    <div class="placeholder">
      <i class="pi pi-wrench" aria-hidden="true"></i>
      <h2>Pantalla en construcción</h2>
      <p>Esta sección estará disponible próximamente.</p>
    </div>
  `,
  styles: [
    `
      .placeholder {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.5rem;
        padding: 3rem 1rem;
        color: var(--text-color-secondary);
        text-align: center;
      }

      .placeholder .pi {
        font-size: 2.5rem;
      }
    `,
  ],
})
export class PlaceholderComponent {}
