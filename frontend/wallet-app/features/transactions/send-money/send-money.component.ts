import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TransactionService } from '../../../core/services/transaction.service';

@Component({
  selector:    'app-send-money',
  templateUrl: './send-money.component.html',
  styleUrls:   ['./send-money.component.scss']
})
export class SendMoneyComponent {
  form:      FormGroup;
  loading =  false;
  success:   any = null;

  constructor(
    private fb:     FormBuilder,
    private txnSvc: TransactionService,
    private snack:  MatSnackBar
  ) {
    this.form = this.fb.group({
      recipientId:  ['', Validators.required],
      amount:       [null, [Validators.required, Validators.min(1)]],
      description:  ['', Validators.required]
    });
  }

  submit(): void {
    if (this.form.invalid || this.loading) return;
    this.loading = true;

    this.txnSvc.pay(this.form.value).subscribe({
      next: (res) => {
        this.loading = false;
        this.success = res.data;
        this.form.reset();
        this.snack.open('Payment sent successfully!', 'Close', {
          duration: 4000, panelClass: 'snack-success'
        });
      },
      error: (err) => {
        this.loading = false;
        const msg = err.error?.message || 'Payment failed. Please try again.';
        this.snack.open(msg, 'Close', {
          duration: 5000, panelClass: 'snack-error'
        });
      }
    });
  }

  get f() { return this.form.controls; }
}