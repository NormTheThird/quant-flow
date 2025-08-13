global using Microsoft.Extensions.Options;
global using QuantFlow.Common.ExternalModels.Kraken;
global using QuantFlow.Common.Models;
global using QuantFlow.Common.Interfaces.Services;
global using QuantFlow.Domain.Services;
global using QuantFlow.Domain.Services.ApiServices;
global using QuantFlow.WorkerService.DataCollection.Configuration;
global using QuantFlow.WorkerService.DataCollection.Interfaces;
global using QuantFlow.WorkerService.DataCollection.Models;
global using QuantFlow.WorkerService.DataCollection.Services;

global using QuantFlow.Data.InfluxDB.Extensions;
global using Serilog;
global using Serilog.Events;
global using Serilog.Sinks.Grafana.Loki;
global using QuantFlow.Data.SQLServer.Extensions;

global using QuantFlow.Domain.Extensions;
global using QuantFlow.Common.Enumerations;