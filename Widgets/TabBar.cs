using System;
using System.Linq;
using ImGuiNET;
using OtterGui.Raii;

namespace OtterGui.Widgets;

public interface ITab
{
    public ReadOnlySpan<byte> Label { get; }

    public bool IsVisible
        => true;

    public ImGuiTabItemFlags Flags
        => ImGuiTabItemFlags.None;

    public void DrawContent();

    public void DrawHeader()
    { }
}

public static class TabBar
{
    public static bool Draw(string label, ImGuiTabBarFlags flags = ImGuiTabBarFlags.None, params ITab[] tabs)
        => Draw(label, flags, ReadOnlySpan<byte>.Empty, tabs);

    public static bool Draw(string label, ReadOnlySpan<byte> selectTab, params ITab[] tabs)
        => Draw(label, ImGuiTabBarFlags.None, selectTab, tabs);

    public static unsafe bool Draw(string label, ImGuiTabBarFlags flags, ReadOnlySpan<byte> selectTab, params ITab[] tabs)
    {
        using var bar = ImRaii.TabBar(label, flags);
        if (!bar)
            return false;

        var ret = false;
        foreach (var tabData in tabs.Where(t => t.IsVisible))
        {
            try
            {
                var tabFlags = tabData.Flags;
                if (selectTab.Length > 0 && selectTab.SequenceEqual(tabData.Label))
                {
                    tabFlags |= ImGuiTabItemFlags.SetSelected;
                    ret      =  true;
                }

                fixed (byte* labelPtr = tabData.Label)
                {
                    using var tab = ImRaii.TabItem(labelPtr, tabFlags);
                    tabData.DrawHeader();
                    if (tab)
                        tabData.DrawContent();
                }
            }
            catch
            {
                // ignored
            }
        }

        return ret;
    }
}