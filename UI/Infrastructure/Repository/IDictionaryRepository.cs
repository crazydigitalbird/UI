using System.Collections.Concurrent;

namespace UI.Infrastructure.Repository
{
    public interface IDictionaryRepository<K, T>
    {
        ConcurrentDictionary<K, T> Active { get; set; }

        ConcurrentDictionary<K, T> Online { get; set; }
    }
}