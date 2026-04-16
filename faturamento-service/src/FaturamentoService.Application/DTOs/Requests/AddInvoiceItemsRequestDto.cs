namespace FaturamentoService.Application.DTOs.Requests;

public class AddInvoiceItemsRequestDto
{
    public List<AddInvoiceItemRequestDto> Items { get; set; } = [];
}
