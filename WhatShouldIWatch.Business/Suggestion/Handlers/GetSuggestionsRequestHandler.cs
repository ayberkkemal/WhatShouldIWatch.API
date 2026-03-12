using MediatR;
using WhatShouldIWatch.Business.Suggestion.Models;
using WhatShouldIWatch.Business.Suggestion.Requests;
using WhatShouldIWatch.Data.Repositories;

namespace WhatShouldIWatch.Business.Suggestion.Handlers;

public class GetSuggestionsRequestHandler : IRequestHandler<GetSuggestionsRequest, List<GetSuggestionsModel>>
{
    private readonly IContentRepository _contentRepository;

    public GetSuggestionsRequestHandler(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<List<GetSuggestionsModel>> Handle(GetSuggestionsRequest request, CancellationToken cancellationToken)
    {
        var names = await _contentRepository.GetMatchingContentNamesByTextAsync(
            request.Text,
            cancellationToken);

        return names
            .Select(name => new GetSuggestionsModel { SuggestedContent = name })
            .ToList();
    }
}
