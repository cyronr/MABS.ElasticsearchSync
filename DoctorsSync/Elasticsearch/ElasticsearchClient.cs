using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.ExceptionServices;
using Azure;
using DoctorsSync.Database.DataAccess;
using DoctorsSync.Models.Elasticsearch;

namespace DoctorsSync.Elasticsearch
{
    public class ElasticsearchClient : IElasticsearchClient
    {
        private readonly ILogger<ElasticsearchClient> _logger;
        private readonly IConfigurationSection _configElastic;
        private readonly IElasticClient _elasticClient;
        private readonly IDoctorRepository _doctorRepository;
        private readonly string Index;

        public ElasticsearchClient(ILogger<ElasticsearchClient> logger, IConfiguration config, IElasticClient elasticClient, IDoctorRepository doctorRepository)
        {
            _logger = logger;
            _elasticClient = elasticClient;
            _doctorRepository = doctorRepository;

            _configElastic = config.GetSection("Elasticsearch");
            if (_configElastic is null)
                throw new ArgumentNullException("No Elasticsearch section in appsettings.");

            Index = _configElastic.GetValue<string?>("Index");
            if (Index is null)
                throw new ArgumentNullException("No Index specified.");
        }

        public void DeleteDocuments(long[] ids)
        {
            _elasticClient.DeleteByQuery<Doctor>(del => del
                .Index(Index)
                .Query(q => q
                    .Terms(t => t
                        .Field(f => f.Id)
                        .Terms(ids)
                    )
                )
            );
            _doctorRepository.SetAsSynced(ids);


            _logger.LogInformation($"Deleted {ids.Length} documents.");
        }

        public void UpsertDocuments(List<Doctor> documents)
        {
            int backOffRetries = _configElastic.GetValue<int?>("BulkProperties:BackOffRetries") ?? 2;
            string backOffTime = _configElastic.GetValue<string?>("BulkProperties:BackOffTime") ?? "30s";
            int maxDegreeOfParallelism = _configElastic.GetValue<int?>("BulkProperties:MaxDegreeOfParallelism") ?? Environment.ProcessorCount;
            int size = _configElastic.GetValue<int?>("BulkProperties:Size") ?? 100;

            _logger.LogDebug($"Starting bulk:" +
                $"\nBackOffRetries: {backOffRetries}" +
                $"\nBackOffTime: {backOffTime}" +
                $"\nMaxDegreeOfParallelism: {maxDegreeOfParallelism}" +
                $"\nSize: {size}"
            );

            var bulkAll = _elasticClient.BulkAll(documents, b => b
                .Index(Index)
                .BackOffRetries(backOffRetries)
                .BackOffTime(backOffTime)
                .MaxDegreeOfParallelism(maxDegreeOfParallelism)
                .Size(size)
            );

            var waitHandle = new CountdownEvent(1);
            ExceptionDispatchInfo captureInfo = null;

            var subscription = bulkAll.Subscribe(new BulkAllObserver(
                onNext: response => {
                    _doctorRepository.SetAsSynced(response.Items.Select(i => Convert.ToInt64(i.Id)).ToArray());
                    _logger.LogInformation($"Upserted {response.Items.Count} documents.");
                },
                onError: e =>
                {
                    captureInfo = ExceptionDispatchInfo.Capture(e);
                    waitHandle.Signal();
                },
                onCompleted: () => waitHandle.Signal()
            ));

            waitHandle.Wait(TimeSpan.FromMinutes(30));
            if (captureInfo is not null && captureInfo.SourceException is not OperationCanceledException)
                _logger.LogError(captureInfo.SourceException.Message);
        }
    }
}
