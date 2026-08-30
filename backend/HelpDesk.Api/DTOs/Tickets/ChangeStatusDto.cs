using HelpDesk.Api.Models;

namespace HelpDesk.Api.DTOs.Tickets;

public record ChangeStatusDto(TicketStatus Status);

public record AssignTicketDto(int? AgentId);

public record ChangePriorityDto(TicketPriority Priority);
