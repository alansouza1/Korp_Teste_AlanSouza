namespace EstoqueService.Application.DTOs.Requests;

public class StockItemRequestDto
{
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
