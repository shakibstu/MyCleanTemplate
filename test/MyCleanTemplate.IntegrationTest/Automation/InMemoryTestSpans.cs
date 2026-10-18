using System.Collections;
using System.Diagnostics;

namespace MyCleanTemplate.IntegrationTest.Automation;

/// <summary>
/// Span sink shared by all tests. Test classes run in parallel, so the exporter adds spans
/// while other tests read them; every access goes through <see cref="_lock"/>.
/// </summary>
public class InMemoryTestSpans : ICollection<Activity>
{
    private readonly Dictionary<string, List<Activity>> _spans = [];
    private readonly Lock _lock = new();

    public void Add(Activity item)
    {
        var traceId = item.TraceId.ToString();
        lock (_lock)
        {
            _spans.TryAdd(traceId, []);
            _spans[traceId].Add(item);
        }
    }

    public void Clear()
    {
        var traceId = Activity.Current?.TraceId.ToString();
        if (traceId is null) return;
        lock (_lock)
        {
            _spans.Remove(traceId);
        }
    }

    public bool Contains(Activity item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(Activity[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(Activity item)
    {
        throw new NotImplementedException();
    }

    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => false;

    public List<Activity> GetSpans(List<Func<Dictionary<string, object?>, bool>> checkups, string displayName, Guid runId)
    {
        List<Activity> snapshot;
        lock (_lock)
        {
            snapshot = _spans.Values.SelectMany(a => a).ToList();
        }

        return snapshot
            .Where(a => a.TagObjects.Any(b => b.Value?.ToString() == runId.ToString()))
            .Where(b => string.IsNullOrEmpty(displayName) || b.DisplayName == displayName)
            .Where(activity => checkups.TrueForAll(d => d.Invoke(activity.TagObjects.ToDictionary())))
            .ToList();
    }

    public IEnumerator<Activity> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}