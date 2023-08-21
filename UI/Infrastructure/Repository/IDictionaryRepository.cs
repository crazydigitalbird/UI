using System.Collections.Concurrent;

namespace UI.Infrastructure.Repository
{
    public interface IDictionaryRepository<K, T>
    {
        Dictionary<K, T> Active { get; set; }

        ConcurrentDictionary<int, int> CounterManOnline { get; set; }

        ConcurrentDictionary<int, bool> SheetsIsOnline { get; set; }
    }
}