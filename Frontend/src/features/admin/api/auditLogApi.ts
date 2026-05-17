import { request } from '../../../shared/api/request';
import type { PageResponse } from '../../../shared/types/pagination';
import type { AuditLogEntry } from '../types';

interface GetAuditLogsOptions {
  actorUserId?: string;
  entityType?: string;
  entityId?: string;
  signal?: AbortSignal;
}

export const auditLogApi = {
  getAuditLogs: async (page = 1, pageSize = 20, options?: GetAuditLogsOptions) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (options?.actorUserId) {
      params.set('actorUserId', options.actorUserId);
    }

    if (options?.entityType) {
      params.set('entityType', options.entityType);
    }

    if (options?.entityId) {
      params.set('entityId', options.entityId);
    }

    return request<PageResponse<AuditLogEntry>>(`/api/admin/audit-logs?${params.toString()}`, {
      signal: options?.signal,
    });
  },
};
