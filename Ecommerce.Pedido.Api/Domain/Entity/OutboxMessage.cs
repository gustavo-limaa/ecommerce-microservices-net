namespace Ecommerce.Pedido.Api.Domain.Entity
{
    public class OutboxMessage
    {
        public Guid Id { get; private set; }
        public string Type { get; private set; } = string.Empty;
        public string Content { get; private set; } = string.Empty;
        public DateTime OccurredOnUtc { get; private set; }
        public Guid CorrelationId { get; private set; }
        public DateTime? ProcessedOnUtc { get; private set; }
        public string? Error { get; private set; }

        // Construtor privado para o Entity Framework Core
        private OutboxMessage()
        { }

        public OutboxMessage(string type, string content, Guid correlationId)
        {
            Id = Guid.NewGuid();
            Type = type;
            Content = content;
            OccurredOnUtc = DateTime.UtcNow;

            CorrelationId = correlationId;
        }

        public OutboxMessage(Guid id, string type, string content, DateTime occurredOnUtc, DateTime? processedOnUtc, Guid correlationId)
        {
            Id = id;
            Type = type;
            Content = content;
            OccurredOnUtc = occurredOnUtc;
            ProcessedOnUtc = processedOnUtc;
            CorrelationId = correlationId;
        }

        public void MarcarComoProcessado()
        {
            ProcessedOnUtc = DateTime.UtcNow;
            Error = null;
        }

        public void RegistrarFalha(string mensagemErro)
        {
            Error = mensagemErro;
        }
    }
}