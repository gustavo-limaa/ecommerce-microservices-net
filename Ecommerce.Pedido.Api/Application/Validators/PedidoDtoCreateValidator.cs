using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Domain.Common;
using FluentValidation;

namespace Ecommerce.Pedido.Api.Application.Validators;

public class PedidoDtoCreateValidator : AbstractValidator<PedidoDtoCreate>
{
    public PedidoDtoCreateValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage(AplicationMessages.NaoEncontrado
            );

        RuleFor(x => x.CpfCliente)
            .NotEmpty().WithMessage(AplicationMessages.DadosInvalidos)
            .Must(ValidarFormatoCpf).WithMessage(AplicationMessages.Conflito
            );

        RuleFor(x => x.Itens)
            .NotNull().WithMessage(AplicationMessages.DadosInvalidos)
            .NotEmpty().WithMessage(AplicationMessages.DadosInvalidos);

        RuleFor(x => x.EnderecoEntrega)
            .NotNull().WithMessage(AplicationMessages.DadosInvalidos)
            .SetValidator(new EnderecoDtoCreateValidator());

        RuleForEach(x => x.Itens)
            .SetValidator(new ItemPedidoDtoCreateValidator());
    }

    private bool ValidarFormatoCpf(string cpf)
    {
        return !string.IsNullOrWhiteSpace(cpf) && cpf.Length == 11;
    }
}