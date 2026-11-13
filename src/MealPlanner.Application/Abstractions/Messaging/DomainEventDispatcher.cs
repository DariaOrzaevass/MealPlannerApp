using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MealPlanner.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Application.Abstractions.Messaging;

// Finds domain event handlers and invokes them
internal sealed class DomainEventDispatcher(
    IServiceProvider serviceProvider,
    ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, HandlerInvoker> _invokers = new();

    private delegate Task HandlerInvoker(
        object handler,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken);

    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            Type eventType = domainEvent.GetType();
            Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

            // GetServices, not GetService, because there can be multiple handlers for an event
            foreach (object? handler in serviceProvider.GetServices(handlerType))
            {
                if (handler is null)
                {
                    continue;
                }

                HandlerInvoker invoke = _invokers.GetOrAdd(eventType, BuildInvoker);

                try
                {
                    await invoke(handler, domainEvent, cancellationToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    // The event describes a fact that has happened already
                    // and cannot be undone. Failure of the side effect
                    // should not turn successfull operation into failure.
                    logger.LogError(
                        exception,
                        "Handler {Handler} for event {Event} finished with error.",
                        handler.GetType().Name,
                        eventType.Name);
                }
            }
        }
    }

    // Creates the delegate that can invoke Handle() for a specific time of domain event.
    // It is done once per type and then calls use the stored delegate.
    private static HandlerInvoker BuildInvoker(Type eventType)
    {
        MethodInfo method = typeof(IDomainEventHandler<>)
            .MakeGenericType(eventType)
            .GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!;

        return (handler, domainEvent, cancellationToken) =>
            (Task)method.Invoke(handler, [domainEvent, cancellationToken])!;
    }
}
