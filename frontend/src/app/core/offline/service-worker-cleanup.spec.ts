import { clearLegacyServiceWorker } from './service-worker-cleanup';

describe('clearLegacyServiceWorker', () => {
  it('desregistra los service workers y limpia las cachés obsoletas', async () => {
    const unregister = jasmine.createSpy('unregister').and.returnValue(Promise.resolve(true));
    const registration = {
      getRegistrations: jasmine
        .createSpy('getRegistrations')
        .and.returnValue(Promise.resolve([{ unregister }, { unregister }])),
    };

    const cacheStorage = {
      keys: jasmine.createSpy('keys').and.returnValue(Promise.resolve(['v1', 'v2'])),
      delete: jasmine.createSpy('delete').and.returnValue(Promise.resolve(true)),
    };

    await clearLegacyServiceWorker(
      registration as unknown as Pick<ServiceWorkerContainer, 'getRegistrations'>,
      cacheStorage as unknown as Pick<CacheStorage, 'keys' | 'delete'>,
    );

    expect(unregister).toHaveBeenCalledTimes(2);
    expect(cacheStorage.delete).toHaveBeenCalledWith('v1');
    expect(cacheStorage.delete).toHaveBeenCalledWith('v2');
  });
});
