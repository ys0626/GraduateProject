using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;

public static class MCTSTranspositionTable
{
    private static readonly Dictionary<ulong, float> table = new Dictionary<ulong, float>();

    public static int hitCount;
    public static int totalLookups;
    public static bool TryGet(ulong hash, out float value)
    {
        totalLookups++;
        bool found = table.TryGetValue(hash, out value);
        if (found) hitCount++;
        return found;
    }

    public static void Store(ulong hash, float value)
    {
        table[hash] = value;
    }

    public static void Clear()
    {
        table.Clear();
        hitCount = 0;
        totalLookups = 0;
    }
}