using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WhatShouldIWatch.Business.Suggestion.Requests;

namespace WhatShouldIWatch.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SuggestionController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuggestionController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    public async Task<IActionResult> GetSuggestions([FromQuery] GetSuggestionsRequest query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
