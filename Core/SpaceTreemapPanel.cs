using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Wpc_SutilBox.Models;
using WpfPanel = System.Windows.Controls.Panel;
using WpfSize = System.Windows.Size;
using WpfRect = System.Windows.Rect;

namespace Wpc_SutilBox.Core;

public sealed class SpaceTreemapPanel : WpfPanel
{
    private const int MaxVisibleItems = 20;

    public SpaceTreemapPanel()
    {
        ClipToBounds = true;
    }

    protected override WpfSize MeasureOverride(WpfSize availableSize)
    {
        double w = double.IsInfinity(availableSize.Width) ? 400 : availableSize.Width;
        double h = double.IsInfinity(availableSize.Height) ? 350 : availableSize.Height;
        WpfSize childSize = new WpfSize(w, h);

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(childSize);
        }

        return new WpfSize(w, h);
    }

    protected override WpfSize ArrangeOverride(WpfSize finalSize)
    {
        if (InternalChildren.Count == 0 || finalSize.Width <= 0 || finalSize.Height <= 0)
            return finalSize;

        var items = new List<(UIElement Element, double Weight)>();
        foreach (UIElement child in InternalChildren)
        {
            var entry = (child as FrameworkElement)?.DataContext as SpaceMapEntry;
            double weight = Math.Max(1.0, entry?.SizeBytes ?? 0);
            items.Add((child, weight));
        }

        items.Sort((a, b) => b.Weight.CompareTo(a.Weight));

        var visibleItems = items.Take(MaxVisibleItems).ToList();
        var hiddenItems = items.Skip(MaxVisibleItems).ToList();

        foreach (var hidden in hiddenItems)
        {
            hidden.Element.Arrange(new WpfRect(0, 0, 0, 0));
        }

        WpfRect bounds = new WpfRect(0, 0, finalSize.Width, finalSize.Height);
        LayoutBinaryPartition(visibleItems, bounds);

        return finalSize;
    }

    private static void LayoutBinaryPartition(List<(UIElement Element, double Weight)> items, WpfRect rect)
    {
        if (items.Count == 0 || rect.Width <= 1 || rect.Height <= 1)
            return;

        if (items.Count == 1)
        {
            double m = 2;
            double w = Math.Max(1, rect.Width - m * 2);
            double h = Math.Max(1, rect.Height - m * 2);
            items[0].Element.Arrange(new WpfRect(rect.X + m, rect.Y + m, w, h));
            return;
        }

        double totalWeight = items.Sum(i => i.Weight);
        if (totalWeight <= 0) totalWeight = items.Count;

        double halfWeight = totalWeight / 2.0;
        double accumulated = 0;
        int splitIndex = 1;

        for (int i = 0; i < items.Count - 1; i++)
        {
            accumulated += items[i].Weight;
            if (accumulated >= halfWeight)
            {
                splitIndex = i + 1;
                break;
            }
        }

        var groupA = items.Take(splitIndex).ToList();
        var groupB = items.Skip(splitIndex).ToList();

        double weightA = groupA.Sum(i => i.Weight);
        double ratioA = weightA / totalWeight;

        WpfRect rectA, rectB;
        if (rect.Width >= rect.Height)
        {
            double widthA = Math.Max(1, rect.Width * ratioA);
            double widthB = Math.Max(1, rect.Width - widthA);
            rectA = new WpfRect(rect.X, rect.Y, widthA, rect.Height);
            rectB = new WpfRect(rect.X + widthA, rect.Y, widthB, rect.Height);
        }
        else
        {
            double heightA = Math.Max(1, rect.Height * ratioA);
            double heightB = Math.Max(1, rect.Height - heightA);
            rectA = new WpfRect(rect.X, rect.Y, rect.Width, heightA);
            rectB = new WpfRect(rect.X, rect.Y + heightA, rect.Width, heightB);
        }

        LayoutBinaryPartition(groupA, rectA);
        LayoutBinaryPartition(groupB, rectB);
    }
}
