namespace FaturamentoService.Application.DTOs.Responses;

public class StockValidationResultDto
{
    public bool IsValid { get; set; }
    public List<StockItemAvailabilityDto> Items { get; set; } = [];
}
