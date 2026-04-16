namespace EstoqueService.Application.DTOs.Requests;

public class DebitStockRequestDto
{
    public List<StockItemRequestDto> Items { get; set; } = [];
}
