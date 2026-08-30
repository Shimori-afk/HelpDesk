export interface DashboardSummary {
  openCount: number;
  inProgressCount: number;
  waitingForUserCount: number;
  escalatedCount: number;
  resolvedCount: number;
  closedCount: number;
  reopenedCount: number;
  myAssignedCount: number;
  unassignedCount: number;
  overdueCount: number;
  criticalOpenCount: number;
}
