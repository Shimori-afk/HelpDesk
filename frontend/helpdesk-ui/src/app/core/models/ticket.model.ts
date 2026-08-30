export type TicketStatus =
  | 'Open'
  | 'InProgress'
  | 'WaitingForUser'
  | 'Escalated'
  | 'Resolved'
  | 'Closed'
  | 'Reopened';

export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export type SlaState = 'WithinSla' | 'ApproachingBreach' | 'Breached';

export type TicketActivityType =
  | 'Created'
  | 'Assigned'
  | 'StatusChanged'
  | 'PriorityChanged'
  | 'CommentAdded'
  | 'Escalated'
  | 'Resolved'
  | 'Reopened'
  | 'Closed';

export interface TicketListItem {
  id: number;
  title: string;
  status: TicketStatus;
  priority: TicketPriority;
  categoryName: string;
  departmentName: string | null;
  requesterId: number;
  requesterName: string;
  assignedAgentId: number | null;
  assignedAgentName: string | null;
  createdAt: string;
  responseDueAt: string;
  resolutionDueAt: string;
  resolvedAt: string | null;
  slaState: SlaState;
}

export interface TicketComment {
  id: number;
  authorId: number;
  authorName: string;
  message: string;
  createdAt: string;
}

export interface TicketActivity {
  id: number;
  userId: number | null;
  userName: string;
  type: TicketActivityType;
  description: string | null;
  oldValue: string | null;
  newValue: string | null;
  createdAt: string;
}

export interface TicketAttachment {
  id: number;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  createdAt: string;
}

export interface TicketDetail {
  id: number;
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  location: string | null;
  categoryId: number;
  categoryName: string;
  departmentId: number | null;
  departmentName: string | null;
  requesterId: number;
  requesterName: string;
  assignedAgentId: number | null;
  assignedAgentName: string | null;
  createdAt: string;
  updatedAt: string | null;
  resolvedAt: string | null;
  closedAt: string | null;
  responseDueAt: string;
  resolutionDueAt: string;
  respondedAt: string | null;
  slaState: SlaState;
  comments: TicketComment[];
  activities: TicketActivity[];
  attachments: TicketAttachment[];
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  categoryId: number;
  priority: TicketPriority;
  departmentId?: number | null;
  location?: string | null;
}

export interface UpdateTicketRequest {
  title: string;
  description: string;
  categoryId: number;
  departmentId?: number | null;
  location?: string | null;
}

export interface TicketQuery {
  search?: string;
  status?: TicketStatus;
  priority?: TicketPriority;
  categoryId?: number;
  departmentId?: number;
  assignedAgentId?: number;
  unassigned?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
