using Microsoft.Extensions.Options;

namespace SMSMicroserviceTests.ServiceTests;

public class NotificationServiceTests
{
    private readonly Mock<ISMSService> _smsService;
    private readonly Mock<ICacheService> _cacheService;
    private readonly IOptions<AppSettings> _appSettings;
    private readonly Mock<IEventBus> _eventBus;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _smsService =  new Mock<ISMSService>();
        _cacheService = new Mock<ICacheService>();
        _appSettings = Options.Create(new AppSettings(){});
        _eventBus = new Mock<IEventBus>();
        _notificationService = new NotificationService(_appSettings, _cacheService.Object, _smsService.Object, _eventBus.Object);
    }

    [Fact]
    public async Task HandleNotification_WhenIdempotenceKeyDoesNotExist_ShouldSendSMSAndSaveKey()
    {
        // Arrange
        var notificationQueue = new QueueModel
        {
            IdempotenceKey = Guid.NewGuid(),
            PhoneNumber = "1234567890",
            Message = "Test message",
            Subject = "Test subject"
        };


        _smsService.Setup(x => x.SendSMS(It.IsAny<SMSModel>())).ReturnsAsync(true);
        _cacheService.Setup(x => x.Get<bool>(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _notificationService.HandleNotification(notificationQueue);

        // Assert
        _smsService.Verify(x => x.SendSMS(It.IsAny<SMSModel>()), Times.Once);
        _cacheService.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task HandleNotification_WhenIdempotenceKeyExists_ShouldNotSendSMSAndNotSaveKey()
    {
        // Arrange
        var notificationQueue = new QueueModel
        {
            IdempotenceKey = Guid.NewGuid(),
            PhoneNumber = "1234567890",
            Message = "Test message",
            Subject = "Test subject"
        };

        _smsService.Setup(x => x.SendSMS(It.IsAny<SMSModel>())).ReturnsAsync(true);
        _cacheService.Setup(x => x.Get<bool>(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        await _notificationService.HandleNotification(notificationQueue);

        // Assert
        _smsService.Verify(x => x.SendSMS(It.IsAny<SMSModel>()), Times.Never);
        _cacheService.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task HandleNotification_WhenSMSSendingFails_ShouldThrowException()
    {
        // Arrange
        var notificationQueue = new QueueModel
        {
            IdempotenceKey = Guid.NewGuid(),
            PhoneNumber = "1234567890",
            Message = "Test message",
            Subject = "Test subject"
        };

        _smsService.Setup(x => x.SendSMS(It.IsAny<SMSModel>())).ReturnsAsync(false);

        _cacheService.Setup(x => x.Get<bool>(It.IsAny<string>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _notificationService.HandleNotification(notificationQueue));
    }

    [Fact]
    public async Task HandleNotification_WhenSMSSendingFails_ShouldNotSaveKey()
    {
        // Arrange
        var notificationQueue = new QueueModel
        {
            IdempotenceKey = Guid.NewGuid(),
            PhoneNumber = "1234567890",
            Message = "Test message",
            Subject = "Test subject"
        };

        _smsService.Setup(x => x.SendSMS(It.IsAny<SMSModel>())).ReturnsAsync(false);

        _cacheService.Setup(x => x.Get<bool>(It.IsAny<string>())).ReturnsAsync(false);

        // Act & Assert
        try
        {
            await _notificationService.HandleNotification(notificationQueue);
        }
        catch (Exception)
        {
            _cacheService.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan>()), Times.Never);
        }
    }

    [Fact]
    public async Task HandleNotification_WhenSMSSendingFails_ShouldNotPublishEvent()
    {
        // Arrange
        var notificationQueue = new QueueModel
        {
            IdempotenceKey = Guid.NewGuid(),
            PhoneNumber = "1234567890",
            Message = "Test message",
            Subject = "Test subject"
        };

        _smsService.Setup(x => x.SendSMS(It.IsAny<SMSModel>())).ReturnsAsync(false);

        _cacheService.Setup(x => x.Get<bool>(It.IsAny<string>())).ReturnsAsync(false);

        // Act & Assert
        try
        {
            await _notificationService.HandleNotification(notificationQueue);
        }
        catch (Exception)
        {
            _eventBus.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }

    [Fact]
    public async Task HandleNotification_WhenSMSSendingSucceeds_ShouldPublishEvent()
    {
        // Arrange
        var notificationQueue = new QueueModel
        {
            IdempotenceKey = Guid.NewGuid(),
            PhoneNumber = "1234567890",
            Message = "Test message",
            Subject = "Test subject"
        };

        _smsService.Setup(x => x.SendSMS(It.IsAny<SMSModel>())).ReturnsAsync(true);

        _cacheService.Setup(x => x.Get<bool>(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _notificationService.HandleNotification(notificationQueue);

        // Assert
        _eventBus.Verify(x => x.PublishAsync(It.IsAny<string>(),It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task HandleNotification_WhenSMSSendingSucceeds_ShouldNotThrowException()
    {
        // Arrange
        var notificationQueue = new QueueModel
        {
            IdempotenceKey = Guid.NewGuid(),
            PhoneNumber = "1234567890",
            Message = "Test message",
            Subject = "Test subject"
        };

        _smsService.Setup(x => x.SendSMS(It.IsAny<SMSModel>())).ReturnsAsync(true);

        _cacheService.Setup(x => x.Get<bool>(It.IsAny<string>())).ReturnsAsync(false);

        // Act & Assert
        await _notificationService.HandleNotification(notificationQueue);
    }

    [Fact]
    public async Task HandleNotification_WhenSMSSendingSucceeds_ShouldSaveKey()
    {
        // Arrange
        var notificationQueue = new QueueModel
        {
            IdempotenceKey = Guid.NewGuid(),
            PhoneNumber = "1234567890",
            Message = "Test message",
            Subject = "Test subject"
        };

        _smsService.Setup(x => x.SendSMS(It.IsAny<SMSModel>())).ReturnsAsync(true);

        _cacheService.Setup(x => x.Get<bool>(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _notificationService.HandleNotification(notificationQueue);

        // Assert
        _cacheService.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan>()), Times.Once);
    }
}