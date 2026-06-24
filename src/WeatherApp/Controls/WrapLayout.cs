using Microsoft.Maui.Layouts;

namespace WeatherApp.Controls;

public sealed class WrapLayout : Layout
{
    public static readonly BindableProperty SpacingProperty = BindableProperty.Create(
        nameof(Spacing), typeof(double), typeof(WrapLayout), 8d,
        propertyChanged: (bindable, _, _) => ((WrapLayout)bindable).InvalidateMeasure());

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    protected override ILayoutManager CreateLayoutManager() => new WrapLayoutManager(this);

    private sealed class WrapLayoutManager(WrapLayout layout) : ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var spacing = layout.Spacing;
            double rowWidth = 0, rowHeight = 0, totalHeight = 0, widest = 0;

            foreach (var child in layout)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                var size = child.Measure(double.PositiveInfinity, double.PositiveInfinity);

                if (rowWidth > 0 && rowWidth + spacing + size.Width > widthConstraint)
                {
                    widest = Math.Max(widest, rowWidth);
                    totalHeight += rowHeight + spacing;
                    rowWidth = 0;
                    rowHeight = 0;
                }

                rowWidth += (rowWidth > 0 ? spacing : 0) + size.Width;
                rowHeight = Math.Max(rowHeight, size.Height);
            }

            widest = Math.Max(widest, rowWidth);
            totalHeight += rowHeight;

            var width = double.IsInfinity(widthConstraint) ? widest : widthConstraint;
            return new Size(width, totalHeight);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var spacing = layout.Spacing;
            double x = bounds.Left, y = bounds.Top, rowHeight = 0;

            foreach (var child in layout)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                var size = child.DesiredSize;

                if (x > bounds.Left && x + size.Width > bounds.Right + 0.5)
                {
                    x = bounds.Left;
                    y += rowHeight + spacing;
                    rowHeight = 0;
                }

                child.Arrange(new Rect(x, y, size.Width, size.Height));
                x += size.Width + spacing;
                rowHeight = Math.Max(rowHeight, size.Height);
            }

            return new Size(bounds.Width, (y - bounds.Top) + rowHeight);
        }
    }
}
