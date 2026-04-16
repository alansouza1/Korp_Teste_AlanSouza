namespace EstoqueService.Application.DTOs.Responses;

public class DebitStockResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ProductResponseDto> UpdatedProducts { get; set; } = [];
}
