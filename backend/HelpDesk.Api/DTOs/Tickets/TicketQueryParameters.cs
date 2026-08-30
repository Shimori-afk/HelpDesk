using HelpDesk.Api.Models;

namespace HelpDesk.Api.DTOs.Tickets;

public class TicketQueryParameters
{
    public string? Search { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public int? CategoryId { get; set; }
    public int? DepartmentId { get; set; }
    public int? AssignedAgentId { get; set; }
    public bool? Unassigned { get; set; }
    public DateTimeOffset? CreatedFrom { get; set; }
    public DateTimeOffset? CreatedTo { get; set; }

    public string SortBy { get; set; } = "createdAt";
    public bool SortDescending { get; set; } = true;

    private int _page = 1;
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > 100 ? 20 : value;
    }
}
