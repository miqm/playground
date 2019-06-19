using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

namespace FunctionApp1.Impl
{
    internal class MyTelemetryConverter : TelemetryConverterBase
    {
        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            if (logEvent == null)
                throw new ArgumentNullException(nameof(logEvent));

            if (logEvent.Exception == null)
            {
                var renderedMessage = logEvent.RenderMessage(formatProvider);

                var telemetry = new TraceTelemetry(renderedMessage)
                {
                    Timestamp = logEvent.Timestamp,
                    SeverityLevel = ToSeverityLevel(logEvent.Level)
                };

                // write logEvent's .Properties to the AI one
                ForwardPropertiesToTelemetryProperties(logEvent, telemetry, formatProvider);

                yield return telemetry;
            }
            else
            {
                yield return ToExceptionTelemetry(logEvent, formatProvider);
            }
        }

        public override void ForwardPropertiesToTelemetryProperties(LogEvent logEvent,
            ISupportProperties telemetryProperties,
            IFormatProvider formatProvider)
        {
            ForwardPropertiesToTelemetryProperties(logEvent, telemetryProperties, formatProvider,
                includeLogLevel: false,
                includeRenderedMessage: false,
                includeMessageTemplate: false);
        }
    }
}
