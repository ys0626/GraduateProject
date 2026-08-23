using System.Collections.Generic;

public static class MCTSTranspositionTable
{
    private static readonly Dictionary<ulong, float> table = new Dictionary<ulong, float>();

    public static bool TryGet(ulong hash, out float value)
    {
        return table.TryGetValue(hash, out value);
    }

    public static void Store(ulong hash, float value)
    {
        table[hash] = value;
    }

    public static void Clear()
    {
        table.Clear();
    }
}