using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Domain.Common;
using FluentValidation;

namespace Ecommerce.Pedido.Api.Application.Validators
{
    public class EnderecoDtoCreateValidator : AbstractValidator<EnderecoDtoCreate>
    {
        public EnderecoDtoCreateValidator()
        {
            RuleFor(x => x.Logradouro)
                .NotEmpty().WithMessage(AplicationMessages.DadosInvalidos);
            RuleFor(x => x.Numero)
                .NotEmpty().WithMessage(AplicationMessages.DadosInvalidos);
            RuleFor(x => x.Cidade)
                .NotEmpty().WithMessage(AplicationMessages.DadosInvalidos);
            RuleFor(x => x.Estado)
                .NotEmpty().WithMessage(AplicationMessages.DadosInvalidos);
            RuleFor(x => x.Cep)
                .NotEmpty().WithMessage(AplicationMessages.DadosInvalidos);
            RuleFor(x => x.Bairro)
                .NotEmpty().WithMessage(AplicationMessages.DadosInvalidos);
            RuleFor(x => x.complemento)
                .NotEmpty().WithMessage(AplicationMessages.DadosInvalidos);
        }
    }
}