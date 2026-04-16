namespace EstoqueService.Application.DTOs.Responses;

public class ValidateStockResponseDto
{
    public bool IsValid { get; set; }
    public List<StockValidationItemResponseDto> Items { get; set; } = [];
}
