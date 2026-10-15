import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MessageService } from 'primeng/api';
import { ConfigService } from '../config/config.service';
import { InMemoryOfflineStore, OfflineStore } from './offline.models';
import { OfflineService } from './offline.service';

describe('OfflineService', () => {
  let service: OfflineService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        MessageService,
        { provide: OfflineStore, useClass: InMemoryOfflineStore },
        { provide: ConfigService, useValue: { apiUrl: 'http://test' } },
      ],
    });

    service = TestBed.inject(OfflineService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('encola un cambio con un operacionId persistido', async () => {
    const operacionId = await service.encolar({ pedidoId: 1, estadoNuevo: 'EnRuta' });

    const pendientes = await service.pendientes();

    expect(pendientes.length).toBe(1);
    expect(pendientes[0].operacionId).toBe(operacionId);
    expect(pendientes[0].estadoNuevo).toBe('EnRuta');
    expect(pendientes[0].encoladaEn).toBeTruthy();
  });

  it('no duplica operaciones con el mismo operacionId (idempotencia)', async () => {
    await service.encolar({ pedidoId: 1, estadoNuevo: 'EnRuta' }, 'op-1');
    await service.encolar({ pedidoId: 1, estadoNuevo: 'EnRuta' }, 'op-1');

    expect((await service.pendientes()).length).toBe(1);
  });

  it('sincroniza las operaciones pendientes y las elimina al confirmar', async () => {
    await service.encolar({ pedidoId: 7, estadoNuevo: 'Entregado' }, 'op-9');

    const sincronizacion = service.sincronizar();

    const req = httpMock.expectOne('http://test/api/pedidos/7/estados');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ operacionId: 'op-9', estadoNuevo: 'Entregado' });
    req.flush(null);

    await sincronizacion;

    expect((await service.pendientes()).length).toBe(0);
  });

  it('descarta la operación y notifica cuando el servidor reporta conflicto', async () => {
    const messageService = TestBed.inject(MessageService);
    const addSpy = spyOn(messageService, 'add');
    await service.encolar({ pedidoId: 5, estadoNuevo: 'Entregado' }, 'op-c');

    const sincronizacion = service.sincronizar();

    const req = httpMock.expectOne('http://test/api/pedidos/5/estados');
    req.flush({ detail: 'Estado desactualizado' }, { status: 409, statusText: 'Conflict' });

    await sincronizacion;

    expect((await service.pendientes()).length).toBe(0);
    expect(addSpy).toHaveBeenCalled();
  });
});
