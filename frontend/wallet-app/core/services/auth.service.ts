import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import {
  AuthResponse, LoginRequest,
  RegisterRequest, UserProfile
} from '../models/auth.models';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data:    T;
  errors:  string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl   = `${environment.apiUrl}/auth`;
  private readonly userKey  = 'wallet_user';
  private readonly tokenKey = 'wallet_token';

  private currentUserSubject = new BehaviorSubject<UserProfile | null>(
    this.getUserFromStorage()
  );

  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  get currentUser(): UserProfile | null {
    return this.currentUserSubject.value;
  }

  get isLoggedIn(): boolean {
    return !!this.getToken();
  }

  get isAdmin(): boolean {
    return this.currentUser?.role === 'Admin';
  }

  register(request: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(
      `${this.apiUrl}/register`, request
    ).pipe(tap(res => {
      if (res.success) this.storeSession(res.data);
    }));
  }

  login(request: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(
      `${this.apiUrl}/login`, request
    ).pipe(tap(res => {
      if (res.success) this.storeSession(res.data);
    }));
  }

  logout(): void {
    const refreshToken = localStorage.getItem('wallet_refresh');
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/logout`, refreshToken).subscribe();
    }
    this.clearSession();
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  private storeSession(auth: AuthResponse): void {
    localStorage.setItem(this.tokenKey,    auth.accessToken);
    localStorage.setItem('wallet_refresh', auth.refreshToken);
    localStorage.setItem(this.userKey,     JSON.stringify(auth.user));
    this.currentUserSubject.next(auth.user);
  }

  private clearSession(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem('wallet_refresh');
    localStorage.removeItem(this.userKey);
    this.currentUserSubject.next(null);
  }

  private getUserFromStorage(): UserProfile | null {
    const stored = localStorage.getItem(this.userKey);
    return stored ? JSON.parse(stored) : null;
  }
}