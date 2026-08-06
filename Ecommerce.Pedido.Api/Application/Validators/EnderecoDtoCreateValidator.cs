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
                .NotEmpty().WithMessage(ApplicationMessages.DadosInvalidos);
            RuleFor(x => x.Numero)
                .NotEmpty().WithMessage(ApplicationMessages.DadosInvalidos);
            RuleFor(x => x.Cidade)
                .NotEmpty().WithMessage(ApplicationMessages.DadosInvalidos);
            RuleFor(x => x.Estado)
                .NotEmpty().WithMessage(ApplicationMessages.DadosInvalidos);
            RuleFor(x => x.Cep)
                .NotEmpty().WithMessage(ApplicationMessages.DadosInvalidos);
            RuleFor(x => x.Bairro)
                .NotEmpty().WithMessage(ApplicationMessages.DadosInvalidos);
            RuleFor(x => x.complemento)
                .NotEmpty().WithMessage(ApplicationMessages.DadosInvalidos);
        }
    }
}