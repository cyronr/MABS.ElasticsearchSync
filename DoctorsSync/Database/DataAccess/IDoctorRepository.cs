using DoctorsSync.Models;

namespace DoctorsSync.Database.DataAccess
{
    public interface IDoctorRepository
    {
        List<DbDoctor> GetDoctors();
        void SetAsSynced(long[] ids);
    }
}
