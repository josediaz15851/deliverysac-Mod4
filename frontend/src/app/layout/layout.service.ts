import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LayoutService {
  private readonly collapsedState = signal(false);
  private readonly mobileOpenState = signal(false);

  readonly collapsed = this.collapsedState.asReadonly();
  readonly mobileOpen = this.mobileOpenState.asReadonly();

  toggleSidebar(): void {
    this.collapsedState.update((value) => !value);
  }

  toggleMobileSidebar(): void {
    this.mobileOpenState.update((value) => !value);
  }

  closeMobileSidebar(): void {
    this.mobileOpenState.set(false);
  }
}
