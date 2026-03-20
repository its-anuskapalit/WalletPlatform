import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector:    'app-register',
  templateUrl: './register.component.html',
  styleUrls:   ['./register.component.scss']
})
export class RegisterComponent {
  form:      FormGroup;
  loading =  false;
  showPass = false;

  constructor(
    private fb:          FormBuilder,
    private authService: AuthService,
    private router:      Router,
    private snackBar:    MatSnackBar
  ) {
    this.form = this.fb.group({
      firstName:   ['', Validators.required],
      lastName:    ['', Validators.required],
      email:       ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\+?[1-9]\d{9,14}$/)]],
      password:    ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])/)
      ]]
    });
  }

  submit(): void {
    if (this.form.invalid || this.loading) return;
    this.loading = true;

    this.authService.register(this.form.value).subscribe({
      next: () => {
        this.snackBar.open('Account created! Wallet is being set up.', 'Close', {
          duration: 4000, panelClass: 'snack-success'
        });
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        const msg = err.error?.message || 'Registration failed. Please try again.';
        this.snackBar.open(msg, 'Close', {
          duration: 4000, panelClass: 'snack-error'
        });
      }
    });
  }

  get f() { return this.form.controls; }
}