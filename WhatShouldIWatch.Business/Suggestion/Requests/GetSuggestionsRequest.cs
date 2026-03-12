using MediatR;
using WhatShouldIWatch.Business.Suggestion.Models;

namespace WhatShouldIWatch.Business.Suggestion.Requests;

public class GetSuggestionsRequest : IRequest<List<GetSuggestionsModel>>
{
    /// <summary>Kullanıcının girdiği metin; Duygu Hedefi, Tür ve Duygu kolonlarıyla eşleştirilir.</summary>
    public string? Text { get; set; }
}
