using EstoqueService.Application.DTOs.Requests;
using EstoqueService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueService.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPost("description-suggestions")]
    public async Task<IActionResult> SuggestDescription([FromBody] SuggestProductDescriptionRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _productService.SuggestDescriptionAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? code, [FromQuery] string? description, CancellationToken cancellationToken)
    {
        var response = await _productService.GetAllAsync(code, description, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var response = await _productService.GetByCodeAsync(code, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDescription(Guid id, [FromBody] UpdateProductRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _productService.UpdateDescriptionAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpPatch("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _productService.UpdateStockAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("stock/validate")]
    public async Task<IActionResult> ValidateStock([FromBody] ValidateStockRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _productService.ValidateStockAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("stock/debit")]
    public async Task<IActionResult> DebitStock([FromBody] DebitStockRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _productService.DebitStockAsync(request, cancellationToken);
        return Ok(response);
    }
}
