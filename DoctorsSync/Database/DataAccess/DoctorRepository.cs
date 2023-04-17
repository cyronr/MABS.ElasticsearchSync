using DoctorsSync.Models.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DoctorsSync.Database.DataAccess
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly DbContextOptions<DataContext> _options;
        private readonly ILogger<DoctorRepository> _logger;
        private readonly IConfiguration _config;
        private readonly IConfigurationSection _configSQL;

        public DoctorRepository(DbContextOptions<DataContext> options, ILogger<DoctorRepository> logger, IConfiguration config)
        {
            _options = options;
            _logger = logger;
            _config = config;

            _configSQL = config.GetSection("SQL");
            if (_configSQL is null)
                throw new ArgumentNullException("No SQL section in appsettings.");
        }

        public List<Doctor> GetDoctors()
        {
            var command = _configSQL.GetValue<string?>("Command");
            if (command is null || command == "")
                throw new NullReferenceException("SQL.Command cannot be empty.");

            var timeoutInSeconds = _configSQL.GetValue<int?>("TimeoutInSeconds") ?? 600;

            _logger.LogDebug($"Fetching doctors from database." +
                $"\nCommand: {command}" +
                $"\nTimeoutInSeconds: {timeoutInSeconds}"
            );

            using (var context = new DataContext(_options, _config))
            {
                context.Database.SetCommandTimeout(TimeSpan.FromSeconds(timeoutInSeconds));
                var doctors = context.Doctors.FromSqlRaw(command).ToList();

                _logger.LogDebug($"Fetched {doctors.Count} from database.");
                return doctors;
            }
        }

        public void SetAsSynced(long[] ids)
        {
            _logger.LogDebug("Setting records as synced.");

            var command = $"update Doctors set synced_with_elasticsearch = 1 where Id in ({String.Join(", ", ids)})";
            var timeoutInSeconds = _configSQL.GetValue<int?>("TimeoutInSeconds") ?? 600;

            using (var context = new DataContext(_options, _config))
            {
                _logger.LogDebug($"Executing command:" +
                    $"\nCommand: {command}" +
                    $"\nnTimeoutInSeconds: {timeoutInSeconds}"
                );

                context.Database.SetCommandTimeout(TimeSpan.FromSeconds(timeoutInSeconds));
                context.Database.ExecuteSqlRaw(command);
            }
        }
    }
}
