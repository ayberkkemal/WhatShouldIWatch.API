using MediatR;
using Microsoft.EntityFrameworkCore;
using WhatShouldIWatch.Business.Algorithms;
using WhatShouldIWatch.Business.Suggestion.Models;
using WhatShouldIWatch.Business.Suggestion.Requests;
using WhatShouldIWatch.Business.Suggestion.Services;
using WhatShouldIWatch.Data;

namespace WhatShouldIWatch.Business.Suggestion.Handlers;

public class GetSuggestionsRequestHandler : IRequestHandler<GetSuggestionsRequest, List<GetSuggestionsModel>>
{
    private readonly AppDbContext _db;
    private readonly IKeyboardFuzzySearch _keyboardFuzzySearch;
    private readonly IEmotionKeywordExtractor _keywordExtractor;

    public GetSuggestionsRequestHandler(
        AppDbContext db,
        IKeyboardFuzzySearch keyboardFuzzySearch,
        IEmotionKeywordExtractor keywordExtractor)
    {
        _db = db;
        _keyboardFuzzySearch = keyboardFuzzySearch;
        _keywordExtractor = keywordExtractor;
    }

    public async Task<List<GetSuggestionsModel>> Handle(GetSuggestionsRequest request, CancellationToken cancellationToken)
    {
        var searchText = request.Text?.Trim();
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<GetSuggestionsModel>();

        var keywords = await _keywordExtractor.ExtractKeywordsAsync(searchText, cancellationToken);

        var byId = new Dictionary<long, (string TypeName, string Name)>();

        if (keywords.Count > 0)
        {
            foreach (var keyword in keywords.Take(15))
            {
                var pattern = "%" + keyword + "%";
                var items = await _db.Contents
                    .AsNoTracking()
                    .Include(c => c.ContentType)
                    .Where(c =>
                        (c.EmotionTarget != null && EF.Functions.ILike(c.EmotionTarget, pattern)) ||
                        (c.Genre != null && EF.Functions.ILike(c.Genre, pattern)) ||
                        (c.Emotion != null && EF.Functions.ILike(c.Emotion, pattern)))
                    .Select(c => new
                    {
                        c.Id,
                        TypeName = c.ContentType!.Name,
                        Name = (c.ContentNameTr ?? c.ContentNameEn) ?? ""
                    })
                    .ToListAsync(cancellationToken);

                foreach (var x in items)
                {
                    if (string.IsNullOrWhiteSpace(x.Name)) continue;
                    var name = x.Name.Trim();
                    if (!byId.ContainsKey(x.Id))
                        byId[x.Id] = (x.TypeName, name);
                }
            }
        }

        if (byId.Count < 5)
        {
            var uniqueTerms = await GetUniqueSearchTermsAsync(cancellationToken);
            var candidates = _keyboardFuzzySearch.GetSearchCandidates(searchText, uniqueTerms, maxCandidates: 5);

            foreach (var candidate in candidates)
            {
                var pattern = "%" + candidate + "%";
                var items = await _db.Contents
                    .AsNoTracking()
                    .Include(c => c.ContentType)
                    .Where(c =>
                        (c.EmotionTarget != null && EF.Functions.ILike(c.EmotionTarget, pattern)) ||
                        (c.Genre != null && EF.Functions.ILike(c.Genre, pattern)) ||
                        (c.Emotion != null && EF.Functions.ILike(c.Emotion, pattern)))
                    .Select(c => new
                    {
                        c.Id,
                        TypeName = c.ContentType!.Name,
                        Name = (c.ContentNameTr ?? c.ContentNameEn) ?? ""
                    })
                    .ToListAsync(cancellationToken);

                foreach (var x in items)
                {
                    if (string.IsNullOrWhiteSpace(x.Name)) continue;
                    var name = x.Name.Trim();
                    if (!byId.ContainsKey(x.Id))
                        byId[x.Id] = (x.TypeName, name);
                }
            }
        }

        return byId
            .Select(kv => new GetSuggestionsModel
            {
                Id = kv.Key,
                Type = kv.Value.TypeName,
                Name = kv.Value.Name,
                SuggestedContent = kv.Value.Name
            })
            .ToList();
    }

    private async Task<IReadOnlyList<string>> GetUniqueSearchTermsAsync(CancellationToken cancellationToken)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var emotionTargets = await _db.Contents
            .AsNoTracking()
            .Where(c => c.EmotionTarget != null && c.EmotionTarget != "")
            .Select(c => c.EmotionTarget!)
            .Distinct()
            .ToListAsync(cancellationToken);
        var genres = await _db.Contents
            .AsNoTracking()
            .Where(c => c.Genre != null && c.Genre != "")
            .Select(c => c.Genre!)
            .Distinct()
            .ToListAsync(cancellationToken);
        var emotions = await _db.Contents
            .AsNoTracking()
            .Where(c => c.Emotion != null && c.Emotion != "")
            .Select(c => c.Emotion!)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var t in emotionTargets) terms.Add(t.Trim());
        foreach (var t in genres) terms.Add(t.Trim());
        foreach (var t in emotions) terms.Add(t.Trim());

        return terms.ToList();
    }
}
