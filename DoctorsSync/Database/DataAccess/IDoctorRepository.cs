using DoctorsSync.Models.Db;

namespace DoctorsSync.Database.DataAccess
{
    public interface IDoctorRepository
    {
        List<Doctor> GetDoctors();
        void SetAsSynced(long[] ids);
    }
}
