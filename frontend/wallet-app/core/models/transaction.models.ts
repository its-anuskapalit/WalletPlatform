export interface Transaction {
  id:            string;
  senderId:      string;
  recipientId:   string;
  amount:        number;
  currency:      string;
  type:          string;
  status:        string;
  description:   string;
  failureReason?: string;
  createdAt:     string;
  completedAt?:  string;
  ledgerEntries: LedgerEntry[];
}

export interface LedgerEntry {
  id:            string;
  accountId:     string;
  entryType:     string;
  amount:        number;
  balanceBefore: number;
  balanceAfter:  number;
  description:   string;
  createdAt:     string;
}

export interface PaymentRequest {
  recipientId:  string;
  amount:       number;
  description:  string;
  referenceId?: string;
}