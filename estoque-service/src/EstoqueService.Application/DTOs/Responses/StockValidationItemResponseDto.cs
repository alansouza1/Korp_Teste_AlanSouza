namespace EstoqueService.Application.DTOs.Responses;

public class StockValidationItemResponseDto
{
    public string ProductCode { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public string Message { get; set; } = string.Empty;
}
