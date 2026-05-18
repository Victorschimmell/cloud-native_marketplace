import { useState } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import IssueForm from '../components/IssueForm';
import IssuesList from '../components/IssuesList';
import { placeholderIssues, type AdminIssue } from '../data/placeholderData';
import './AdminReportIssuePage.css';

// Report Issue page. Reached from the Open Issues "View All" link on the dashboard.
// New issues live only in local state until the back-end story is ready.
export default function AdminReportIssuePage() {
  const [issues, setIssues] = useState<AdminIssue[]>(placeholderIssues);

  function handleSubmit(newIssue: Omit<AdminIssue, 'id' | 'status' | 'reportedBy' | 'date'>) {
    const issue: AdminIssue = {
      ...newIssue,
      id: `ISSUE-${Date.now()}`,
      status: 'open',
      reportedBy: 'Admin User',
      date: new Date().toISOString(),
    };
    setIssues((current) => [issue, ...current]);
  }

  return (
    <PageSkeleton title="Report Issue" summary="File a new issue or browse the ones already reported.">
      <div className="admin-issues-page">
        <IssueForm onSubmit={handleSubmit} />
        <IssuesList issues={issues} />
      </div>
    </PageSkeleton>
  );
}
