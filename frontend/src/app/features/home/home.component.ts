import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-home',
  template: ``,
})
export class HomeComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  constructor() {
    const rol = this.authService.currentUser?.rol;

    if (rol === 'Repartidor') {
      void this.router.navigate(['/ruta']);
    } else {
      void this.router.navigate(['/panel-dia']);
    }
  }
}
