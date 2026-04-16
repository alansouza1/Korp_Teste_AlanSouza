using EstoqueService.Application.DTOs.Requests;
using EstoqueService.Application.DTOs.Responses;

namespace EstoqueService.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default);
    Task<List<ProductResponseDto>> GetAllAsync(string? code, string? description, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> UpdateDescriptionAsync(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> UpdateStockAsync(Guid id, UpdateStockRequestDto request, CancellationToken cancellationToken = default);
    Task<ValidateStockResponseDto> ValidateStockAsync(ValidateStockRequestDto request, CancellationToken cancellationToken = default);
    Task<DebitStockResponseDto> DebitStockAsync(DebitStockRequestDto request, CancellationToken cancellationToken = default);
}
