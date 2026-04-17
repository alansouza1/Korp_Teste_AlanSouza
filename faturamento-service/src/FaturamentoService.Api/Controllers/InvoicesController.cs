using FaturamentoService.Application.DTOs.Requests;
using FaturamentoService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FaturamentoService.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequestDto? request, CancellationToken cancellationToken)
    {
        var response = await _invoiceService.CreateAsync(request ?? new CreateInvoiceRequestDto(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _invoiceService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _invoiceService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("number/{number:int}")]
    public async Task<IActionResult> GetBySequentialNumber(int number, CancellationToken cancellationToken)
    {
        var response = await _invoiceService.GetBySequentialNumberAsync(number, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItems(Guid id, [FromBody] AddInvoiceItemsRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _invoiceService.AddItemsAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/print")]
    public async Task<IActionResult> Print(
        Guid id,
        [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var response = await _invoiceService.PrintIdempotentAsync(id, idempotencyKey, cancellationToken);
            return new ContentResult
            {
                StatusCode = response.StatusCode,
                ContentType = "application/json",
                Content = response.ResponseJson
            };
        }

        var printResponse = await _invoiceService.PrintAsync(id, cancellationToken);
        return Ok(printResponse);
    }
}
