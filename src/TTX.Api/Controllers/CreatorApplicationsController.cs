using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTX.Api.Interfaces;
using TTX.Commands.CreatorApplications.CreateCreatorApplication;
using TTX.Commands.CreatorApplications.UpdateCreatorApplication;
using TTX.Dto;
using TTX.Dto.CreatorApplications;
using TTX.Queries;
using TTX.Queries.CreatorApplications.FindCreatorApplication;
using TTX.Queries.CreatorApplications.IndexCreatorApplications;

namespace TTX.Api.Controllers;

[ApiController]
[Route("creator-applications")]
[Produces(MediaTypeNames.Application.Json)]
public class CreatorApplicationsController(ISender sender, ISessionService sessions) : ControllerBase
{
    [HttpGet]
    [EndpointName("GetCreatorApplications")]
    public async Task<ActionResult<PaginationDto<CreatorApplicationDto>>> Index(
        [FromQuery(Name = "page")] int index = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] CreatorApplicationOrderBy? orderBy = null,
        [FromQuery] OrderDirection? orderDir = null
    )
    {
        var page = await sender.Send(new IndexCreatorApplicationQuery
        {
            Page = index,
            Limit = limit,
            Order = orderBy is null
                ? null
                : new Order<CreatorApplicationOrderBy>
                {
                    By = orderBy.Value,
                    Dir = orderDir ?? OrderDirection.Ascending
                },
            Search = search
        });

        return Ok(page);
    }

    [HttpGet("{id:int}")]
    [EndpointName("GetCreatorApplication")]
    public async Task<ActionResult<CreatorApplicationDto>> Show(int id)
    {
        var application = await sender.Send(new FindCreatorApplicationQuery
        {
            Id = id
        });

        if (application is null)
        {
            return NotFound();
        }

        return Ok(application);
    }

    [HttpPost]
    [Authorize]
    [EndpointName("CreateCreatorApplication")]
    public async Task<ActionResult<CreatorApplicationDto>> Create([FromBody] CreateCreatorApplicationCommand command)
    {
        var currentUserId = sessions.GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var application = await sender.Send(new CreateCreatorApplicationCommand
        {
            SubmitterId = currentUserId.Value,
            Username = command.Username,
            Ticker = command.Ticker
        });

        return CreatedAtAction(nameof(Show), new { id = application.Id }, application);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("UpdateCreatorApplication")]
    public async Task<ActionResult<CreatorApplicationDto>> Update(int id, [FromBody] UpdateCreatorApplicationCommand command)
    {
        var application = await sender.Send(new FindCreatorApplicationQuery
        {
            Id = id
        });

        if (application is null)
        {
            return NotFound();
        }

        var updatedApplication = await sender.Send(new UpdateCreatorApplicationCommand
        {
            ApplicationId = id,
            Status = command.Status
        });

        return Ok(updatedApplication);
    }
}
