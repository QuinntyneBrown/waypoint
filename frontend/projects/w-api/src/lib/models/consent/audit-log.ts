export interface AuditActorOption {
  id: string;
  name: string;
}

export interface AuditLogItem {
  id: string;
  actorUserId?: string;
  actorName: string;
  action: string;
  targetType?: string;
  targetId?: string;
  at: string;
  source?: string;
  isPrivate: boolean;
  details?: string;
}

export interface AuditLogPage {
  items: AuditLogItem[];
  page: number;
  pageSize: number;
  total: number;
  actors: AuditActorOption[];
  actions: string[];
  integrityValid: boolean;
}

export interface AuditLogFilter {
  page?: number;
  pageSize?: number;
  action?: string;
  actorUserId?: string;
  from?: string;
  to?: string;
}
