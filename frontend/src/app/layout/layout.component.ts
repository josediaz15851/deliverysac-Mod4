import { Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MenuItem } from 'primeng/api';
import { TopbarComponent } from './topbar/topbar.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { FooterComponent } from './footer/footer.component';

type LayoutMode = 'app' | 'blank' | 'repartidor';

@Component({
  selector: 'app-layout',
  imports: [
    RouterOutlet,
    BreadcrumbModule,
    ToastModule,
    ConfirmDialogModule,
    TopbarComponent,
    SidebarComponent,
    FooterComponent,
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss',
})
export class LayoutComponent {
  private readonly router = inject(Router);

  readonly layoutMode = signal<LayoutMode>('app');
  readonly pageTitle = signal<string | null>(null);
  readonly breadcrumbItems = signal<MenuItem[]>([]);
  readonly home: MenuItem = { icon: 'pi pi-home', routerLink: '/' };

  readonly showSidebar = computed(() => this.layoutMode() === 'app');
  readonly showChrome = computed(() => this.layoutMode() !== 'blank');

  constructor() {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => this.syncFromRoute());

    this.syncFromRoute();
  }

  private syncFromRoute(): void {
    let route = this.router.routerState.root;
    while (route.firstChild) {
      route = route.firstChild;
    }

    const data = route.snapshot.data;
    this.layoutMode.set((data['layout'] as LayoutMode) ?? 'app');
    this.pageTitle.set((data['title'] as string) ?? null);
    this.breadcrumbItems.set((data['breadcrumb'] as MenuItem[]) ?? []);
  }
}
