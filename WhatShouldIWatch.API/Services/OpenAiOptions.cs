namespace WhatShouldIWatch.API.Services;

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string? ApiKey { get; set; }

    public string Model { get; set; } = "gpt-4o-mini";

    public string? BaseUrl { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
