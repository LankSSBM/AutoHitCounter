// 

using System;

namespace AutoHitCounter.Models;

public class RunSnapshot(int currentSplitIndex, int[] hitCounts, bool isRunComplete, TimeSpan inGameTime)
{
    public int CurrentSplitIndex { get; } = currentSplitIndex;
    public int[] HitCounts { get; } = hitCounts;
    public bool IsRunComplete { get; } = isRunComplete;
    public TimeSpan InGameTime { get; } = inGameTime;
}