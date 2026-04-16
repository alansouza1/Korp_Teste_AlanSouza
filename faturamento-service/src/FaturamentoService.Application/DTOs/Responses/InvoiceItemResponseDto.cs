namespace FaturamentoService.Application.DTOs.Responses;

public class InvoiceItemResponseDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
