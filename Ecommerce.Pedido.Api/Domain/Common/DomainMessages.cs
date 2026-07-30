namespace Ecommerce.Pedido.Api.Domain.Common
{
    public static class DomainMessages
    {
        public static class EnderecoMSG
        {
            public const string DadosInvalidos = "Os Dados Informados Estao Invalidados Ou Faltando Porfavor Preencher Todos Os Dados";
        }

        public static class CpfMSG
        {
            public const string Obrigatorio = "O CPF é de preenchimento obrigatório.";
            public const string TamanhoInvalido = "O CPF deve conter exatamente 11 dígitos numéricos.";
            public const string Invalido = "O CPF informado é matematicamente inválido.";
        }

        public static class EmailMSG
        {
            public const string Obrigatorio = "O e-mail é de preenchimento obrigatório.";
            public const string FormatoInvalido = "O e-mail informado possui um formato inválido.";
        }

        public static class ItemPedidoMSG
        {
            public const string QuantidadeInvalida = "A quantidade do item deve ser maior que zero.";
            public const string NomeObrigatorio = "O nome do produto é obrigatório.";
        }

        public static class ValorMonetarioMSG
        {
            public const string ValorNegativo = "O valor monetário não pode ser negativo.";
        }

        public static class PedidoMSG
        {
            public const string PedidoInvalido = "O Pedido Esta Invalidado Porfavor Rever Os Dados";
            public const string EnderecoObrigatorio = "O Endereço e Obrigatorio";
            public const string ClienteInvalido = " O ID Informado Do Cliente Esta Invalido ";
            public const string AlteracaoNaoPermitida = "Apos O Envio das Inforemançao Nao Pode Ser Parado, So Apos O Termino Da Açao";
        }
    }
}