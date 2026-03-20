export interface RegisterRequest {
  firstName:   string;
  lastName:    string;
  email:       string;
  password:    string;
  phoneNumber: string;
}

export interface LoginRequest {
  email:    string;
  password: string;
}

export interface AuthResponse {
  accessToken:  string;
  refreshToken: string;
  expiresAt:    string;
  user:         UserProfile;
}

export interface UserProfile {
  id:          string;
  email:       string;
  phoneNumber: string;
  fullName:    string;
  role:        string;
  isActive:    boolean;
  kycStatus:   string;
}

export interface KYCSubmitRequest {
  documentType:   string;
  documentNumber: string;
  documentUrl?:   string;
}