
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Common.Logs;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using ServiceGenerator;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;

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

                    var behaviorType = typeof(INotificationHandlerBehavior<>).MakeGenericType(enumerator.Current.Type);
                    var behaviors = serviceProvider.GetServices(behaviorType);
                    var response = behaviors.Reverse()
                        .Aggregate(() => ConsumeAndTelemetry(enumerator.Current, handler, stoppingToken),
                            (next, behavior) => () =>
                                (Task)behavior.GetType()
                                    .GetMethod(nameof(INotificationHandlerBehavior<INotification>.Handle))!
                                    .Invoke(behavior, [enumerator.Current.Message, next, stoppingToken])).Invoke();
                    await response;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
        }

        private async Task ConsumeAndTelemetry(NotificationPublishContext context, object handler, CancellationToken stoppingToken)
        {
            try
            {
                await ((Task)handler!.GetType().GetMethod(HandlerName)!.Invoke(handler, [context.Message, stoppingToken]))!;
            }
            catch (Exception e)
            {
                notificationChannel.AddToChannel(context.Message, stoppingToken);
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            while (notificationChannel.Count > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
            await base.StopAsync(stoppingToken);
        }
    }
}
