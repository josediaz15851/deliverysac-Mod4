import { ApplicationConfig, provideZoneChangeDetection, isDevMode, LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeEsPe from '@angular/common/locales/es-PE';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import { ConfirmationService, MessageService } from 'primeng/api';

import { routes } from './app.routes';
import { provideServiceWorker } from '@angular/service-worker';
import { DeliverySacPreset } from './../theme/deliverysac-preset';
import { authInterceptor } from './core/auth/auth.interceptor';

registerLocaleData(localeEsPe);

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    MessageService,
    ConfirmationService,
    { provide: LOCALE_ID, useValue: 'es-PE' },
    { provide: LOCALE_ID, useValue: 'es-PE' },
    providePrimeNG({ theme: { preset: DeliverySacPreset } }), provideServiceWorker('ngsw-worker.js', {
            enabled: !isDevMode(),
            registrationStrategy: 'registerWhenStable:30000'
          })
  ]
};
