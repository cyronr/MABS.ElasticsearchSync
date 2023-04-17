using Newtonsoft.Json;
using DbDoctor = DoctorsSync.Models.Db.Doctor;

namespace DoctorsSync.Models.Elasticsearch
{
    public class Doctor
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string TitleShortName { get; set; } = null!;
        public string TitleName { get; set; } = null!;
        public List<Specialty> Specalities { get; set; } = new List<Specialty>();
        public List<Facility> Facilities { get; set; } = new List<Facility>();


        public static Doctor MapFromDbDoctor(DbDoctor dbDoctor)
        {
            var esDoctor = new Doctor
            {
                Id = dbDoctor.Id,
                UUID = dbDoctor.UUID,
                FirstName = dbDoctor.FirstName,
                LastName = dbDoctor.LastName,
                TitleShortName = dbDoctor.TitleShortName,
                TitleName = dbDoctor.TitleName
            };

            esDoctor.Specalities = MapSpecialties(dbDoctor.Specalities);
            esDoctor.Facilities = MapFacilities(dbDoctor.Facilities);

            return esDoctor;
        }

        private static List<Specialty> MapSpecialties(string jsonSpecialties)
        {
            var specialties = new List<Specialty>();

            var jsonData = JsonConvert.DeserializeObject<dynamic>(jsonSpecialties);
            foreach (var data in jsonData)
            {
                specialties.Add(new Specialty{
                    Id = data.Id,
                    Name = data.Name
                });
            }

            return specialties;
        }

        private static List<Facility> MapFacilities(string jsonSpecialties)
        {
            var facilities = new List<Facility>();

            var jsonData = JsonConvert.DeserializeObject<dynamic>(jsonSpecialties);
            foreach (var data in jsonData)
            {
                facilities.Add(new Facility
                {
                    Id = data.Id,
                    ShortName = data.ShortName,
                    Name = data.Name
                });
            }

            return facilities;
        }
    }
}
