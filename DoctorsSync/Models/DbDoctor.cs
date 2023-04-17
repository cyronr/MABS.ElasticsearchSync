namespace DoctorsSync.Models
{
    public class DbDoctor
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }
        public DbDoctorStatus Status { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string TitleShortName { get; set; } = null!;
        public string TitleName { get; set; } = null!;
        public string Specalities { get; set; } = null!;
    }
}
