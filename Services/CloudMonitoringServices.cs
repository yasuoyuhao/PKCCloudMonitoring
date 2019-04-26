using CloudMonitoring.Helpers;
using Google.Api;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Monitoring.V3;
using Google.Protobuf.WellKnownTypes;
using Grpc.Auth;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudMonitoring.Services
{
    public class CloudMonitoringServices
    {
        private readonly IConfiguration configuration;
        private readonly GoogleCredential googleCredential;
        private readonly Channel channel;
        private readonly MetricServiceClient client;
        private readonly string projectId;

        public CloudMonitoringServices(IConfiguration configuration)
        {
            this.configuration = configuration;
            var helper = new CloudMonitoringHelper(configuration);

            googleCredential = helper.GetGoogleCredential();
            channel = new Channel(
                MetricServiceClient.DefaultEndpoint.Host,
                MetricServiceClient.DefaultEndpoint.Port,
                googleCredential.ToChannelCredentials()
                );
            client = MetricServiceClient.Create(channel);
            projectId = helper.GetProjectId();
        }

        public void CreateTimeSeries()
        {
            ProjectName name = new ProjectName(projectId);

            // Prepare a data point. 
            Point dataPoint = new Point();
            TypedValue salesTotal = new TypedValue
            {
                DoubleValue = 123.45
            };
            dataPoint.Value = salesTotal;
            // Sets data point's interval end time to current time.
            Timestamp timeStamp = new Timestamp();
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            timeStamp.Seconds = (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
            TimeInterval interval = new TimeInterval
            {
                EndTime = timeStamp
            };
            dataPoint.Interval = interval;

            // Prepare custom metric.
            Metric metric = new Metric
            {
                Type = "custom.googleapis.com/shops/daily_sales"
            };
            metric.Labels.Add("store_id", "Pittsburgh");

            // Prepare monitored resource.
            MonitoredResource resource = new MonitoredResource
            {
                Type = "global"
            };
            resource.Labels.Add("project_id", projectId);

            // Create a new time series using inputs.
            TimeSeries timeSeriesData = new TimeSeries
            {
                Metric = metric,
                Resource = resource
            };
            timeSeriesData.Points.Add(dataPoint);

            // Add newly created time series to list of time series to be written.
            IEnumerable<TimeSeries> timeSeries = new List<TimeSeries> { timeSeriesData };
            // Write time series data.
            client.CreateTimeSeriesAsync(name, timeSeries);
        }

        ~CloudMonitoringServices()
        {
            channel.ShutdownAsync().Wait();
        }
    }
}
