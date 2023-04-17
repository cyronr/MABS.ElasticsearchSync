using DoctorsSync.Models.Elasticsearch;

namespace DoctorsSync.Elasticsearch
{
    public interface IElasticsearchClient
    {
        void UpsertDocuments(List<Doctor> documents);
        void DeleteDocuments(long[] ids);
    }
}
