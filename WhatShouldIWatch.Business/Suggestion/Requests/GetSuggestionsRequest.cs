using MediatR;
using WhatShouldIWatch.Business.Suggestion.Models;

namespace WhatShouldIWatch.Business.Suggestion.Requests;

public class GetSuggestionsRequest : IRequest<List<GetSuggestionsModel>>
{
    public string? Text { get; set; }
}
