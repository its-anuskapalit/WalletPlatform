import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

import { LoginComponent }           from './features/auth/login/login.component';
import { RegisterComponent }        from './features/auth/register/register.component';
import { DashboardComponent }       from './features/dashboard/dashboard.component';
import { WalletComponent }          from './features/wallet/wallet.component';
import { TransactionListComponent } from './features/transactions/transaction-list/transaction-list.component';
import { SendMoneyComponent }       from './features/transactions/send-money/send-money.component';
import { RewardsComponent }         from './features/rewards/rewards.component';

const routes: Routes = [
  { path: '',              redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'auth/login',    component: LoginComponent },
  { path: 'auth/register', component: RegisterComponent },
  {
    path:      'dashboard',
    component: DashboardComponent,
    canActivate: [AuthGuard]
  },
  {
    path:      'wallet',
    component: WalletComponent,
    canActivate: [AuthGuard]
  },
  {
    path:      'transactions',
    component: TransactionListComponent,
    canActivate: [AuthGuard]
  },
  {
    path:      'send-money',
    component: SendMoneyComponent,
    canActivate: [AuthGuard]
  },
  {
    path:      'rewards',
    component: RewardsComponent,
    canActivate: [AuthGuard]
  },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}