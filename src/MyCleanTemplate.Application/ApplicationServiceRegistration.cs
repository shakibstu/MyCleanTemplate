using System.Collections.Concurrent;
using FluentValidation;
using MediatRR;
using MediatRR.ServiceGenerator;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Application.Contract.Abstractions;
using MyCleanTemplate.Application.Test.NotificationHandler;
using MyCleanTemplate.Domain.Events;
using ServiceGenerator;

namespace MyCleanTemplate.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.InjectFromSourceGenerator();
        services.AddValidatorsFromAssemblyContaining<IApplicationContractAssemblyMarker>();
        services.AutoRegisterRequestHandlers();
        services.AddMediatRR(a=>a.NotificationChannelSize = 100, new ConcurrentQueue<DeadLettersInfo>());
        services.AddNotificationHandler<TestAddedEvent, TestAddedEventNotificationHandler>(null);
        return services;
    }
}