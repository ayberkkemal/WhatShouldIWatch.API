namespace WhatShouldIWatch.Business.Algorithms;

public class TurkishKeyboardFuzzySearch : IKeyboardFuzzySearch
{
    private static readonly IReadOnlyDictionary<char, IReadOnlySet<char>> KeyboardNeighbors = BuildTurkishQwertyNeighbors();

    private static IReadOnlyDictionary<char, IReadOnlySet<char>> BuildTurkishQwertyNeighbors()
    {
        var row1 = "qwertyuıopğü";
        var row2 = "asdfghjklşi";
        var row3 = "zxcvbnmöç";

        var dict = new Dictionary<char, HashSet<char>>();

        void AddNeighbors(string row)
        {
            for (var i = 0; i < row.Length; i++)
            {
                var c = row[i];
                if (!dict.ContainsKey(c))
                    dict[c] = new HashSet<char>();
                if (i > 0) dict[c].Add(row[i - 1]);
                if (i < row.Length - 1) dict[c].Add(row[i + 1]);
            }
        }

        AddNeighbors(row1);
        AddNeighbors(row2);
        AddNeighbors(row3);

        var rows = new[] { row1, row2, row3 };
        for (var r = 0; r < rows.Length; r++)
        {
            var row = rows[r];
            for (var i = 0; i < row.Length; i++)
            {
                var c = row[i];
                if (r > 0 && i < rows[r - 1].Length)
                    dict[c].Add(rows[r - 1][i]);
                if (r < rows.Length - 1 && i < rows[r + 1].Length)
                    dict[c].Add(rows[r + 1][i]);
            }
        }

        if (dict.ContainsKey('ı')) dict['ı'].Add('i');
        if (dict.ContainsKey('i')) dict['i'].Add('ı');

        var result = new Dictionary<char, IReadOnlySet<char>>();
        foreach (var kv in dict)
            result[kv.Key] = kv.Value;
        return result;
    }

    public IReadOnlyList<string> GetSearchCandidates(
        string input,
        IReadOnlyList<string> dictionaryWords,
        int maxCandidates = 5)
    {
        var trimmed = input?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return new List<string> { trimmed ?? "" };

        var list = new List<string> { trimmed };
        if (dictionaryWords == null || dictionaryWords.Count == 0)
            return list;

        var scored = dictionaryWords
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => w.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(word => (Word: word, Distance: GetKeyboardEditDistance(trimmed, word)))
            .Where(x => x.Distance >= 0)
            .OrderBy(x => x.Distance)
            .Take(maxCandidates)
            .Select(x => x.Word)
            .ToList();

        foreach (var w in scored)
        {
            if (!list.Contains(w, StringComparer.OrdinalIgnoreCase))
                list.Add(w);
        }

        return list;
    }

    private static int GetKeyboardEditDistance(string a, string b)
    {
        if (a == null || b == null) return -1;
        a = a.Trim().ToLowerInvariant();
        b = b.Trim().ToLowerInvariant();
        if (a.Length == 0 && b.Length == 0) return 0;
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var n = a.Length;
        var m = b.Length;
        var d = new int[n + 1, m + 1];

        for (var i = 0; i <= n; i++) d[i, 0] = i;
        for (var j = 0; j <= m; j++) d[0, j] = j;

        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                var cost = GetSubstituteCost(a[i - 1], b[j - 1]);
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }

    private static int GetSubstituteCost(char c1, char c2)
    {
        if (c1 == c2) return 0;
        if (KeyboardNeighbors.TryGetValue(c1, out var neighbors) && neighbors.Contains(char.ToLowerInvariant(c2)))
            return 1;
        return 2;
    }
}
