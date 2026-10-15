import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../core/auth/auth.service';

interface RolTab {
  label: string;
  value: string;
  usuario: string;
  icon: string;
}

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, PasswordModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  readonly roles: RolTab[] = [
    { label: 'Administrador', value: 'Administrador', usuario: 'admin', icon: 'pi pi-cog' },
    { label: 'Repartidor', value: 'Repartidor', usuario: 'repartidor', icon: 'pi pi-truck' },
    { label: 'Supervisor', value: 'Supervisor', usuario: 'supervisor', icon: 'pi pi-eye' },
  ];

  readonly form = this.formBuilder.nonNullable.group({
    nombreUsuario: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  activeRole = this.roles[0];
  errorMessage: string | null = null;
  loading = false;

  selectRole(role: RolTab): void {
    this.activeRole = role;
    this.form.controls.nombreUsuario.setValue(role.usuario);
  }

  submit(): void {
    this.errorMessage = null;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Bienvenido',
          detail: 'Sesión iniciada correctamente',
        });
        void this.router.navigate(['/']);
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Usuario o contraseña incorrectos';
        this.messageService.add({
          severity: 'error',
          summary: 'No se pudo iniciar sesión',
          detail: 'Usuario o contraseña incorrectos',
        });
      },
    });
  }
}
