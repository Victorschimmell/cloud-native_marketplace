import { useState } from 'react';
import { Link } from 'react-router-dom';
import AdminHeader from '../components/AdminHeader';
import IssueForm from '../components/IssueForm';
import IssuesList from '../components/IssuesList';
import { placeholderIssues, type AdminIssue } from '../data/placeholderData';
import '../components/AdminDashboard.css';

// Report Issue page. Reached from the dashboard header or the Open Issues "View All" link.
// New issues only live in local state for now must change with API calls later.
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
    <section className="admin-dashboard">
      <AdminHeader title="Report Issue" />

      <Link to="/analytics" className="admin-dashboard__back">
        ← Back to Dashboard
      </Link>

      <div className="admin-dashboard__layout--split">
        <IssueForm onSubmit={handleSubmit} />
        <IssuesList issues={issues} />
      </div>
    </section>
  );
}
