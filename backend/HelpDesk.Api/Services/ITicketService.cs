using HelpDesk.Api.DTOs;
using HelpDesk.Api.DTOs.Tickets;
using HelpDesk.Api.Models;

namespace HelpDesk.Api.Services;

public interface ITicketService
{
    Task<PagedResult<TicketListItemDto>> GetTicketsAsync(TicketQueryParameters query);
    Task<TicketDetailDto> GetTicketByIdAsync(int id);
    Task<TicketDetailDto> CreateTicketAsync(CreateTicketDto dto);
    Task<TicketDetailDto> UpdateTicketAsync(int id, UpdateTicketDto dto);
    Task DeleteTicketAsync(int id);
    Task<IReadOnlyList<TicketActivityDto>> GetActivitiesAsync(int ticketId);
    Task<TicketCommentDto> AddCommentAsync(int ticketId, CreateCommentDto dto);
    Task<TicketDetailDto> ChangeStatusAsync(int ticketId, TicketStatus status);
    Task<TicketDetailDto> AssignAsync(int ticketId, int? agentId);
    Task<TicketDetailDto> ChangePriorityAsync(int ticketId, TicketPriority priority);
    Task<TicketDetailDto> EscalateAsync(int ticketId);
}
