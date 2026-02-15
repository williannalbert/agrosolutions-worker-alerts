using AgroSolutions.Alerts.Application.Interfaces;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Alerts.Infrastructure.Messaging;

public class AwsSqsConsumer : IMessageConsumer
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AwsSqsConsumer> _logger;

    private readonly string _queueUrl;
    private readonly int _waitTimeSeconds;
    private readonly int _maxMessages;

    public AwsSqsConsumer(
        IAmazonSQS sqsClient,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<AwsSqsConsumer> logger)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _queueUrl = _configuration["AwsSettings:QueueUrl"]
                    ?? throw new ArgumentNullException("AwsSettings:QueueUrl não configurado.");

        _waitTimeSeconds = int.Parse(_configuration["AwsSettings:WaitTimeSeconds"] ?? "20");
        _maxMessages = int.Parse(_configuration["AwsSettings:MaxMessages"] ?? "10");
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando Long Polling na fila SQS: {Url}", _queueUrl);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = _maxMessages,
                    WaitTimeSeconds = _waitTimeSeconds, 
                    AttributeNames = new List<string> { "All" }
                };

                var response = await _sqsClient.ReceiveMessageAsync(request, cancellationToken);

                if (response.Messages.Count == 0) continue;

                foreach (var message in response.Messages)
                {
                    await ProcessSingleMessageAsync(message, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mensagens no SQS.");
                await Task.Delay(5000, cancellationToken);
            }
        }
    }

    private async Task ProcessSingleMessageAsync(Message message, CancellationToken token)
    {
        try
        {
            _logger.LogDebug("Processando mensagem SQS ID: {MessageId}", message.MessageId);
            var body = message.Body;

            // var snsWrapper = JsonNode.Parse(message.Body);
            // var body = snsWrapper["Message"].ToString();

            using (var scope = _serviceProvider.CreateScope())
            {
                var processor = scope.ServiceProvider.GetRequiredService<ITelemetryProcessingService>();
                await processor.ProcessMessageAsync(body);
            }

            await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem {MessageId}. A mensagem retornará para a fila após o VisibilityTimeout.", message.MessageId);
        }
    }
}