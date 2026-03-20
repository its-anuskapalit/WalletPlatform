import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Transaction, PaymentRequest, LedgerEntry, ApiResponse } from '../models/transaction.models';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private readonly apiUrl = `${environment.apiUrl}/transactions`;

  constructor(private http: HttpClient) {}

  pay(request: PaymentRequest): Observable<ApiResponse<Transaction>> {
    const idempotencyKey = `pay-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    const headers = new HttpHeaders({ 'Idempotency-Key': idempotencyKey });
    return this.http.post<ApiResponse<Transaction>>(
      `${this.apiUrl}/pay`, request, { headers }
    );
  }

  getHistory(page = 1, pageSize = 20): Observable<ApiResponse<Transaction[]>> {
    return this.http.get<ApiResponse<Transaction[]>>(
      `${this.apiUrl}/history?page=${page}&pageSize=${pageSize}`
    );
  }

  getTransaction(id: string): Observable<ApiResponse<Transaction>> {
    return this.http.get<ApiResponse<Transaction>>(`${this.apiUrl}/${id}`);
  }

  getLedger(page = 1, pageSize = 20): Observable<ApiResponse<LedgerEntry[]>> {
    return this.http.get<ApiResponse<LedgerEntry[]>>(
      `${this.apiUrl}/ledger?page=${page}&pageSize=${pageSize}`
    );
  }
}