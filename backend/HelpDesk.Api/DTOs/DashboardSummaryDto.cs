namespace HelpDesk.Api.DTOs;

public record DashboardSummaryDto(
    int OpenCount,
    int InProgressCount,
    int WaitingForUserCount,
    int EscalatedCount,
    int ResolvedCount,
    int ClosedCount,
    int ReopenedCount,
    int MyAssignedCount,
    int UnassignedCount,
    int OverdueCount,
    int CriticalOpenCount);
