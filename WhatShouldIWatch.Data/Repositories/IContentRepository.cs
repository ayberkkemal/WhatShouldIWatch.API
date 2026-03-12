namespace WhatShouldIWatch.Data.Repositories;

/// <summary>
/// Film ve dizi listelerinden kullanıcı metnini Duygu Hedefi, Tür ve Duygu kolonlarıyla eşleştirerek içerik adlarını (ilk kolon) getirir.
/// </summary>
public interface IContentRepository
{
    /// <summary>
    /// Kullanıcının girdiği tek metni her iki Excel dosyasında Duygu Hedefi, Tür ve Duygu kolonlarıyla eşleştirir;
    /// bu kolonlardan herhangi birinde eşleşen satırların ilk kolonundaki değerleri döndürür.
    /// </summary>
    Task<IReadOnlyList<string>> GetMatchingContentNamesByTextAsync(
        string? text,
        CancellationToken cancellationToken = default);
}
