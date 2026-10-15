import { MenuItem } from 'primeng/api';

export interface AppMenuItem extends MenuItem {
  roles: string[];
}

export const APP_MENU: AppMenuItem[] = [
  {
    label: 'Clientes',
    icon: 'pi pi-users',
    routerLink: ['/clientes'],
    roles: ['Administrador'],
  },
  {
    label: 'Pedidos',
    icon: 'pi pi-shopping-cart',
    routerLink: ['/pedidos'],
    roles: ['Administrador'],
  },
  {
    label: 'Nuevo pedido',
    icon: 'pi pi-plus-circle',
    routerLink: ['/pedidos/nuevo'],
    roles: ['Administrador'],
  },
  {
    label: 'Asignación',
    icon: 'pi pi-send',
    routerLink: ['/asignacion'],
    roles: ['Administrador'],
  },
  {
    label: 'Panel del día',
    icon: 'pi pi-calendar',
    routerLink: ['/panel-dia'],
    roles: ['Administrador', 'Supervisor'],
  },
  {
    label: 'Mi ruta',
    icon: 'pi pi-truck',
    routerLink: ['/ruta'],
    roles: ['Repartidor'],
  },
];
