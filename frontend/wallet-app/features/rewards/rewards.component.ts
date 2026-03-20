import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RewardsService }  from '../../core/services/rewards.service';
import { LoyaltyAccount, PointTransaction, CatalogItem } from '../../core/models/rewards.models';

@Component({
  selector:    'app-rewards',
  templateUrl: './rewards.component.html',
  styleUrls:   ['./rewards.component.scss']
})
export class RewardsComponent implements OnInit {
  loyalty:  LoyaltyAccount | null  = null;
  history:  PointTransaction[]     = [];
  catalog:  CatalogItem[]          = [];
  loading = true;
  redeeming: string | null         = null;

  constructor(
    private rewardsSvc: RewardsService,
    private snack:      MatSnackBar
  ) {}

  ngOnInit(): void {
    forkJoin({
      account: this.rewardsSvc.getAccount(),
      history: this.rewardsSvc.getHistory(1, 10),
      catalog: this.rewardsSvc.getCatalog()
    }).subscribe({
      next: (res) => {
        this.loyalty = res.account.data;
        this.history = res.history.data;
        this.catalog = res.catalog.data;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  canRedeem(item: CatalogItem): boolean {
    return (this.loyalty?.totalPoints ?? 0) >= item.pointsCost;
  }

  redeem(item: CatalogItem): void {
    if (!this.canRedeem(item) || this.redeeming) return;
    this.redeeming = item.id;

    this.rewardsSvc.redeem(item.id).subscribe({
      next: () => {
        this.redeeming = null;
        this.snack.open(`Redeemed "${item.name}" successfully!`, 'Close', {
          duration: 4000, panelClass: 'snack-success'
        });
        // Refresh account
        this.rewardsSvc.getAccount().subscribe(res => this.loyalty = res.data);
      },
      error: (err) => {
        this.redeeming = null;
        const msg = err.error?.message || 'Redemption failed.';
        this.snack.open(msg, 'Close', {
          duration: 4000, panelClass: 'snack-error'
        });
      }
    });
  }

  getTierProgress(): number {
    if (!this.loyalty) return 0;
    const curr = this.loyalty.lifetimePoints;
    const next = this.loyalty.pointsToNextTier;
    if (next === 0) return 100;
    return Math.min(100, Math.round((curr / (curr + next)) * 100));
  }
}