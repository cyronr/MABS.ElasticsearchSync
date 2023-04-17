namespace DoctorsSync.Models
{
    public class ElasticsearchDoctor
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string TitleShortName { get; set; } = null!;
        public string TitleName { get; set; } = null!;
        public List<string> Specalities { get; set; } = new List<string>();


        public static ElasticsearchDoctor MapFromDbDoctor(DbDoctor dbDoctor)
        {
            var esDoctor = new ElasticsearchDoctor
            {
                Id = dbDoctor.Id,
                UUID = dbDoctor.UUID,
                FirstName = dbDoctor.FirstName,
                LastName = dbDoctor.LastName,
                TitleShortName = dbDoctor.TitleShortName,
                TitleName = dbDoctor.TitleName
            };
            dbDoctor.Specalities.Split(';').ToList()
                .ForEach(s => esDoctor.Specalities.Add(s));

            return esDoctor;
        }
    }
}
