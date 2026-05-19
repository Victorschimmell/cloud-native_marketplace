import { useState } from 'react';
import { Link } from 'react-router-dom';
import type { AdminIssue, IssuePriority, IssueType } from '../data/placeholderData';

interface IssueFormProps {
  onSubmit: (issue: Omit<AdminIssue, 'id' | 'status' | 'reportedBy' | 'date'>) => Promise<void> | void;
  cancelTo?: string;
  submitLabel?: string;
}

const issueTypeOptions: IssueType[] = ['user behavior', 'payment', 'workload anomaly', 'system', 'other'];
const priorityOptions: IssuePriority[] = ['low', 'medium', 'high'];

export default function IssueForm({ onSubmit, cancelTo, submitLabel = 'Submit Issue' }: IssueFormProps) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [type, setType] = useState<IssueType>('user behavior');
  const [priority, setPriority] = useState<IssuePriority>('medium');
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!title.trim() || !description.trim()) {
      return;
    }

    setIsSubmitting(true);
    try {
      await onSubmit({ title: title.trim(), description: description.trim(), type, priority });
      setTitle('');
      setDescription('');
      setType('user behavior');
      setPriority('medium');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="admin-issues-page__form-panel">
      <h2 className="admin-issues-page__form-title">Issue Information</h2>

      <form className="admin-issues-page__form" onSubmit={handleSubmit}>
        <div className="admin-issues-page__form-field">
          <label htmlFor="issue-title">Issue Title *</label>
          <input
            id="issue-title"
            type="text"
            placeholder="Brief description"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            disabled={isSubmitting}
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
            disabled={isSubmitting}
            required
          />
        </div>

        <div className="admin-issues-page__form-row">
          <div className="admin-issues-page__form-field">
            <label htmlFor="issue-type">Issue Type *</label>
            <select
              id="issue-type"
              value={type}
              onChange={(event) => setType(event.target.value as IssueType)}
              disabled={isSubmitting}
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
              disabled={isSubmitting}
            >
              {priorityOptions.map((option) => (
                <option key={option} value={option}>
                  {toTitleCase(option)}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="admin-issues-page__form-actions">
          <button type="submit" className="admin-issues-page__form-submit" disabled={isSubmitting}>
            {isSubmitting ? 'Submitting...' : submitLabel}
          </button>
          {cancelTo ? (
            <Link to={cancelTo} className="admin-issues-page__form-cancel">
              Cancel
            </Link>
          ) : null}
        </div>
      </form>
    </div>
  );
}

function toTitleCase(value: string): string {
  return value.replace(/\b\w/g, (character) => character.toUpperCase());
}
