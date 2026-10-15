import { Routes } from '@angular/router';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
    data: { layout: 'blank' },
  },
  {
    path: '',
    canActivate: [roleGuard('Administrador', 'Repartidor', 'Supervisor')],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
      },
      {
        path: 'clientes',
        canActivate: [roleGuard('Administrador')],
        loadComponent: () =>
          import('./features/clientes/clientes-list/clientes-list.component').then(
            (m) => m.ClientesListComponent,
          ),
        data: { title: 'Clientes', breadcrumb: [{ label: 'Clientes' }] },
      },
      {
        path: 'pedidos',
        canActivate: [roleGuard('Administrador')],
        loadComponent: () =>
          import('./features/pedidos/pedidos-list/pedidos-list.component').then(
            (m) => m.PedidosListComponent,
          ),
        data: { title: 'Pedidos', breadcrumb: [{ label: 'Pedidos' }] },
      },
      {
        path: 'pedidos/nuevo',
        canActivate: [roleGuard('Administrador')],
        loadComponent: () =>
          import('./features/pedidos/pedido-form/pedido-form.component').then(
            (m) => m.PedidoFormComponent,
          ),
        data: {
          title: 'Nuevo pedido',
          breadcrumb: [{ label: 'Pedidos', routerLink: '/pedidos' }, { label: 'Nuevo' }],
        },
      },
      {
        path: 'asignacion',
        canActivate: [roleGuard('Administrador')],
        loadComponent: () =>
          import('./features/asignacion/asignacion-form/asignacion-form.component').then(
            (m) => m.AsignacionFormComponent,
          ),
        data: { title: 'Asignación', breadcrumb: [{ label: 'Asignación' }] },
      },
      {
        path: 'panel-dia',
        canActivate: [roleGuard('Administrador', 'Supervisor')],
        loadComponent: () =>
          import('./features/listado-dia/listado-dia.component').then((m) => m.ListadoDiaComponent),
        data: { title: 'Panel del día', breadcrumb: [{ label: 'Panel del día' }] },
      },
      {
        path: 'ruta',
        canActivate: [roleGuard('Repartidor')],
        loadComponent: () =>
          import('./features/repartidor/ruta-dia/ruta-dia.component').then(
            (m) => m.RutaDiaComponent,
          ),
        data: { title: 'Mi ruta', layout: 'repartidor' },
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
