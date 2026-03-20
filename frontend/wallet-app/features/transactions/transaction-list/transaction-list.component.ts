import { Component, OnInit } from '@angular/core';
import { TransactionService } from '../../../core/services/transaction.service';
import { AuthService }        from '../../../core/services/auth.service';
import { Transaction }        from '../../../core/models/transaction.models';

@Component({
  selector:    'app-transaction-list',
  templateUrl: './transaction-list.component.html',
  styleUrls:   ['./transaction-list.component.scss']
})
export class TransactionListComponent implements OnInit {
  transactions: Transaction[] = [];
  loading = true;
  page    = 1;

  constructor(
    public  authService: AuthService,
    private txnSvc:      TransactionService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.txnSvc.getHistory(this.page, 20).subscribe({
      next: (res) => {
        this.transactions = res.data;
        this.loading      = false;
      },
      error: () => { this.loading = false; }
    });
  }

  isDebit(txn: Transaction): boolean {
    return txn.senderId === this.authService.currentUser?.id;
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
}