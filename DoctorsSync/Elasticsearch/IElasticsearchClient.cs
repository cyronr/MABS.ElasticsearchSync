using DoctorsSync.Models;

namespace DoctorsSync.Elasticsearch
{
    public interface IElasticsearchClient
    {
        void UpsertDocuments(List<ElasticsearchDoctor> documents);
        void DeleteDocuments(long[] ids);
    }
}
