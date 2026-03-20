export interface Wallet {
  id:               string;
  userId:           string;
  walletNumber:     string;
  balance:          number;
  frozenAmount:     number;
  availableBalance: number;
  currency:         string;
  status:           string;
  createdAt:        string;
  paymentMethods:   PaymentMethod[];
}

export interface PaymentMethod {
  id:          string;
  type:        string;
  displayName: string;
  last4Digits?: string;
  bankName?:   string;
  upiId?:      string;
  isDefault:   boolean;
}

export interface FundWalletRequest {
  amount:          number;
  paymentMethodId: string;
  description:     string;
}