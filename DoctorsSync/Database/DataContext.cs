using DoctorsSync.Database.DataAccess;
using DoctorsSync.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DoctorsSync.Database
{
    public class DataContext : DbContext
    {
        private readonly IConfiguration _config;

        public DbSet<DbDoctor> Doctors { get; set; }

        public DataContext(DbContextOptions<DataContext> options, IConfiguration config) : base(options)
        {
            _config = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));
        }
    }
}
