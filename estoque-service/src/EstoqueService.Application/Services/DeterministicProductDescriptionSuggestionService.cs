using EstoqueService.Application.Interfaces;

namespace EstoqueService.Application.Services;

public class DeterministicProductDescriptionSuggestionService : IProductDescriptionSuggestionService
{
    public Task<string> SuggestAsync(string code, string? partialDescription, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedPartialDescription = NormalizePartialDescription(partialDescription);
        var category = InferCategory(normalizedCode, normalizedPartialDescription);

        var suggestedDescription = string.IsNullOrWhiteSpace(normalizedPartialDescription)
            ? $"{category} {normalizedCode}"
            : $"{normalizedPartialDescription} {category.ToLowerInvariant()}";

        return Task.FromResult(suggestedDescription.Trim());
    }

    private static string NormalizePartialDescription(string? partialDescription)
    {
        if (string.IsNullOrWhiteSpace(partialDescription))
        {
            return string.Empty;
        }

        var compact = string.Join(" ", partialDescription
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return char.ToUpperInvariant(compact[0]) + compact[1..];
    }

    private static string InferCategory(string code, string partialDescription)
    {
        var signal = $"{code} {partialDescription}".ToUpperInvariant();

        if (ContainsAny(signal, "NOTE", "NB", "LAP"))
        {
            return "Notebook corporativo";
        }

        if (ContainsAny(signal, "MON", "DISPLAY", "TELA"))
        {
            return "Monitor profissional";
        }

        if (ContainsAny(signal, "TECL", "KEY", "KB"))
        {
            return "Teclado para escritorio";
        }

        if (ContainsAny(signal, "MOUSE", "MS"))
        {
            return "Mouse optico";
        }

        if (ContainsAny(signal, "CAB", "HDMI", "USB", "RJ45"))
        {
            return "Cabo de conexao";
        }

        if (ContainsAny(signal, "IMP", "PRINT"))
        {
            return "Impressora para uso empresarial";
        }

        return "Produto para operacao fiscal";
    }

    private static bool ContainsAny(string value, params string[] terms)
    {
        return terms.Any(value.Contains);
    }
}
