using System.Collections.Generic;

public static class MCTSTranspositionTable
{
    private const int MaxEntries = 10000;
    private const int CachedUsesBeforeRefresh = 3;

    private struct CacheEntry
    {
        public float totalReward;
        public int sampleCount;
        private int cachedUsesSinceSample;

        public float AverageReward =>
            sampleCount > 0 ? totalReward / sampleCount : 0f;

        public CacheEntry(float reward)
        {
            totalReward = reward;
            sampleCount = 1;
            cachedUsesSinceSample = 0;
        }

        public void AddSample(float reward)
        {
            totalReward += reward;
            sampleCount++;
            cachedUsesSinceSample = 0;
        }

        public bool ShouldUseCachedResult()
        {
            cachedUsesSinceSample++;

            if (cachedUsesSinceSample <= CachedUsesBeforeRefresh)
                return true;

            // 주기적으로 새 롤아웃을 수행해 평균 보상을 실제 표본으로 갱신한다.
            cachedUsesSinceSample = 0;
            return false;
        }
    }

    private static readonly Dictionary<ulong, CacheEntry> table =
        new Dictionary<ulong, CacheEntry>();

    // 삽입 순서대로 제거하는 FIFO 제한. 전투 단위 캐시의 메모리 상한을 보장한다.
    private static readonly Queue<ulong> insertionOrder = new Queue<ulong>();

    public static int hitCount;
    public static int totalLookups;
    public static int EntryCount => table.Count;
    public static float HitRate =>
        totalLookups > 0 ? (float)hitCount / totalLookups : 0f;

    public static bool TryGet(ulong hash, out float value)
    {
        totalLookups++;

        if (table.TryGetValue(hash, out CacheEntry entry))
        {
            if (entry.ShouldUseCachedResult())
            {
                hitCount++;
                table[hash] = entry;
                value = entry.AverageReward;
                return true;
            }

            // 캐시가 존재하더라도 네 번째 조회는 새 표본을 수집한다.
            table[hash] = entry;
        }

        value = default;
        return false;
    }

    // 동일 상태가 다시 롤아웃되면 단일 결과를 덮어쓰지 않고 평균 보상을 갱신한다.
    public static void Store(ulong hash, float value)
    {
        if (table.TryGetValue(hash, out CacheEntry entry))
        {
            entry.AddSample(value);
            table[hash] = entry;
            return;
        }

        if (table.Count >= MaxEntries)
        {
            ulong oldestHash = insertionOrder.Dequeue();
            table.Remove(oldestHash);
        }

        table.Add(hash, new CacheEntry(value));
        insertionOrder.Enqueue(hash);
    }

    public static void Clear()
    {
        table.Clear();
        insertionOrder.Clear();
        hitCount = 0;
        totalLookups = 0;
    }
}
