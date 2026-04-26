import type { PropsWithChildren } from 'react';
import './PageSkeleton.css';

interface PageSkeletonProps extends PropsWithChildren {
  title: string;
  summary?: string;
  titleId?: string;
}

export default function PageSkeleton({ children, summary, title, titleId }: PageSkeletonProps) {
  return (
    <section className="page-skeleton" aria-labelledby={titleId}>
      <header className="page-skeleton__header">
        <div>
          <h1 id={titleId} className="page-skeleton__title">
            {title}
          </h1>
          {summary ? <p className="page-skeleton__summary">{summary}</p> : null}
        </div>
      </header>

      {children ?? (
        <p className="page-skeleton__placeholder">
          This is the skeleton page.
          <br />
          Implementation of the page goes here.
        </p>
      )}
    </section>
  );
}
