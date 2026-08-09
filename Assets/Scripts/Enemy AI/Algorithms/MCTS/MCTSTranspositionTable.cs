using UnityEngine;
using System.Collections.Generic;

//동일 상태 재탐색 방지를 위한 결과 캐시
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
