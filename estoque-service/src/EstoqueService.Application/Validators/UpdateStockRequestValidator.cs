using EstoqueService.Application.DTOs.Requests;
using FluentValidation;

namespace EstoqueService.Application.Validators;

public class UpdateStockRequestValidator : AbstractValidator<UpdateStockRequestDto>
{
    public UpdateStockRequestValidator()
    {
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0);
    }
}
