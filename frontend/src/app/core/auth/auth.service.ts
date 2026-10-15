import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ConfigService } from '../config/config.service';

export interface LoginRequest {
  nombreUsuario: string;
  password: string;
}

export interface AuthUser {
  id: number;
  nombreUsuario: string;
  nombreCompleto: string;
  rol: string;
}

export interface LoginResponse {
  token: string;
  usuario: AuthUser;
}

const TOKEN_KEY = 'deliverysac.token';
const USER_KEY = 'deliverysac.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);
  private readonly router = inject(Router);

  private readonly userSubject = new BehaviorSubject<AuthUser | null>(this.readStoredUser());
  readonly user$ = this.userSubject.asObservable();

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.config.apiUrl}/api/auth/login`, credentials)
      .pipe(tap((response) => this.setSession(response)));
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.userSubject.next(null);
    void this.router.navigate(['/login']);
  }

  get token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  get currentUser(): AuthUser | null {
    return this.userSubject.value;
  }

  isAuthenticated(): boolean {
    return this.token !== null;
  }

  hasRole(...roles: string[]): boolean {
    const user = this.currentUser;
    return user !== null && roles.includes(user.rol);
  }

  private setSession(response: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USER_KEY, JSON.stringify(response.usuario));
    this.userSubject.next(response.usuario);
  }

  private readStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as AuthUser;
    } catch {
      return null;
    }
  }
}
