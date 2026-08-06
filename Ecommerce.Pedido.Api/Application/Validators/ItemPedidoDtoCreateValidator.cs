using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Domain.Common;
using FluentValidation;

namespace Ecommerce.Pedido.Api.Application.Validators
{
    public class ItemPedidoDtoCreateValidator : AbstractValidator<ItemPedidoDtoCreate>
    {
        public ItemPedidoDtoCreateValidator()
        {
            RuleFor(x => x.ProdutoId)
                .NotEmpty().WithMessage(ApplicationMessages.DadosInvalidos)
                .NotNull().WithMessage(ApplicationMessages.DadosInvalidos);

            RuleFor(x => x.Quantidade)
                .GreaterThan(0).WithMessage(ApplicationMessages.DadosInvalidos);
            RuleFor(x => x.PrecoUnitario)
                .GreaterThan(0).WithMessage(ApplicationMessages.DadosInvalidos);
            RuleFor(x => x.NomeProduto)
                .NotEmpty().WithMessage(ApplicationMessages.DadosInvalidos)
                .NotNull().WithMessage(ApplicationMessages.DadosInvalidos);
        }
    }
}