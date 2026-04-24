export interface BrowseProduct {
  productId: string;
  listingId: string;
  categoryId: string;
  productName: string;
  description: string;
  categoryName: string | null;
  price: number;
  stockQuantity: number;
  productPhotosQty: number;
  productWeightG: number;
  productLengthCm: number;
  productHeightCm: number;
  productWidthCm: number;
}

export interface Category {
  id: string;
  categoryNamePt: string;
  categoryNameEn: string | null;
}

export type ProductSortOption = 'newest' | 'price-asc' | 'price-desc' | 'name-asc';
