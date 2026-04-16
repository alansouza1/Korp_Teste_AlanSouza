namespace FaturamentoService.Application.DTOs.Requests;

public class AddInvoiceItemRequestDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
