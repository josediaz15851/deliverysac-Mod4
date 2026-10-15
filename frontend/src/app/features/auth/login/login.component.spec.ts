import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { throwError } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { authInterceptor } from '../../../core/auth/auth.interceptor';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;
  let authService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    localStorage.clear();
    authService = jasmine.createSpyObj<AuthService>('AuthService', ['login']);

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        MessageService,
        ConfirmationService,
        { provide: AuthService, useValue: authService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('no llama al servicio y marca el formulario inválido cuando faltan datos', () => {
    component.submit();

    expect(authService.login).not.toHaveBeenCalled();
    expect(component.form.invalid).toBeTrue();
  });

  it('muestra un error inline cuando el login falla', () => {
    authService.login.and.returnValue(throwError(() => new HttpErrorResponse({ status: 401 })));
    component.form.setValue({ nombreUsuario: 'admin', password: 'incorrecta' });

    component.submit();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.login__error')).not.toBeNull();
  });

  it('envía las credenciales cuando el formulario es válido', () => {
    authService.login.and.returnValue(throwError(() => new HttpErrorResponse({ status: 401 })));
    component.form.setValue({ nombreUsuario: 'admin', password: 'secreta' });

    component.submit();

    expect(authService.login).toHaveBeenCalledWith({
      nombreUsuario: 'admin',
      password: 'secreta',
    });
  });
});

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('guarda el token y el usuario al iniciar sesión', () => {
    service.login({ nombreUsuario: 'admin', password: 'secreta' }).subscribe();

    const req = httpMock.expectOne((r) => r.url.endsWith('/api/auth/login'));
    req.flush({
      token: 'jwt-token',
      usuario: { id: 1, nombreUsuario: 'admin', nombreCompleto: 'Admin', rol: 'Administrador' },
    });

    expect(service.token).toBe('jwt-token');
    expect(service.currentUser?.rol).toBe('Administrador');
    expect(service.isAuthenticated()).toBeTrue();
  });
});

describe('authInterceptor', () => {
  let httpMock: HttpTestingController;
  let http: HttpClient;
  let authService: AuthService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
  });

  afterEach(() => httpMock.verify());

  it('cierra la sesión cuando recibe un 401', () => {
    spyOn(authService, 'logout');

    http.get('/api/recurso-protegido').subscribe({ error: () => undefined });
    const req = httpMock.expectOne('/api/recurso-protegido');
    req.flush('', { status: 401, statusText: 'Unauthorized' });

    expect(authService.logout).toHaveBeenCalled();
  });
});
