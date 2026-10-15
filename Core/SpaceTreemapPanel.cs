using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Wpc_SutilBox.Models;

namespace Wpc_SutilBox.Core;

public sealed class SpaceTreemapPanel : System.Windows.Controls.Panel
{
    protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
    {
        foreach (UIElement child in InternalChildren) child.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
        return new System.Windows.Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
    }

    protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
    {
        if (InternalChildren.Count == 0) return finalSize;
        var weights = InternalChildren.Cast<UIElement>().Select(child => (child as FrameworkElement)?.DataContext as SpaceMapEntry).Select(entry => Math.Max(1d, entry?.SizeBytes ?? 0)).ToArray();
        var total = weights.Sum();
        var x = 0d;
        var y = 0d;
        var rowHeight = 0d;
        for (var index = 0; index < InternalChildren.Count; index++)
        {
            var width = index == InternalChildren.Count - 1 ? finalSize.Width - x : finalSize.Width * weights[index] / total;
            width = Math.Max(0, width);
            if (x > 0 && x + width > finalSize.Width)
            {
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }
            var height = Math.Max(48, InternalChildren[index].DesiredSize.Height);
            InternalChildren[index].Arrange(new Rect(x, y, Math.Max(width, 1), height));
            x += width;
            rowHeight = Math.Max(rowHeight, height);
        }
        return finalSize;
    }
}
