using System;
using QuestPDF.Drawing.Exceptions;
using QuestPDF.Elements;
using QuestPDF.Infrastructure;

namespace QuestPDF.Fluent;

public class MultiColumnDescriptor
{
    internal MultiColumn MultiColumn { get; } = new MultiColumn();
        
    public void Spacing(float value, Unit unit = Unit.Point)
    {
        MultiColumn.Spacing = value.ToPoints(unit);
    }
    
    public void Columns(int value = 2)
    {
        MultiColumn.ColumnCount = value;
    }
        
    public void BalanceHeight(bool enable = true)
    {
        MultiColumn.BalanceHeight = enable;
    }

    public IContainer Content()
    {
        if (MultiColumn.Content is not Empty)
            throw new DocumentComposeException("The 'MultiColumn.Content' layer has already been defined. Please call this method only once.");
        
        var container = new Container();
        MultiColumn.Content = container;
        return container;
    }
    
    public IContainer Decoration()
    {
        if (MultiColumn.Decoration is not Empty)
            throw new DocumentComposeException("The 'MultiColumn.Decoration' layer has already been defined. Please call this method only once.");
        
        var container = new RepeatContent();
        MultiColumn.Decoration = container;
        return container;
    }
}

public static class MultiColumnExtensions
{
    /// <summary>
    /// Creates a multi-column layout within the current container element.
    /// </summary>
    public static void MultiColumn(this IContainer element, Action<MultiColumnDescriptor> handler)
    {
        var descriptor = new MultiColumnDescriptor();
        
        element.Element(descriptor.MultiColumn);
        handler(descriptor);
    }
}