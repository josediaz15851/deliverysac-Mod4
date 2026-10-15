import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../core/auth/auth.service';
import { LayoutService } from '../layout.service';
import { APP_MENU } from './menu.config';

@Component({
  selector: 'app-sidebar',
  imports: [MenuModule, RouterLink, RouterLinkActive],
  template: `
    <aside class="sidebar" [class.sidebar--collapsed]="collapsed()">
      <p-menu [model]="items()" styleClass="sidebar__menu">
        <ng-template let-item pTemplate="item">
          <a
            class="sidebar__link"
            [routerLink]="item.routerLink"
            routerLinkActive="sidebar__link--active"
          >
            <i [class]="item.icon" aria-hidden="true"></i>
            @if (!collapsed()) {
              <span class="sidebar__label">{{ item.label }}</span>
            }
          </a>
        </ng-template>
      </p-menu>
    </aside>
  `,
  styles: [
    `
      .sidebar {
        width: 16rem;
        padding: 1rem 0.5rem;
        background: var(--surface-card);
        border-right: 1px solid var(--surface-border);
        transition: width 0.2s ease;
        overflow-x: hidden;
      }

      .sidebar--collapsed {
        width: 4.5rem;
      }

      .sidebar__link {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.75rem;
        border-radius: var(--content-border-radius, 6px);
        color: var(--text-color);
        text-decoration: none;
      }

      .sidebar__link:hover {
        background: var(--surface-hover);
      }

      .sidebar__link--active {
        background: var(--primary-color);
        color: var(--primary-contrast-color, #ffffff);
      }
    `,
  ],
})
export class SidebarComponent {
  private readonly authService = inject(AuthService);
  private readonly layoutService = inject(LayoutService);

  readonly collapsed = this.layoutService.collapsed;

  readonly items = computed<MenuItem[]>(() => {
    const user = this.authService.currentUser;
    if (!user) {
      return [];
    }

    return APP_MENU.filter((item) => item.roles.includes(user.rol));
  });
}
