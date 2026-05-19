import { Link, useNavigate } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import IssueForm from '../components/IssueForm';
import { adminApi } from '../api/adminApi';
import { toCreateIssuePayload } from '../api/issueMapping';
import type { AdminIssue } from '../data/placeholderData';
import './AdminReportIssuePage.css';

export default function AdminCreateIssuePage() {
  const navigate = useNavigate();

  async function handleSubmit(newIssue: Omit<AdminIssue, 'id' | 'status' | 'reportedBy' | 'date'>) {
    try {
      await adminApi.createIssue(toCreateIssuePayload(newIssue));
      navigate('/admin/issues');
    } catch (requestError) {
      window.alert(`Could not create issue: ${getErrorMessage(requestError)}`);
      throw requestError;
    }
  }

  return (
    <PageSkeleton
      summary="Create an administrator issue for follow-up and resolution."
      title="Create New Issue"
      titleId="admin-create-issue-page-title"
    >
      <section className="admin-issue-create-page" aria-labelledby="admin-create-issue-page-title">
        <div className="admin-issues-page__toolbar">
          <Link to="/admin/issues" className="admin-issues-page__back">
            Back to Issues
          </Link>
        </div>

        <div className="admin-issue-create-page__panel">
          <IssueForm cancelTo="/admin/issues" onSubmit={handleSubmit} submitLabel="Create Issue" />
        </div>
      </section>
    </PageSkeleton>
  );
}

function getErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    const payload = err.payload as Record<string, unknown> | null;
    if (payload && typeof payload === 'object' && 'error' in payload) {
      return String(payload.error);
    }
    return err.message;
  }
  if (err instanceof Error) {
    return err.message;
  }
  return 'Unknown error';
}
