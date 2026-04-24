import './Pagination.css';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  disabled?: boolean;
  label?: string;
  onNext: () => void;
  onPrevious: () => void;
}

export default function Pagination({
  currentPage,
  totalPages,
  disabled = false,
  label = 'Pagination',
  onNext,
  onPrevious,
}: PaginationProps) {
  if (totalPages <= 1) {
    return null;
  }

  return (
    <nav className="pagination" aria-label={label}>
      <button disabled={disabled || currentPage === 1} onClick={onPrevious} type="button">
        Previous
      </button>
      <span>
        Page {currentPage} of {totalPages}
      </span>
      <button disabled={disabled || currentPage === totalPages} onClick={onNext} type="button">
        Next
      </button>
    </nav>
  );
}
