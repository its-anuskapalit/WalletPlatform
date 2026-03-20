import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LoyaltyAccount, PointTransaction,
  CatalogItem, ApiResponse
} from '../models/rewards.models';

@Injectable({ providedIn: 'root' })
export class RewardsService {
  private readonly rewardsUrl = `${environment.apiUrl}/rewards`;
  private readonly catalogUrl = `${environment.apiUrl}/catalog`;
  private readonly redeemUrl  = `${environment.apiUrl}/redemption`;

  constructor(private http: HttpClient) {}

  getAccount(): Observable<ApiResponse<LoyaltyAccount>> {
    return this.http.get<ApiResponse<LoyaltyAccount>>(this.rewardsUrl);
  }

  getHistory(page = 1, pageSize = 20): Observable<ApiResponse<PointTransaction[]>> {
    return this.http.get<ApiResponse<PointTransaction[]>>(
      `${this.rewardsUrl}/history?page=${page}&pageSize=${pageSize}`
    );
  }

  getCatalog(): Observable<ApiResponse<CatalogItem[]>> {
    return this.http.get<ApiResponse<CatalogItem[]>>(this.catalogUrl);
  }

  redeem(catalogItemId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(this.redeemUrl, { catalogItemId });
  }
}