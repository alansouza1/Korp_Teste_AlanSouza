using EstoqueService.Application.DTOs.Requests;
using EstoqueService.Application.DTOs.Responses;
using EstoqueService.Application.Exceptions;
using EstoqueService.Application.Interfaces;
using EstoqueService.Application.Mapping;
using EstoqueService.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using AppValidationException = EstoqueService.Application.Exceptions.ValidationException;

namespace EstoqueService.Application.Services;

public class ProductService : IProductService
{
    private const string SimulatedFailureCode = "ERRO500";

    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateProductRequestDto> _createValidator;
    private readonly IValidator<UpdateProductRequestDto> _updateValidator;
    private readonly IValidator<UpdateStockRequestDto> _stockValidator;
    private readonly IValidator<ValidateStockRequestDto> _validateStockValidator;
    private readonly IValidator<DebitStockRequestDto> _debitStockValidator;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        IValidator<CreateProductRequestDto> createValidator,
        IValidator<UpdateProductRequestDto> updateValidator,
        IValidator<UpdateStockRequestDto> stockValidator,
        IValidator<ValidateStockRequestDto> validateStockValidator,
        IValidator<DebitStockRequestDto> debitStockValidator,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _stockValidator = stockValidator;
        _validateStockValidator = validateStockValidator;
        _debitStockValidator = debitStockValidator;
        _logger = logger;
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await _productRepository.ExistsByCodeAsync(normalizedCode, cancellationToken))
        {
            throw new ConflictException($"A product with code '{normalizedCode}' already exists.");
        }

        var product = new Product(normalizedCode, request.Description, request.StockQuantity);
        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductCode} created with id {ProductId}", product.Code, product.Id);

        return product.ToResponse();
    }

    public async Task<List<ProductResponseDto>> GetAllAsync(string? code, string? description, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(code, description, cancellationToken);
        return products.Select(x => x.ToResponse()).ToList();
    }

    public async Task<ProductResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetExistingProductByIdAsync(id, cancellationToken);
        return product.ToResponse();
    }

    public async Task<ProductResponseDto> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var product = await _productRepository.GetByCodeAsync(normalizedCode, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with code '{normalizedCode}' was not found.");
        }

        return product.ToResponse();
    }

    public async Task<ProductResponseDto> UpdateDescriptionAsync(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);

        var product = await GetExistingProductByIdAsync(id, cancellationToken);
        product.UpdateDescription(request.Description);

        await _productRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} description updated", product.Id);

        return product.ToResponse();
    }

    public async Task<ProductResponseDto> UpdateStockAsync(Guid id, UpdateStockRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_stockValidator, request, cancellationToken);

        var product = await GetExistingProductByIdAsync(id, cancellationToken);
        product.SetStock(request.StockQuantity);

        await _productRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} stock updated to {StockQuantity}", product.Id, product.StockQuantity);

        return product.ToResponse();
    }

    public async Task<ValidateStockResponseDto> ValidateStockAsync(ValidateStockRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_validateStockValidator, request, cancellationToken);
        EnsureFailureScenarioNotTriggered(request.Items.Select(x => x.ProductCode));

        var normalizedCodes = request.Items
            .Select(x => x.ProductCode.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var products = await _productRepository.GetByCodesAsync(normalizedCodes, cancellationToken);
        var productsByCode = products.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        var items = request.Items.Select(item =>
        {
            var normalizedCode = item.ProductCode.Trim().ToUpperInvariant();
            var productExists = productsByCode.TryGetValue(normalizedCode, out var product);

            if (!productExists || product is null)
            {
                return new StockValidationItemResponseDto
                {
                    ProductCode = normalizedCode,
                    RequestedQuantity = item.Quantity,
                    AvailableQuantity = 0,
                    IsAvailable = false,
                    Message = "Product not found."
                };
            }

            var isAvailable = product.StockQuantity >= item.Quantity;
            return new StockValidationItemResponseDto
            {
                ProductCode = normalizedCode,
                RequestedQuantity = item.Quantity,
                AvailableQuantity = product.StockQuantity,
                IsAvailable = isAvailable,
                Message = isAvailable ? "Stock available." : "Insufficient stock."
            };
        }).ToList();

        return new ValidateStockResponseDto
        {
            IsValid = items.All(x => x.IsAvailable),
            Items = items
        };
    }

    public async Task<DebitStockResponseDto> DebitStockAsync(DebitStockRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_debitStockValidator, request, cancellationToken);
        EnsureFailureScenarioNotTriggered(request.Items.Select(x => x.ProductCode));

        var groupedItems = request.Items
            .GroupBy(x => x.ProductCode.Trim().ToUpperInvariant())
            .Select(group => new
            {
                ProductCode = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToList();

        await using var transaction = await _productRepository.BeginTransactionAsync(cancellationToken);

        var products = await _productRepository.GetByCodesForUpdateAsync(groupedItems.Select(x => x.ProductCode), cancellationToken);
        var productsByCode = products.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var item in groupedItems)
        {
            if (!productsByCode.TryGetValue(item.ProductCode, out var product))
            {
                throw new NotFoundException($"Product with code '{item.ProductCode}' was not found.");
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new ConflictException($"Insufficient stock for product '{item.ProductCode}'. Another operation may have already consumed the available stock.");
            }
        }

        foreach (var item in groupedItems)
        {
            var product = productsByCode[item.ProductCode];
            product.DebitStock(item.Quantity);
        }

        await _productRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Stock debited for {ProductsCount} products", groupedItems.Count);

        return new DebitStockResponseDto
        {
            Success = true,
            Message = "Stock debited successfully.",
            UpdatedProducts = products
                .OrderBy(x => x.Code)
                .Select(x => x.ToResponse())
                .ToList()
        };
    }

    private async Task<Product> GetExistingProductByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException($"Product with id '{id}' was not found.");
        }

        return product;
    }

    private static async Task ValidateAsync<T>(IValidator<T> validator, T instance, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            var message = string.Join("; ", result.Errors.Select(error => error.ErrorMessage));
            throw new AppValidationException(message);
        }
    }

    private void EnsureFailureScenarioNotTriggered(IEnumerable<string> productCodes)
    {
        if (productCodes.Any(code => string.Equals(code.Trim(), SimulatedFailureCode, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogError("Simulated stock service failure triggered for code {ProductCode}", SimulatedFailureCode);
            throw new SimulatedFailureException("A simulated failure occurred while processing stock.");
        }
    }
}
