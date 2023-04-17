using DoctorsSync.Database.DataAccess;
using DoctorsSync.Elasticsearch;
using DoctorsSync.Models.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DoctorsSync
{
    public class DoctorsSyncService
    {
        private readonly ILogger<DoctorsSyncService> _logger;
        private readonly Timer _timer;
        private readonly IConfiguration _config;
        private readonly IDoctorRepository _dataAccess;
        private readonly IElasticsearchClient _elasticsearch;

        public DoctorsSyncService(
            ILogger<DoctorsSyncService> logger,
            IConfiguration config,
            IDoctorRepository dataAccess,
            IElasticsearchClient elasticsearch)
        {
            _logger = logger;
            _config = config;

            int RefreshTimeInMiliseconds = _config.GetValue<int>("AppSettings:RefreshTimeInMiliseconds");
            _timer = new Timer(RefreshTimeInMiliseconds) { AutoReset = true };
            _timer.Elapsed += _timer_Elapsed;
            _dataAccess = dataAccess;
            _elasticsearch = elasticsearch;
        }

        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            _logger.LogInformation("Synchronizing SQL DB with Elastic DB.");

            try
            {
                _timer.Stop();

                var data = _dataAccess.GetDoctors();
                _logger.LogInformation($"Found ${data.Count} to synchronize.");

                var doctorsToUpsert = data
                    .Where(d => d.Status == DoctorStatus.Active)
                    .Select(x => Models.Elasticsearch.Doctor.MapFromDbDoctor(x))
                    .ToList();
                if (doctorsToUpsert.Count != 0)
                    _elasticsearch.UpsertDocuments(doctorsToUpsert);

                var doctorsToDelete = data
                    .Where(d => d.Status == DoctorStatus.Deleted)
                    .Select(x => Models.Elasticsearch.Doctor.MapFromDbDoctor(x))
                    .ToList();
                if (doctorsToDelete.Count != 0)
                    _elasticsearch.DeleteDocuments(doctorsToDelete.Select(d => d.Id).ToArray());

                _timer.Start();
            }
            catch(Exception ex)
            {
                _logger.LogCritical($"{ex}");
            }
        }

        public void Start()
        {
            _logger.LogInformation("Service started.");
            _timer.Start();
        }

        public void Stop()
        {
            _logger.LogInformation("Service stoped.");
            _timer.Stop();
        }
    }
}
