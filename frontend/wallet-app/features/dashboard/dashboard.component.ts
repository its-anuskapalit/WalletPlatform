import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { WalletService }      from '../../core/services/wallet.service';
import { TransactionService } from '../../core/services/transaction.service';
import { RewardsService }     from '../../core/services/rewards.service';
import { AuthService }        from '../../core/services/auth.service';
import { Wallet }             from '../../core/models/wallet.models';
import { Transaction }        from '../../core/models/transaction.models';
import { LoyaltyAccount }     from '../../core/models/rewards.models';

@Component({
  selector:    'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls:   ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  wallet:       Wallet | null       = null;
  transactions: Transaction[]       = [];
  loyalty:      LoyaltyAccount | null = null;
  loading =     true;

  constructor(
    public  authService:  AuthService,
    private walletSvc:    WalletService,
    private txnSvc:       TransactionService,
    private rewardsSvc:   RewardsService
  ) {}

  ngOnInit(): void {
    forkJoin({
      wallet:       this.walletSvc.getWallet(),
      transactions: this.txnSvc.getHistory(1, 5),
      loyalty:      this.rewardsSvc.getAccount()
    }).subscribe({
      next: (results) => {
        this.wallet       = results.wallet.data;
        this.transactions = results.transactions.data;
        this.loyalty      = results.loyalty.data;
        this.loading      = false;
      },
      error: () => { this.loading = false; }
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Completed': 'badge-success',
      'Pending':   'badge-warning',
      'Failed':    'badge-danger',
      'Reversed':  'badge-default'
    };
    return map[status] ?? 'badge-default';
  }

  getTierProgress(): number {
    if (!this.loyalty) return 0;
    const current = this.loyalty.lifetimePoints;
    const needed  = this.loyalty.pointsToNextTier;
    if (needed === 0) return 100;
    const tierMin = current - (needed > 0 ? 0 : 0);
    return Math.min(100, Math.round((current / (current + needed)) * 100));
  }
}