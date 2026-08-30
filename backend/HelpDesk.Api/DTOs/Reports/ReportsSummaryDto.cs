namespace HelpDesk.Api.DTOs.Reports;

public record DailyCountDto(DateOnly Date, int Count);

public record NamedCountDto(string Name, int Count);

public record ReportsSummaryDto(
    IReadOnlyList<DailyCountDto> TicketsPerDay,
    IReadOnlyList<NamedCountDto> ByCategory,
    IReadOnlyList<NamedCountDto> ByPriority,
    IReadOnlyList<NamedCountDto> ByStatus,
    IReadOnlyList<NamedCountDto> ByAgent,
    double? AverageResolutionHours,
    double SlaComplianceRate);
