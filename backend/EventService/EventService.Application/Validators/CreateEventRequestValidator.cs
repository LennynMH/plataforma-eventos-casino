using FluentValidation;
using EventService.Application.DTOs;

namespace EventService.Application.Validators;

public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del evento es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha del evento es obligatoria")
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha del evento debe ser futura");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("El lugar del evento es obligatorio")
            .MaximumLength(500).WithMessage("El lugar no puede exceder 500 caracteres");

        RuleFor(x => x.Zones)
            .NotEmpty().WithMessage("Debe haber al menos una zona")
            .Must(zones => zones.Count > 0).WithMessage("Debe haber al menos una zona");

        RuleForEach(x => x.Zones)
            .SetValidator(new ZoneRequestValidator());
    }
}

public class ZoneRequestValidator : AbstractValidator<ZoneRequest>
{
    public ZoneRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la zona es obligatorio")
            .MaximumLength(100).WithMessage("El nombre de la zona no puede exceder 100 caracteres");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("El precio debe ser mayor o igual a 0");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("La capacidad debe ser mayor a 0");
    }
}
