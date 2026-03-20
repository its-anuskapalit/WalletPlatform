import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

// Angular Material
import { MatSnackBarModule }         from '@angular/material/snack-bar';
import { MatProgressSpinnerModule }  from '@angular/material/progress-spinner';
import { MatTooltipModule }          from '@angular/material/tooltip';
import { MatMenuModule }             from '@angular/material/menu';
import { MatDialogModule }           from '@angular/material/dialog';

// Routing
import { AppRoutingModule } from './app-routing.module';
import { AppComponent }     from './app.component';

// Interceptors
import { JwtInterceptor }   from './core/interceptors/jwt.interceptor';
import { ErrorInterceptor } from './core/interceptors/error.interceptor';

// Shared components
import { NavbarComponent }  from './shared/components/navbar/navbar.component';
import { SidebarComponent } from './shared/components/sidebar/sidebar.component';

// Feature components
import { LoginComponent }           from './features/auth/login/login.component';
import { RegisterComponent }        from './features/auth/register/register.component';
import { DashboardComponent }       from './features/dashboard/dashboard.component';
import { WalletComponent }          from './features/wallet/wallet.component';
import { TransactionListComponent } from './features/transactions/transaction-list/transaction-list.component';
import { SendMoneyComponent }       from './features/transactions/send-money/send-money.component';
import { RewardsComponent }         from './features/rewards/rewards.component';

@NgModule({
  declarations: [
    AppComponent,
    NavbarComponent,
    SidebarComponent,
    LoginComponent,
    RegisterComponent,
    DashboardComponent,
    WalletComponent,
    TransactionListComponent,
    SendMoneyComponent,
    RewardsComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    AppRoutingModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatMenuModule,
    MatDialogModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor,   multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}