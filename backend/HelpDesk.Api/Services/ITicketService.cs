using HelpDesk.Api.DTOs;
using HelpDesk.Api.DTOs.Tickets;

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
}
