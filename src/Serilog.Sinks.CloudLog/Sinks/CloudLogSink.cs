﻿using System;
using System.Collections.Generic;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.PeriodicBatching;
using Serilog.Debugging;
using Anexia.BDP.CloudLog;
using System.IO;
using Serilog.Sinks.CloudLog.Formatting;

namespace Serilog.Sinks.CloudLog
{
    public class CloudLogSink : PeriodicBatchingSink
    {
        private Client client;
        private CloudLogJsonFormatter formatter;

        const int batchSize = 50;
        const int period = 1;

        public CloudLogSink(string index, string caFile, string certFile, string keyFile, string keyPassword = "") : base(batchSize, TimeSpan.FromSeconds(period))
        {
            try
            {
                client = new Client(index, caFile, certFile, keyFile, keyPassword);
                Init();
            }
            catch (Exception exception)
            {
                SelfLog.WriteLine("Unable to create CloudLogSink for index {0}: {1}", index, exception);
            }
        }

        public CloudLogSink(string index, string token) : base(batchSize, TimeSpan.FromSeconds(period))
        {
            try
            {
                client = new Client(index, token);
                Init();
            }
            catch (Exception exception)
            {
                SelfLog.WriteLine("Unable to create CloudLogSink for index {0}: {1}", index, exception);
            }
        }

        private void Init()
        {
            formatter = new CloudLogJsonFormatter();
            client.SetClientType("dotnet-client-serilog");
        }

        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            foreach (var e in events)
            {
                var sw = new StringWriter();
                formatter.Format(e, sw);
                client.PushEvent(sw.ToString());
            }
        }
    }
}
