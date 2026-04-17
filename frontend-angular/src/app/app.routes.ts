import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'products'
  },
  {
    path: 'products',
    loadComponent: () =>
      import('./pages/products/products-page.component').then((m) => m.ProductsPageComponent)
  },
  {
    path: 'invoices',
    loadComponent: () =>
      import('./pages/invoices/invoices-page.component').then((m) => m.InvoicesPageComponent)
  },
  {
    path: 'invoices/:id',
    loadComponent: () =>
      import('./pages/invoice-detail/invoice-detail-page.component').then(
        (m) => m.InvoiceDetailPageComponent
      )
  },
  {
    path: '**',
    redirectTo: 'products'
  }
];
