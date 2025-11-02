
using Azure.Core;
using Microsoft.Extensions.Hosting;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Common.Logs;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using ServiceGenerator;
using System.Diagnostics;
using System.Text.Json;

namespace MyCleanTemplate.Infrastructure.Messaging
{
    [SingletonDependency<IHostedService>]
    internal class HandleNotificationsWorker(NotificationChannel notificationChannel, IServiceProvider serviceProvider)
        : BackgroundService
    {
        private const string HandlerName = nameof(INotificationHandler<INotification>.Handle);
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = notificationChannel.ReadFromChannel(stoppingToken);
                var enumerator = result.GetAsyncEnumerator(stoppingToken);
                while (await enumerator.MoveNextAsync())
                {
                    var notificationHandlerType = typeof(INotificationHandler<>).MakeGenericType(enumerator.Current.Type);
                    var handler = serviceProvider.GetService(notificationHandlerType);
                    if (handler is not null)
                    {
                        await ConsumeAndTelemetry(enumerator.Current, handler, stoppingToken);
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }

        }

        private async Task ConsumeAndTelemetry(NotificationPublishContext context,object handler, CancellationToken stoppingToken)
        {
            var parentContext = Propagators.DefaultTextMapPropagator.Extract(default, context.Properties,
                (properties, key) =>
                {
                    if (properties.TryGetValue(key, out var value))
                    {
                        return [value];
                    }

                    return [];
                });
            Baggage.Current = parentContext.Baggage;
            using var activity = TelemetryHelper.MainActivitySource.StartActivity(
                TelemetryHelper.TelemetrySpanLevelMediator, ActivityKind.Consumer, parentContext.ActivityContext);
            try
            {
                activity?.SetTag("Mediator.Action.Type", "Consumer");
                activity?.SetTag("Mediator.Notification.Type", context.Type.ToString());
                activity?.SetTag("Mediator.Notification.Value", JsonSerializer.Serialize(context.Message, context.Type));
                await ((Task)handler!.GetType().GetMethod(HandlerName)!.Invoke(handler, [context.Message, stoppingToken]))!;
            }
            catch (Exception e)
            {
                activity?.AddException(e);
                activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                notificationChannel.AddToChannel(context.Message,activity, stoppingToken);
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {

            await base.StopAsync(stoppingToken);
        }
    }
}
