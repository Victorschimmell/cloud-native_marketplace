import { useState } from 'react';
import type { AdminIssue, IssuePriority, IssueType } from '../data/placeholderData';

interface IssueFormProps {
  // Called when the form is submitted. Parent stores the new issue.
  onSubmit: (issue: Omit<AdminIssue, 'id' | 'status' | 'reportedBy' | 'date'>) => void;
}

const issueTypeOptions: IssueType[] = ['user behavior', 'payment', 'workload anomaly', 'system', 'other'];
const priorityOptions: IssuePriority[] = ['low', 'medium', 'high'];

// "Create New Issue" form on the Report Issue page.
export default function IssueForm({ onSubmit }: IssueFormProps) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [type, setType] = useState<IssueType>('user behavior');
  const [priority, setPriority] = useState<IssuePriority>('medium');

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!title.trim() || !description.trim()) {
      return;
    }

    onSubmit({ title: title.trim(), description: description.trim(), type, priority });
    setTitle('');
    setDescription('');
    setType('user behavior');
    setPriority('medium');
  }

  return (
    <div className="admin-issues-page__panel">
      <div className="admin-issues-page__panel-header">
        <h2 className="admin-issues-page__panel-title">Create New Issue</h2>
      </div>

      <form className="admin-issues-page__form" onSubmit={handleSubmit}>
        <div className="admin-issues-page__form-field">
          <label htmlFor="issue-title">Issue Title *</label>
          <input
            id="issue-title"
            type="text"
            placeholder="Brief description"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            required
          />
        </div>

        <div className="admin-issues-page__form-field">
          <label htmlFor="issue-description">Description *</label>
          <textarea
            id="issue-description"
            placeholder="Detailed description"
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            required
          />
        </div>

        <div className="admin-issues-page__form-field">
          <label htmlFor="issue-type">Issue Type *</label>
          <select
            id="issue-type"
            value={type}
            onChange={(event) => setType(event.target.value as IssueType)}
          >
            {issueTypeOptions.map((option) => (
              <option key={option} value={option}>
                {toTitleCase(option)}
              </option>
            ))}
          </select>
        </div>

        <div className="admin-issues-page__form-field">
          <label htmlFor="issue-priority">Priority *</label>
          <select
            id="issue-priority"
            value={priority}
            onChange={(event) => setPriority(event.target.value as IssuePriority)}
          >
            {priorityOptions.map((option) => (
              <option key={option} value={option}>
                {toTitleCase(option)}
              </option>
            ))}
          </select>
        </div>

        <button type="submit" className="admin-issues-page__form-submit">
          Submit Issue
        </button>
      </form>
    </div>
  );
}

function toTitleCase(value: string): string {
  return value.replace(/\b\w/g, (character) => character.toUpperCase());
}
