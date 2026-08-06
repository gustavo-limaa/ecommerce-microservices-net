namespace Ecommerce.Pedido.Api.Domain.Common
{
    public class ApplicationMessages
    {
        public const string NaoEncontrado = "The requested resource was not found.";
        public const string Conflito = "A conflict occurred with the current state of the resource.";
        public const string DadosInvalidos = "Invalid data provided.";
        public const string SemPermissao = "You do not have permission to perform this action.";
        public const string SemAutorizacao = "Authentication is required to access this resource.";
        public const string ErroBancoDeDados = "A database error occurred while processing the request.";
        public const string ErroInesperado = "An unexpected error occurred. Please try again later.";

        public static class Pedido
        {
            public const string NaoEncontrado = "Order not found with the provided ID.";
            public const string FalhaCriacao = "Failed to create the order.";
            public const string StatusInvalidoParaAtualizacao = "The order cannot be updated due to its current status.";
        }
    }
}