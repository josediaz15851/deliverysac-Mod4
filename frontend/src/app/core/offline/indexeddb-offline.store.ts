import { Injectable } from '@angular/core';
import { OfflineStore, OperacionPendiente } from './offline.models';

const DB_NAME = 'deliverysac-offline';
const DB_VERSION = 1;
const STORE_NAME = 'operaciones';

@Injectable()
export class IndexedDbOfflineStore extends OfflineStore {
  private dbPromise?: Promise<IDBDatabase>;

  private open(): Promise<IDBDatabase> {
    if (!this.dbPromise) {
      this.dbPromise = new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);
        request.onupgradeneeded = () => {
          const db = request.result;
          if (!db.objectStoreNames.contains(STORE_NAME)) {
            db.createObjectStore(STORE_NAME, { keyPath: 'operacionId' });
          }
        };
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
      });
    }

    return this.dbPromise;
  }

  private async mutate(action: (store: IDBObjectStore) => void): Promise<void> {
    const db = await this.open();
    return new Promise((resolve, reject) => {
      const transaction = db.transaction(STORE_NAME, 'readwrite');
      action(transaction.objectStore(STORE_NAME));
      transaction.oncomplete = () => resolve();
      transaction.onerror = () => reject(transaction.error);
      transaction.onabort = () => reject(transaction.error);
    });
  }

  private async readAll(): Promise<OperacionPendiente[]> {
    const db = await this.open();
    return new Promise((resolve, reject) => {
      const request = db
        .transaction(STORE_NAME, 'readonly')
        .objectStore(STORE_NAME)
        .getAll();
      request.onsuccess = () => resolve(request.result as OperacionPendiente[]);
      request.onerror = () => reject(request.error);
    });
  }

  async add(operacion: OperacionPendiente): Promise<void> {
    await this.mutate((store) => store.put({ ...operacion }));
  }

  getAll(): Promise<OperacionPendiente[]> {
    return this.readAll();
  }

  async remove(operacionId: string): Promise<void> {
    await this.mutate((store) => store.delete(operacionId));
  }

  async clear(): Promise<void> {
    await this.mutate((store) => store.clear());
  }
}
