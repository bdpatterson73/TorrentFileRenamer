using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;

namespace TorrentFileRenamer.WPF.Behaviors;

/// <summary>
/// Behavior that automatically scrolls an ItemsControl to bring a specific item into view
/// </summary>
public class ScrollToItemBehavior : Behavior<ItemsControl>
{
    /// <summary>
    /// The item to scroll to
    /// </summary>
    public static readonly DependencyProperty ScrollToItemProperty =
        DependencyProperty.Register(
      nameof(TargetItem),
      typeof(object),
            typeof(ScrollToItemBehavior),
  new PropertyMetadata(null, OnScrollToItemChanged));

    public object? TargetItem
    {
        get => GetValue(ScrollToItemProperty);
        set => SetValue(ScrollToItemProperty, value);
    }

    private static void OnScrollToItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
if (d is ScrollToItemBehavior behavior && e.NewValue != null)
     {
            behavior.PerformScroll(e.NewValue);
      }
    }

    private void PerformScroll(object item)
    {
   if (AssociatedObject == null || item == null)
      return;

        // Try to scroll the item into view
  AssociatedObject.Dispatcher.BeginInvoke(new Action(() =>
        {
// Find the ScrollViewer
    var scrollViewer = FindScrollViewer(AssociatedObject);
            if (scrollViewer == null)
      return;

      // Get the container for the item
    var container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            if (container == null)
            {
        // Container might not be generated yet, wait for it
 void OnStatusChanged(object? sender, EventArgs args)
                {
       if (AssociatedObject.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
        {
        AssociatedObject.ItemContainerGenerator.StatusChanged -= OnStatusChanged;
           container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
      if (container != null)
         {
    BringIntoView(container, scrollViewer);
       }
          }
    }

    AssociatedObject.ItemContainerGenerator.StatusChanged += OnStatusChanged;
    return;
            }

 BringIntoView(container, scrollViewer);
     }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void BringIntoView(FrameworkElement container, ScrollViewer scrollViewer)
 {
        try
        {
  // Get the container's position relative to the ScrollViewer
        var transform = container.TransformToAncestor(scrollViewer);
            var position = transform.Transform(new System.Windows.Point(0, 0));

     // Calculate the desired scroll position to center the item
  var containerHeight = container.ActualHeight;
       var viewportHeight = scrollViewer.ViewportHeight;

            // Center the item in the viewport
   var targetOffset = position.Y + scrollViewer.VerticalOffset - (viewportHeight / 2) + (containerHeight / 2);

      // Ensure we don't scroll past the end
            targetOffset = Math.Max(0, Math.Min(targetOffset, scrollViewer.ScrollableHeight));

          // Smooth scroll to the target position
       scrollViewer.ScrollToVerticalOffset(targetOffset);
        }
        catch
      {
     // If scrolling fails, silently ignore
        }
    }

    private ScrollViewer? FindScrollViewer(DependencyObject element)
    {
        if (element is ScrollViewer scrollViewer)
            return scrollViewer;

        // First, search descendants (children)
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
     var child = VisualTreeHelper.GetChild(element, i);
            var result = FindScrollViewer(child);
    if (result != null)
     return result;
        }

        // If not found in descendants, search ancestors (parents)
    var visited = new HashSet<DependencyObject>();
        var current = element;
        
   while (current != null)
        {
     if (!visited.Add(current))
   break; // Already visited, prevent infinite loop
      
        if (current is ScrollViewer sv)
            return sv;
             
    // Try visual parent first
      current = VisualTreeHelper.GetParent(current);
            
        // If no visual parent, try logical parent
            if (current == null && element is FrameworkElement fe)
    {
 current = fe.Parent as DependencyObject;
       }
    }

        return null;
    }
}
