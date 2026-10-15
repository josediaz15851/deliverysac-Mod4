import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { Cliente } from '../clientes.service';

@Component({
  selector: 'app-cliente-form',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule],
  templateUrl: './cliente-form.component.html',
  styles: [
    `
      .cliente-form {
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .cliente-form .field {
        display: flex;
        flex-direction: column;
        gap: 0.35rem;
      }

      .cliente-form__error {
        color: var(--red-500, #d32f2f);
      }

      .cliente-form__actions {
        display: flex;
        justify-content: flex-end;
        gap: 0.5rem;
      }
    `,
  ],
})
export class ClienteFormComponent {
  private readonly formBuilder = inject(FormBuilder);

  @Input()
  set cliente(value: Cliente | null) {
    if (value) {
      this.form.patchValue({ nombre: value.nombre, direccion: value.direccion });
    } else {
      this.form.reset();
    }
  }

  @Output() readonly guardado = new EventEmitter<Omit<Cliente, 'id'>>();
  @Output() readonly cancelado = new EventEmitter<void>();

  readonly form = this.formBuilder.nonNullable.group({
    nombre: ['', [Validators.required]],
    direccion: ['', [Validators.required]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.guardado.emit(this.form.getRawValue());
  }

  cancelar(): void {
    this.cancelado.emit();
  }
}
