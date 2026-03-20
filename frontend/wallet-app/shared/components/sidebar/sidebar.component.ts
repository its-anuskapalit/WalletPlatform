import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:    'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls:   ['./sidebar.component.scss']
})
export class SidebarComponent {
  navItems = [
    { label: 'Dashboard',     icon: '⊞',  route: '/dashboard'    },
    { label: 'Wallet',        icon: '◈',  route: '/wallet'       },
    { label: 'Transactions',  icon: '⇄',  route: '/transactions' },
    { label: 'Send Money',    icon: '↗',  route: '/send-money'   },
    { label: 'Rewards',       icon: '✦',  route: '/rewards'      }
  ];

  constructor(public authService: AuthService, private router: Router) {}

  logout(): void {
    this.authService.logout();
  }
}