// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using OpenTelemetry.Instrumentation.SqlClient.Implementation;
using System;
using System.Threading;

namespace OpenTelemetry.Instrumentation.SqlClient
{
    /// <summary>
    /// SqlClient instrumentation.
    /// </summary>
    internal sealed class SqlClientInstrumentation : IDisposable
    {
        public static readonly SqlClientInstrumentation Instance = new SqlClientInstrumentation();

        internal const string SqlClientDiagnosticListenerName = "SqlClientDiagnosticListener";

        internal static int MetricHandles;
        internal static int TracingHandles;

        private readonly SqlEventSourceListener sqlEventSourceListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlClientInstrumentation"/> class.
        /// </summary>
        private SqlClientInstrumentation()
        {
            this.sqlEventSourceListener = new SqlEventSourceListener();
        }

        public static SqlClientTraceInstrumentationOptions TracingOptions { get; set; } = new SqlClientTraceInstrumentationOptions();

        public static IDisposable AddMetricHandle() => new MetricHandle();

        public static IDisposable AddTracingHandle() => new TracingHandle();

        /// <inheritdoc/>
        public void Dispose()
        {
            this.sqlEventSourceListener?.Dispose();
        }

        private sealed class MetricHandle : IDisposable
        {
            private bool disposed;

            public MetricHandle()
            {
                Interlocked.Increment(ref MetricHandles);
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    Interlocked.Decrement(ref MetricHandles);
                    this.disposed = true;
                }
            }
        }

        private sealed class TracingHandle : IDisposable
        {
            private bool disposed;

            public TracingHandle()
            {
                Interlocked.Increment(ref TracingHandles);
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    Interlocked.Decrement(ref TracingHandles);
                    this.disposed = true;
                }
            }
        }
    }
}
