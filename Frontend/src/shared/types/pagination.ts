export interface PageResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}
