import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ButtonModule } from 'primeng/button';
import { LayoutService } from '../layout.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-topbar',
  imports: [ButtonModule],
  template: `
    <header class="topbar">
      <div class="topbar__left">
        <button
          type="button"
          pButton
          text
          rounded
          icon="pi pi-bars"
          ariaLabel="Mostrar u ocultar menú"
          class="topbar__menu-button"
          (click)="toggle()"
        ></button>
        <span class="topbar__brand">DeliverySac</span>
      </div>

      <div class="topbar__right">
        @if (user(); as currentUser) {
          <div class="topbar__user">
            <i class="pi pi-user" aria-hidden="true"></i>
            <div class="topbar__user-data">
              <span class="topbar__user-name">{{ currentUser.nombreCompleto }}</span>
              <span class="topbar__user-role">{{ currentUser.rol }}</span>
            </div>
          </div>
          <button
            type="button"
            pButton
            text
            rounded
            icon="pi pi-sign-out"
            ariaLabel="Cerrar sesión"
            (click)="logout()"
          ></button>
        }
      </div>
    </header>
  `,
  styles: [
    `
      .topbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        height: 4rem;
        padding: 0 1rem;
        background: var(--surface-card);
        border-bottom: 1px solid var(--surface-border);
      }

      .topbar__left,
      .topbar__right,
      .topbar__user {
        display: flex;
        align-items: center;
        gap: 0.5rem;
      }

      .topbar__brand {
        font-size: 1.25rem;
        font-weight: 600;
        color: var(--primary-color);
      }

      .topbar__user {
        gap: 0.5rem;
      }

      .topbar__user-data {
        display: flex;
        flex-direction: column;
        line-height: 1.1;
      }

      .topbar__user-name {
        font-weight: 600;
      }

      .topbar__user-role {
        font-size: 0.75rem;
        color: var(--text-color-secondary);
      }
    `,
  ],
})
export class TopbarComponent {
  private readonly layoutService = inject(LayoutService);
  private readonly authService = inject(AuthService);

  readonly user = toSignal(this.authService.user$, { initialValue: null });

  toggle(): void {
    this.layoutService.toggleSidebar();
  }

  logout(): void {
    this.authService.logout();
  }
}
