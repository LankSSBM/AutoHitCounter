// 

namespace AutoHitCounter.Models;

public class OverlayConfig(int maxSplits, int prevSplits, int nextSplits, bool showDiff, bool showPb, bool showIgt, int overlayWidth)
{
    public int MaxSplits { get; } = maxSplits;
    public int PrevSplits { get; } = prevSplits;
    public int NextSplits { get; } = nextSplits;
    public bool ShowDiff { get; } = showDiff;
    public bool ShowPb { get; } = showPb;
    public bool ShowIgt { get; } = showIgt;
    public int OverlayWidth { get; } = overlayWidth;
}