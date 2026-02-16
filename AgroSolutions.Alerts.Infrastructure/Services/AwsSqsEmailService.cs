using AgroSolutions.Alerts.Application.DTOs.Integration;
using AgroSolutions.Alerts.Application.Interfaces;
using AgroSolutions.Alerts.Domain.Entities;
using AgroSolutions.Alerts.Domain.Enums;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgroSolutions.Alerts.Infrastructure.Services;

public class AwsSqsEmailService : INotificationService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AwsSqsEmailService> _logger;
    private readonly string _emailQueueUrl;

    public AwsSqsEmailService(
        IAmazonSQS sqsClient,
        IConfiguration configuration,
        ILogger<AwsSqsEmailService> logger)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _logger = logger;

        _emailQueueUrl = _configuration["AwsSettings:EmailQueueUrl"]
                         ?? throw new ArgumentNullException("AwsSettings:EmailQueueUrl não configurada.");
    }

    public async Task NotifyAsync(Alert alert)
    {
        var emailPayload = new SendEmailEventDto
        {
            To = "email.email@gmail.com", 
            Subject = $"[{alert.Severity.ToString().ToUpper()}] Alerta AgroSolutions: {alert.DeviceId}",
            HtmlBody = BuildHtmlBody(alert),
            TextBody = $"Alerta: {alert.Message}. Ação: {alert.SuggestedFieldStatus}"
        };

        try
        {
            var jsonBody = JsonSerializer.Serialize(emailPayload);

            var request = new SendMessageRequest
            {
                QueueUrl = _emailQueueUrl,
                MessageBody = jsonBody
            };

            var response = await _sqsClient.SendMessageAsync(request);

            _logger.LogInformation("Solicitação de e-mail enviada para fila SQS. MessageId: {MessageId}", response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar mensagem para fila de e-mail.");
        }
    }

    private string BuildHtmlBody(Alert alert)
    {
        var color = alert.Severity == AlertSeverity.Critical ? "#d32f2f" : "#f57c00"; 
        return $@"
            <div style='font-family: Arial, sans-serif; border: 1px solid #ccc; padding: 20px; border-radius: 8px;'>
                <h2 style='color: {color};'>Novo Alerta Identificado</h2>
                <p><strong>Sensor:</strong> {alert.DeviceId}</p>
                <p><strong>Severidade:</strong> {alert.Severity}</p>
                <p style='font-size: 1.1em;'><strong>Mensagem:</strong> {alert.Message}</p>
                <hr />
                <p><strong>Ação Recomendada:</strong> {alert.SuggestedFieldStatus}</p>
                <p><small>Gerado em: {alert.GeneratedAt}</small></p>
            </div>";
    }
}