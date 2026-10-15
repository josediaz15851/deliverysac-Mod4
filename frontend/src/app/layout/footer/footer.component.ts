import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  template: `
    <footer class="footer">
      <span>DeliverySac &copy; {{ year }} - Reparto y trazabilidad de pedidos</span>
    </footer>
  `,
  styles: [
    `
      .footer {
        display: flex;
        justify-content: center;
        padding: 0.75rem 1rem;
        font-size: 0.85rem;
        color: var(--text-color-secondary);
        border-top: 1px solid var(--surface-border);
        background: var(--surface-card);
      }
    `,
  ],
})
export class FooterComponent {
  protected readonly year = new Date().getFullYear();
}
