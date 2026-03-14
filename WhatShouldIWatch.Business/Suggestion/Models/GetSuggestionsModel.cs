namespace WhatShouldIWatch.Business.Suggestion.Models;

public class GetSuggestionsModel
{
    public long Id { get; set; }
    public string? Type { get; set; }
    public required string Name { get; set; }
    public required string SuggestedContent { get; set; }
}
