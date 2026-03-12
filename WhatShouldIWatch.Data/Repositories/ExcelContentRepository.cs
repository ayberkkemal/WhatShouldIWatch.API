using System.IO;
using ClosedXML.Excel;

namespace WhatShouldIWatch.Data.Repositories;

public class ExcelContentRepository : IContentRepository
{
    private const string DuyguHedefiColumn = "Duygu Hedefi";
    private const string TurColumn = "Tür";
    private const string DuyguColumn = "Duygu";

    private readonly string _contentsPath;

    private static readonly string[] ExcelFiles = { "Film Listesi.xlsx", "Dizi Listesi.xlsx" };

    /// <param name="contentsPath">Excel dosyalarının bulunduğu Contents klasörünün tam yolu.</param>
    public ExcelContentRepository(string contentsPath)
    {
        _contentsPath = contentsPath ?? throw new ArgumentNullException(nameof(contentsPath));
    }

    public async Task<IReadOnlyList<string>> GetMatchingContentNamesByTextAsync(
        string? text,
        CancellationToken cancellationToken = default)
    {
        var results = new List<string>();
        var searchText = text?.Trim();
        if (string.IsNullOrWhiteSpace(searchText))
            return results;

        foreach (var fileName in ExcelFiles)
        {
            var filePath = Path.Combine(_contentsPath, fileName);
            if (!File.Exists(filePath))
                continue;

            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);

                var (firstColIndex, duyguHedefiCol, turCol, duyguCol) = FindColumnIndexes(worksheet);
                if (firstColIndex < 0)
                    return;

                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
                for (var row = 2; row <= lastRow; row++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var rowDuyguHedefi = duyguHedefiCol >= 0 ? GetCellValue(worksheet, row, duyguHedefiCol) : null;
                    var rowTur = turCol >= 0 ? GetCellValue(worksheet, row, turCol) : null;
                    var rowDuygu = duyguCol >= 0 ? GetCellValue(worksheet, row, duyguCol) : null;

                    if (!TextMatchesAnyColumn(searchText, rowDuyguHedefi, rowTur, rowDuygu))
                        continue;

                    var firstColValue = GetCellValue(worksheet, row, firstColIndex);
                    if (!string.IsNullOrWhiteSpace(firstColValue))
                        results.Add(firstColValue.Trim());
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        return results;
    }

    private static (int FirstCol, int DuyguHedefiCol, int TurCol, int DuyguCol) FindColumnIndexes(IXLWorksheet ws)
    {
        var firstRow = ws.FirstRowUsed();
        if (firstRow == null)
            return (-1, -1, -1, -1);

        int firstCol = 1;
        int duyguHedefiCol = -1, turCol = -1, duyguCol = -1;

        var lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
        for (var c = 1; c <= lastCol; c++)
        {
            var header = GetCellValue(ws, 1, c);
            if (string.IsNullOrWhiteSpace(header))
                continue;

            var normalized = header.Trim();
            if (normalized.Equals(DuyguHedefiColumn, StringComparison.OrdinalIgnoreCase))
                duyguHedefiCol = c;
            else if (normalized.Equals(TurColumn, StringComparison.OrdinalIgnoreCase))
                turCol = c;
            else if (normalized.Equals(DuyguColumn, StringComparison.OrdinalIgnoreCase))
                duyguCol = c;
        }

        return (firstCol, duyguHedefiCol, turCol, duyguCol);
    }

    private static string? GetCellValue(IXLWorksheet ws, int row, int col)
    {
        var cell = ws.Cell(row, col);
        var value = cell.GetString();
        if (!string.IsNullOrWhiteSpace(value))
            return value.Trim();
        return cell.GetValue<string>()?.Trim();
    }

    /// <summary>Kullanıcı metni Duygu Hedefi, Tür veya Duygu kolonlarından en az birinin içinde geçiyorsa true (LIKE/Contains).</summary>
    private static bool TextMatchesAnyColumn(string searchText, string? rowDuyguHedefi, string? rowTur, string? rowDuygu)
    {
        return ContainsIgnoreCase(rowDuyguHedefi, searchText)
               || ContainsIgnoreCase(rowTur, searchText)
               || ContainsIgnoreCase(rowDuygu, searchText);
    }

    private static bool ContainsIgnoreCase(string? columnValue, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return false;
        var value = columnValue?.Trim() ?? "";
        return value.Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
