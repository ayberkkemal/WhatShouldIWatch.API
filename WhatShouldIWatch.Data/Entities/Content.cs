namespace WhatShouldIWatch.Data.Entities;

public class Content
{
    public long Id { get; set; }
    public string? ContentNameTr { get; set; }
    public string? ContentNameEn { get; set; }
    public string? EmotionTarget { get; set; }
    public string? Genre { get; set; }
    public string? Emotion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int ContentTypeId { get; set; }

    public ContentType ContentType { get; set; } = null!;
}
