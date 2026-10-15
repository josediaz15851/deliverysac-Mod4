import { ApplicationConfig, provideZoneChangeDetection, provideAppInitializer, isDevMode, LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeEsPe from '@angular/common/locales/es-PE';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import { ConfirmationService, MessageService } from 'primeng/api';

import { routes } from './app.routes';
import { provideServiceWorker } from '@angular/service-worker';
import { DeliverySacPreset } from './../theme/deliverysac-preset';
import { authInterceptor } from './core/auth/auth.interceptor';
import { OfflineStore } from './core/offline/offline.models';
import { IndexedDbOfflineStore } from './core/offline/indexeddb-offline.store';
import { disableServiceWorkerInDev } from './core/offline/service-worker-cleanup';

registerLocaleData(localeEsPe);

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAppInitializer(() => disableServiceWorkerInDev()),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimationsAsync(),
    providePrimeNG({ theme: { preset: DeliverySacPreset } }),
    MessageService,
    ConfirmationService,
    { provide: LOCALE_ID, useValue: 'es-PE' },
    { provide: OfflineStore, useClass: IndexedDbOfflineStore },
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
  ],
};
