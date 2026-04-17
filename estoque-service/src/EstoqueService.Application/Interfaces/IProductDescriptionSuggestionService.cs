namespace EstoqueService.Application.Interfaces;

public interface IProductDescriptionSuggestionService
{
    Task<string> SuggestAsync(string code, string? partialDescription, CancellationToken cancellationToken = default);
}
