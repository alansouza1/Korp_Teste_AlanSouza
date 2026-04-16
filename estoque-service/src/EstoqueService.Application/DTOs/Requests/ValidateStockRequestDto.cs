namespace EstoqueService.Application.DTOs.Requests;

public class ValidateStockRequestDto
{
    public List<StockItemRequestDto> Items { get; set; } = [];
}
