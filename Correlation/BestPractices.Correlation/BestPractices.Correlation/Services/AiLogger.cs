using BestPractices.Correlation.Contracts;
using BestPractices.Correlation.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace BestPractices.Correlation.Services
{
    public class AiLogger : ILogger
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AppConfig appConfiguration;
        private TelemetryClient telemetryClient;

        public AiLogger(IHttpContextAccessor _httpContextAccessor, IOptions<AppConfig> _appConfiguration)
        {
            httpContextAccessor = _httpContextAccessor;
            appConfiguration = _appConfiguration.Value;
            telemetryClient = new TelemetryClient() { InstrumentationKey = appConfiguration.AiInstrumentationKey };
        }

        public string CorrelationId {
            get
            {
                return httpContextAccessor.HttpContext.Request.Headers["x-correlation-id"];
            }
        }

        public void LogEvent(string eventName)
        {
            var customProperties = new Dictionary<string, string> {
                { "CorrelationId", CorrelationId }
            };
            telemetryClient.TrackEvent(eventName, customProperties);
        }

        public void LogException(Exception ex)
        {
            var customProperties = new Dictionary<string, string> {
                { "CorrelationId", CorrelationId }
            };
            telemetryClient.TrackException(ex, customProperties);
        }
    }
}
