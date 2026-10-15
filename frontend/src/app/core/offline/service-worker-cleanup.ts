import { isDevMode } from '@angular/core';

export async function clearLegacyServiceWorker(
  registration: Pick<ServiceWorkerContainer, 'getRegistrations'>,
  cacheStorage: Pick<CacheStorage, 'keys' | 'delete'>,
): Promise<void> {
  const registraciones = await registration.getRegistrations();
  await Promise.all(registraciones.map((registro) => registro.unregister()));

  const claves = await cacheStorage.keys();
  await Promise.all(claves.map((clave) => cacheStorage.delete(clave)));
}

export function disableServiceWorkerInDev(): Promise<void> {
  if (!isDevMode() || typeof navigator === 'undefined' || !('serviceWorker' in navigator)) {
    return Promise.resolve();
  }

  return clearLegacyServiceWorker(navigator.serviceWorker, caches);
}
