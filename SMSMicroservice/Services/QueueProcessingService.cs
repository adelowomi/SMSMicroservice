namespace SMSMicroservice;

using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public class QueueProcessingService : IQueueProcessingService
{
    private readonly AppSettings _appSettings;
    private readonly IQueueClient _queueClient;
    private readonly INotificationService _notificationService;
    private readonly ILogger<QueueProcessingService> _logger;

    public QueueProcessingService(IOptions<AppSettings> appSettings, IQueueClient queueClient, INotificationService notificationService, ILogger<QueueProcessingService> logger)
    {
        _appSettings = appSettings.Value;
        _queueClient = queueClient;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ReceiveMessage()
    {
        var processor = _queueClient.CreateProcessor(_appSettings.NotificationQueue, new QueueProcessorOptions() { AutoCompleteMessages = false });
        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;
        await processor.StartProcessingAsync();
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error occurred while processing message");
        return Task.CompletedTask;
    }

    private async Task MessageHandler(ProcessEventArgs args)
    {
        var message = args.Message;
        var deserializedMessage = JsonSerializer.Deserialize<QueueModel>(message.Body.ToString());
        if (deserializedMessage != null) await _notificationService.HandleNotification(deserializedMessage);
        await args.CompleteMessageAsync(args.Message);
    }
}

