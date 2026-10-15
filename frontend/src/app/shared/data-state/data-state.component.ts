import { Component, input } from '@angular/core';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-data-state',
  imports: [SkeletonModule],
  template: `
    @if (loading()) {
      <div class="data-state__skeleton" aria-busy="true" aria-live="polite">
        @for (row of skeletonRows; track row) {
          <p-skeleton height="2.5rem" styleClass="mb-2"></p-skeleton>
        }
      </div>
    } @else if (empty()) {
      <div class="data-state__empty" role="status">
        <i class="pi pi-inbox" aria-hidden="true"></i>
        <span>{{ emptyMessage() }}</span>
      </div>
    } @else {
      <ng-content></ng-content>
    }
  `,
  styles: [
    `
      .data-state__empty {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.5rem;
        padding: 2rem 1rem;
        color: var(--text-color-secondary);
        text-align: center;
      }

      .data-state__empty .pi {
        font-size: 2rem;
      }
    `,
  ],
})
export class DataStateComponent {
  readonly loading = input(false);
  readonly empty = input(false);
  readonly emptyMessage = input('No se encontraron registros');

  protected readonly skeletonRows = [0, 1, 2, 3, 4];
}
