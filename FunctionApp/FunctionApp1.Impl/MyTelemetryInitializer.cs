using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace FunctionApp1.Impl
{
    internal class MyTelemetryInitializer : ITelemetryInitializer
    {
        private const string ComputerNameKey = "COMPUTERNAME";
        private const string WebSiteInstanceIdKey = "WEBSITE_INSTANCE_ID";

        public MyTelemetryInitializer()
        {

        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null)
            {
                return;
            }
#pragma warning disable CS0618 // Type or member is obsolete
            if (telemetry.Context.Properties.TryGetValue("LogEventLevel", out string origLogLevel))
            {
                telemetry.Context.Properties["LogLevel"] = origLogLevel;
                telemetry.Context.Properties.Remove("LogEventLevel");
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
