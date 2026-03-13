using Npgsql;

namespace WhatShouldIWatch.Data.Repositories;

public class PgContentRepository : IContentRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PgContentRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async Task<IReadOnlyList<string>> GetMatchingContentNamesByTextAsync(
        string? text,
        CancellationToken cancellationToken = default)
    {
        var searchText = text?.Trim();
        if (string.IsNullOrWhiteSpace(searchText))
            return Array.Empty<string>();

        var list = new List<string>();
        await using var cmd = _dataSource.CreateCommand("""
            SELECT COALESCE("ContentNameTr", "ContentNameEn") AS "Name"
            FROM "CNT"."Content"
            WHERE "EmotionTarget" ILIKE $1 OR "Genre" ILIKE $1 OR "Emotion" ILIKE $1
            """);
        var pattern = "%" + searchText + "%";
        cmd.Parameters.Add(new NpgsqlParameter { Value = pattern });

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(0);
            if (!string.IsNullOrWhiteSpace(name))
                list.Add(name.Trim());
        }

        return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<IReadOnlyList<string>> GetUniqueSearchTermsAsync(CancellationToken cancellationToken = default)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var cmd = _dataSource.CreateCommand("""
            SELECT "EmotionTarget" FROM "CNT"."Content" WHERE "EmotionTarget" IS NOT NULL AND "EmotionTarget" <> ''
            UNION
            SELECT "Genre" FROM "CNT"."Content" WHERE "Genre" IS NOT NULL AND "Genre" <> ''
            UNION
            SELECT "Emotion" FROM "CNT"."Content" WHERE "Emotion" IS NOT NULL AND "Emotion" <> ''
            """);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var value = reader.GetString(0);
            if (!string.IsNullOrWhiteSpace(value))
                set.Add(value.Trim());
        }

        return set.ToList();
    }
}
