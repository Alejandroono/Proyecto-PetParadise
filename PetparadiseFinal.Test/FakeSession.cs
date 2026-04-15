using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class FakeSession : ISession
{
    private Dictionary<string, byte[]> storage = new();

    public IEnumerable<string> Keys => storage.Keys;

    public string Id => "TestSession";

    public bool IsAvailable => true;

    public void Clear()
    {
        storage.Clear();
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        storage.Remove(key);
    }

    public void Set(string key, byte[] value)
    {
        storage[key] = value;
    }

    public bool TryGetValue(string key, out byte[] value)
    {
        return storage.TryGetValue(key, out value);
    }
}