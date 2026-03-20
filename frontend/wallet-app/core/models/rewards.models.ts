export interface LoyaltyAccount {
  id:               string;
  userId:           string;
  totalPoints:      number;
  lifetimePoints:   number;
  redeemedPoints:   number;
  tierName:         string;
  tierBadgeColor:   string;
  tierMultiplier:   number;
  pointsToNextTier: number;
  nextTierName:     string;
  createdAt:        string;
}

export interface PointTransaction {
  id:            string;
  type:          string;
  points:        number;
  balanceBefore: number;
  balanceAfter:  number;
  description:   string;
  referenceId?:  string;
  createdAt:     string;
}

export interface CatalogItem {
  id:          string;
  name:        string;
  description: string;
  category:    string;
  pointsCost:  number;
  imageUrl?:   string;
  brand?:      string;
  stockCount:  number;
  isActive:    boolean;
  validFrom:   string;
  validUntil?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data:    T;
  errors:  string[];
}