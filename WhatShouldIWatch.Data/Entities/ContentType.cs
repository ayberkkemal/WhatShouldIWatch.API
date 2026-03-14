namespace WhatShouldIWatch.Data.Entities;

public class ContentType
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<Content> Contents { get; set; } = new List<Content>();
}
