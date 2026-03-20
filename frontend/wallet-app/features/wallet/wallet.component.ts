import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { WalletService } from '../../core/services/wallet.service';
import { Wallet } from '../../core/models/wallet.models';

@Component({
  selector:    'app-wallet',
  templateUrl: './wallet.component.html',
  styleUrls:   ['./wallet.component.scss']
})
export class WalletComponent implements OnInit {
  wallet:   Wallet | null = null;
  loading = true;
  amount  = 0;
  funding = false;

  constructor(
    private walletSvc: WalletService,
    private snack:     MatSnackBar
  ) {}

  ngOnInit(): void {
    this.walletSvc.getWallet().subscribe({
      next:  (res) => { this.wallet = res.data; this.loading = false; },
      error: ()    => { this.loading = false; }
    });
  }

  fund(): void {
    if (this.amount <= 0 || this.funding) return;
    this.funding = true;
    this.walletSvc.fundWallet({
      amount: this.amount, paymentMethodId: '', description: 'Manual top-up'
    }).subscribe({
      next: (res) => {
        this.wallet  = res.data;
        this.funding = false;
        this.amount  = 0;
        this.snack.open(`₹${res.data.balance} funded successfully!`, 'Close', {
          duration: 3000, panelClass: 'snack-success'
        });
      },
      error: (err) => {
        this.funding = false;
        this.snack.open(err.error?.message || 'Fund failed.', 'Close', {
          duration: 4000, panelClass: 'snack-error'
        });
      }
    });
  }
}