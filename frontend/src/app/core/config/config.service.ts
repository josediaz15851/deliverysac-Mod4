import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ConfigService {
  readonly apiUrl = environment.apiUrl;
  readonly defaultLocale = environment.defaultLocale;
}
