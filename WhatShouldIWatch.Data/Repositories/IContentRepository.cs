namespace WhatShouldIWatch.Data.Repositories;

public interface IContentRepository
{
    Task<IReadOnlyList<string>> GetMatchingContentNamesByTextAsync(
        string? text,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetUniqueSearchTermsAsync(CancellationToken cancellationToken = default);
}
