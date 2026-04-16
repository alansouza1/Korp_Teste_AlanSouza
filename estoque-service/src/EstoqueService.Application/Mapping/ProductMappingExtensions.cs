using EstoqueService.Application.DTOs.Responses;
using EstoqueService.Domain.Entities;

namespace EstoqueService.Application.Mapping;

public static class ProductMappingExtensions
{
    public static ProductResponseDto ToResponse(this Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Code = product.Code,
            Description = product.Description,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
