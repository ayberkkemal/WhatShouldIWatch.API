namespace WhatShouldIWatch.Business.Algorithms;

public interface IKeyboardFuzzySearch
{
    IReadOnlyList<string> GetSearchCandidates(
        string input,
        IReadOnlyList<string> dictionaryWords,
        int maxCandidates = 5);
}
