import type { Category, ProductSortOption } from '../types';
import './ProductFilters.css';

interface ProductFiltersProps {
  categories: Category[];
  categoryFilter: string;
  searchTerm: string;
  sortOption: ProductSortOption;
  onCategoryChange: (value: string) => void;
  onSearchChange: (value: string) => void;
  onSortChange: (value: ProductSortOption) => void;
}

export default function ProductFilters({
  categories,
  categoryFilter,
  searchTerm,
  sortOption,
  onCategoryChange,
  onSearchChange,
  onSortChange,
}: ProductFiltersProps) {
  const categoryOptions = [...categories].sort((first, second) => getCategoryName(first).localeCompare(getCategoryName(second)));

  return (
    <div className="product-filters">
      <label className="product-filters__control">
        <span>Search</span>
        <input
          onChange={(event) => onSearchChange(event.target.value)}
          placeholder="Product or category"
          type="search"
          value={searchTerm}
        />
      </label>

      <label className="product-filters__control">
        <span>Category</span>
        <select onChange={(event) => onCategoryChange(event.target.value)} value={categoryFilter}>
          <option value="all">All categories</option>
          {categoryOptions.map((category) => (
            <option key={category.id} value={category.id}>
              {getCategoryName(category)}
            </option>
          ))}
        </select>
      </label>

      <label className="product-filters__control">
        <span>Sort</span>
        <select onChange={(event) => onSortChange(event.target.value as ProductSortOption)} value={sortOption}>
          <option value="newest">Newest</option>
          <option value="price-asc">Price low to high</option>
          <option value="price-desc">Price high to low</option>
          <option value="name-asc">Name A to Z</option>
        </select>
      </label>
    </div>
  );
}

function getCategoryName(category: Category) {
  return category.categoryNameEn ?? category.categoryNamePt;
}
