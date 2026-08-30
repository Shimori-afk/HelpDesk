using FluentValidation;
using HelpDesk.Api.Common;
using HelpDesk.Api.DTOs.Tickets;
using HelpDesk.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController(
    ITicketService ticketService,
    IValidator<CreateTicketDto> createValidator,
    IValidator<UpdateTicketDto> updateValidator,
    IValidator<CreateCommentDto> commentValidator,
    IValidator<ChangeStatusDto> statusValidator,
    IValidator<ChangePriorityDto> priorityValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetTickets([FromQuery] TicketQueryParameters query)
    {
        var result = await ticketService.GetTicketsAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDetailDto>> GetTicket(int id)
    {
        var ticket = await ticketService.GetTicketByIdAsync(id);
        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<TicketDetailDto>> CreateTicket(CreateTicketDto dto)
    {
        await createValidator.ValidateAndThrowAppExceptionAsync(dto);
        var ticket = await ticketService.CreateTicketAsync(dto);
        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketDetailDto>> UpdateTicket(int id, UpdateTicketDto dto)
    {
        await updateValidator.ValidateAndThrowAppExceptionAsync(dto);
        var ticket = await ticketService.UpdateTicketAsync(id, dto);
        return Ok(ticket);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        await ticketService.DeleteTicketAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/activities")]
    public async Task<ActionResult<IReadOnlyList<TicketActivityDto>>> GetActivities(int id)
    {
        var activities = await ticketService.GetActivitiesAsync(id);
        return Ok(activities);
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<TicketCommentDto>> AddComment(int id, CreateCommentDto dto)
    {
        await commentValidator.ValidateAndThrowAppExceptionAsync(dto);
        var comment = await ticketService.AddCommentAsync(id, dto);
        return Ok(comment);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<TicketDetailDto>> ChangeStatus(int id, ChangeStatusDto dto)
    {
        await statusValidator.ValidateAndThrowAppExceptionAsync(dto);
        var ticket = await ticketService.ChangeStatusAsync(id, dto.Status);
        return Ok(ticket);
    }

    [Authorize(Roles = "Agent,Admin")]
    [HttpPut("{id:int}/assign")]
    public async Task<ActionResult<TicketDetailDto>> Assign(int id, AssignTicketDto dto)
    {
        var ticket = await ticketService.AssignAsync(id, dto.AgentId);
        return Ok(ticket);
    }

    [Authorize(Roles = "Agent,Admin")]
    [HttpPut("{id:int}/priority")]
    public async Task<ActionResult<TicketDetailDto>> ChangePriority(int id, ChangePriorityDto dto)
    {
        await priorityValidator.ValidateAndThrowAppExceptionAsync(dto);
        var ticket = await ticketService.ChangePriorityAsync(id, dto.Priority);
        return Ok(ticket);
    }

    [Authorize(Roles = "Agent,Admin")]
    [HttpPost("{id:int}/escalate")]
    public async Task<ActionResult<TicketDetailDto>> Escalate(int id)
    {
        var ticket = await ticketService.EscalateAsync(id);
        return Ok(ticket);
    }
}
