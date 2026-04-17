namespace EstoqueService.Application.DTOs.Requests;

public class SuggestProductDescriptionRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string? PartialDescription { get; set; }
}
