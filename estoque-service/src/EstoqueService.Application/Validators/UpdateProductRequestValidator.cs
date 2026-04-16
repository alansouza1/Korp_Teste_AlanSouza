using EstoqueService.Application.DTOs.Requests;
using FluentValidation;

namespace EstoqueService.Application.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequestDto>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(255);
    }
}
