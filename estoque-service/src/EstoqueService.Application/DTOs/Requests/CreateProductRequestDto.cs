namespace EstoqueService.Application.DTOs.Requests;

public class CreateProductRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}
