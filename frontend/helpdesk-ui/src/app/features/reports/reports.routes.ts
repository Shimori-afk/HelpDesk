import { Routes } from '@angular/router';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

export const reportsRoutes: Routes = [
  {
    path: '',
    providers: [provideCharts(withDefaultRegisterables())],
    loadComponent: () => import('./reports').then((m) => m.Reports)
  }
];
