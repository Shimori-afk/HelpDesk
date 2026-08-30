export interface DailyCount {
  date: string;
  count: number;
}

export interface NamedCount {
  name: string;
  count: number;
}

export interface ReportsSummary {
  ticketsPerDay: DailyCount[];
  byCategory: NamedCount[];
  byPriority: NamedCount[];
  byStatus: NamedCount[];
  byAgent: NamedCount[];
  averageResolutionHours: number | null;
  slaComplianceRate: number;
}
