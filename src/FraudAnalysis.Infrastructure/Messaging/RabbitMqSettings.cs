namespace FraudAnalysis.Infrastructure.Messaging;

/// <summary>
/// Configurações do RabbitMQ lidas do appsettings.json (seção "RabbitMq").
/// </summary>
public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    /// <summary>Host do broker. Ex: localhost ou fraudanalysis.rabbitmq</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>Porta AMQP padrão.</summary>
    public int Port { get; set; } = 5672;

    /// <summary>Virtual host do RabbitMQ.</summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>Usuário de autenticação.</summary>
    public string Username { get; set; } = "guest";

    /// <summary>Senha de autenticação.</summary>
    public string Password { get; set; } = "guest";
}
