import { Injectable } from '@angular/core';

export interface OperacionPendiente {
  operacionId: string;
  pedidoId: number;
  estadoNuevo: string;
  motivo?: string;
  encoladaEn: string;
}

export abstract class OfflineStore {
  abstract add(operacion: OperacionPendiente): Promise<void>;
  abstract getAll(): Promise<OperacionPendiente[]>;
  abstract remove(operacionId: string): Promise<void>;
  abstract clear(): Promise<void>;
}

@Injectable()
export class InMemoryOfflineStore extends OfflineStore {
  private readonly items = new Map<string, OperacionPendiente>();

  async add(operacion: OperacionPendiente): Promise<void> {
    this.items.set(operacion.operacionId, { ...operacion });
  }

  async getAll(): Promise<OperacionPendiente[]> {
    return [...this.items.values()];
  }

  async remove(operacionId: string): Promise<void> {
    this.items.delete(operacionId);
  }

  async clear(): Promise<void> {
    this.items.clear();
  }
}
