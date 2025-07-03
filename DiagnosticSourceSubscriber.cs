// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace OpenTelemetry.Instrumentation
{
    internal abstract class ListenerHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListenerHandler"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the <see cref="ListenerHandler"/>.</param>
        protected ListenerHandler(string sourceName)
        {
            this.SourceName = sourceName;
        }

        /// <summary>
        /// Gets the name of the <see cref="ListenerHandler"/>.
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ListenerHandler"/> supports NULL <see cref="Activity"/>.
        /// </summary>
        public virtual bool SupportsNullActivity { get; }

        /// <summary>
        /// Method called for an event which does not have 'Start', 'Stop' or 'Exception' as suffix.
        /// </summary>
        /// <param name="name">Custom name.</param>
        /// <param name="payload">An object that represent the value being passed as a payload for the event.</param>
        public virtual void OnEventWritten(string name, object payload)
        {
        }
    }

    internal sealed class DiagnosticSourceSubscriber : IDisposable, IObserver<DiagnosticListener>
    {
        private readonly List<IDisposable> listenerSubscriptions;
        private readonly Func<string, ListenerHandler> handlerFactory;
        private readonly Func<DiagnosticListener, bool> diagnosticSourceFilter;
        private readonly Func<string, object, object, bool> isEnabledFilter;
        private readonly Action<string, string, Exception> logUnknownException;
        private long disposed;
        private IDisposable allSourcesSubscription;

        public DiagnosticSourceSubscriber(
            ListenerHandler handler,
            Func<string, object, object, bool> isEnabledFilter,
            Action<string, string, Exception> logUnknownException)
            : this(_ => handler, value => handler.SourceName == value.Name, isEnabledFilter, logUnknownException)
        {
        }

        public DiagnosticSourceSubscriber(
            Func<string, ListenerHandler> handlerFactory,
            Func<DiagnosticListener, bool> diagnosticSourceFilter,
            Func<string, object, object, bool> isEnabledFilter,
            Action<string, string, Exception> logUnknownException)
        {
            //Guard.ThrowIfNull(handlerFactory);

            this.listenerSubscriptions = new List<IDisposable>();
            this.handlerFactory = handlerFactory;
            this.diagnosticSourceFilter = diagnosticSourceFilter;
            this.isEnabledFilter = isEnabledFilter;
            this.logUnknownException = logUnknownException;
        }

        public void Subscribe()
        {
            this.allSourcesSubscription = this.allSourcesSubscription == null ? DiagnosticListener.AllListeners.Subscribe(this) : this.allSourcesSubscription;
        }

        public void OnNext(DiagnosticListener value)
        {
            if ((Interlocked.Read(ref this.disposed) == 0) &&
                this.diagnosticSourceFilter(value))
            {
                var handler = this.handlerFactory(value.Name);
                var listener = new DiagnosticSourceListener(handler, this.logUnknownException);
                var subscription = this.isEnabledFilter == null ?
                    value.Subscribe(listener) :
                    value.Subscribe(listener, this.isEnabledFilter);

                lock (this.listenerSubscriptions)
                {
                    this.listenerSubscriptions.Add(subscription);
                }
            }
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref this.disposed, 1, 0) == 1)
            {
                return;
            }

            lock (this.listenerSubscriptions)
            {
                if (disposing)
                {
                    foreach (var listenerSubscription in this.listenerSubscriptions)
                    {
                        listenerSubscription?.Dispose();
                    }
                }

                this.listenerSubscriptions.Clear();
            }

            if (disposing)
            {
                this.allSourcesSubscription?.Dispose();
            }

            this.allSourcesSubscription = null;
        }
    }

    internal sealed class DiagnosticSourceListener : IObserver<KeyValuePair<string, object>>
    {
        private readonly ListenerHandler handler;

        private readonly Action<string, string, Exception> logUnknownException;

        public DiagnosticSourceListener(ListenerHandler handler, Action<string, string, Exception> logUnknownException)
        {
            //Guard.ThrowIfNull(handler);

            this.handler = handler;
            this.logUnknownException = logUnknownException;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (!this.handler.SupportsNullActivity && Activity.Current == null)
            {
                return;
            }

            try
            {
                this.handler.OnEventWritten(value.Key, value.Value);
            }
            catch (Exception ex)
            {
                this.logUnknownException?.Invoke(this.handler.SourceName, value.Key, ex);
            }
        }
    }
}
