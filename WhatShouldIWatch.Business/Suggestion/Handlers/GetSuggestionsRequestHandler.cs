using MediatR;
using WhatShouldIWatch.Business.Algorithms;
using WhatShouldIWatch.Business.Suggestion.Models;
using WhatShouldIWatch.Business.Suggestion.Requests;
using WhatShouldIWatch.Data.Repositories;

namespace WhatShouldIWatch.Business.Suggestion.Handlers;

public class GetSuggestionsRequestHandler : IRequestHandler<GetSuggestionsRequest, List<GetSuggestionsModel>>
{
    private readonly IContentRepository _contentRepository;
    private readonly IKeyboardFuzzySearch _keyboardFuzzySearch;

    public GetSuggestionsRequestHandler(
        IContentRepository contentRepository,
        IKeyboardFuzzySearch keyboardFuzzySearch)
    {
        _contentRepository = contentRepository;
        _keyboardFuzzySearch = keyboardFuzzySearch;
    }

    public async Task<List<GetSuggestionsModel>> Handle(GetSuggestionsRequest request, CancellationToken cancellationToken)
    {
        var searchText = request.Text?.Trim();
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<GetSuggestionsModel>();

        var uniqueTerms = await _contentRepository.GetUniqueSearchTermsAsync(cancellationToken);
        var candidates = _keyboardFuzzySearch.GetSearchCandidates(searchText, uniqueTerms, maxCandidates: 5);

        var allNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var candidate in candidates)
        {
            var names = await _contentRepository.GetMatchingContentNamesByTextAsync(candidate, cancellationToken);
            foreach (var name in names)
                allNames.Add(name);
        }

        return allNames
            .Select(name => new GetSuggestionsModel { SuggestedContent = name })
            .ToList();
    }
}
