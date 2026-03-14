namespace WhatShouldIWatch.Business.Suggestion.Services;

public interface IEmotionKeywordExtractor
{
    Task<IReadOnlyList<string>> ExtractKeywordsAsync(string userText, CancellationToken cancellationToken = default);
}
