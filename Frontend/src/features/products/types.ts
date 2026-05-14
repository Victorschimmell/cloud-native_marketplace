import type { CurrencyCode } from '../../shared/currency/currency';

export interface BrowseProduct {
  productId: string;
  listingId: string;
  categoryId: string;
  productName: string;
  description: string;
  categoryName: string | null;
  price: number;
  currencyCode: ProductCurrencyCode;
  stockQuantity: number;
  averageReviewScore: number | null;
  reviewCount: number;
  productPhotosQty: number;
  productWeightG: number;
  productLengthCm: number;
  productHeightCm: number;
  productWidthCm: number;
}

export interface ProductDetails {
  productId: string;
  listingId: string;
  categoryId: string;
  productName: string;
  description: string;
  categoryName: string | null;
  price: number;
  currencyCode: ProductCurrencyCode;
  stockQuantity: number;
  productPhotosQty: number;
  productWeightG: number;
  productLengthCm: number;
  productHeightCm: number;
  productWidthCm: number;
  sellerId: string;
  sellerName: string;
  sellerVerificationStatus: string;
}

export interface ProductReview {
  id: string;
  orderId: string;
  orderItemId?: number | null;
  productId?: string | null;
  reviewerDisplayName?: string | null;
  reviewScore: number;
  reviewCommentTitle?: string | null;
  reviewCommentMessage?: string | null;
  reviewCreationDateUtc: string;
  reviewAnswerTimestampUtc?: string | null;
}

export interface Category {
  id: string;
  categoryNamePt: string;
  categoryNameEn: string | null;
}

export type ProductCurrencyCode = CurrencyCode;

export type ProductSortOption = 'newest' | 'price-asc' | 'price-desc' | 'name-asc';
