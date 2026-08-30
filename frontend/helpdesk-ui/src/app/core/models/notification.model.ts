export type NotificationType =
  | 'TicketAssigned'
  | 'PriorityChanged'
  | 'StatusChanged'
  | 'CommentAdded'
  | 'TicketEscalated'
  | 'TicketResolved';

export interface AppNotification {
  id: number;
  ticketId: number | null;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
}
