namespace E_Library.Dtos.Books;

public sealed record BookFilterParameters(
    string? SearchTerm = null, 
    decimal? MinPrice = null, 
    decimal? MaxPrice = null, 
    EntityId? CategoryId = null,
    int PageNumber = 1, 
    int PageSize = 10);