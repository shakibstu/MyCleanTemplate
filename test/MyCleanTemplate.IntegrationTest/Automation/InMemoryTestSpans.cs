using System.Collections;
using System.Diagnostics;

namespace MyCleanTemplate.IntegrationTest.Automation;

public class InMemoryTestSpans : ICollection<Activity>
{
    private Dictionary<string, List<Activity>> _spans = [];

    public void Add(Activity item)
    {
        var traceId = item.TraceId.ToString();
        _spans.TryAdd(traceId, []);
        _spans[traceId].Add(item);
    }

    public void Clear()
    {
        _spans.Remove(Activity.Current?.TraceId.ToString());
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
        return _spans.SelectMany(a => a.Value)
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