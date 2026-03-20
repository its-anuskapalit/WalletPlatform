import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Wallet, FundWalletRequest, ApiResponse } from '../models/wallet.models';

@Injectable({ providedIn: 'root' })
export class WalletService {
  private readonly apiUrl = `${environment.apiUrl}/wallet`;

  constructor(private http: HttpClient) {}

  getWallet(): Observable<ApiResponse<Wallet>> {
    return this.http.get<ApiResponse<Wallet>>(this.apiUrl);
  }

  fundWallet(request: FundWalletRequest): Observable<ApiResponse<Wallet>> {
    return this.http.post<ApiResponse<Wallet>>(`${this.apiUrl}/fund`, request);
  }

  withdraw(request: FundWalletRequest): Observable<ApiResponse<Wallet>> {
    return this.http.post<ApiResponse<Wallet>>(`${this.apiUrl}/withdraw`, request);
  }
}