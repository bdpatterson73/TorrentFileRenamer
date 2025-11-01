using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using TorrentFileRenamer.WPF.Services;

namespace TorrentFileRenamer.WPF.Behaviors;

/// <summary>
/// Behavior to automatically persist and restore DataGrid column widths
/// </summary>
public class DataGridColumnWidthPersistence : Behavior<DataGrid>
{
    public static readonly DependencyProperty GridKeyProperty =
  DependencyProperty.Register(
    nameof(GridKey),
        typeof(string),
   typeof(DataGridColumnWidthPersistence),
      new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty WindowStateServiceProperty =
    DependencyProperty.Register(
    nameof(WindowStateService),
         typeof(IWindowStateService),
   typeof(DataGridColumnWidthPersistence),
  new PropertyMetadata(null));

    public string GridKey
    {
        get => (string)GetValue(GridKeyProperty);
  set => SetValue(GridKeyProperty, value);
   }

    public IWindowStateService? WindowStateService
{
 get => (IWindowStateService?)GetValue(WindowStateServiceProperty);
   set => SetValue(WindowStateServiceProperty, value);
    }

protected override void OnAttached()
    {
  base.OnAttached();
        
        if (AssociatedObject != null)
   {
   AssociatedObject.Loaded += OnDataGridLoaded;
      AssociatedObject.Unloaded += OnDataGridUnloaded;
        }
    }

    protected override void OnDetaching()
 {
        if (AssociatedObject != null)
   {
            AssociatedObject.Loaded -= OnDataGridLoaded;
   AssociatedObject.Unloaded -= OnDataGridUnloaded;
   SaveColumnWidths();
        }
        
base.OnDetaching();
    }

    private void OnDataGridLoaded(object sender, RoutedEventArgs e)
    {
  RestoreColumnWidths();
    }

 private void OnDataGridUnloaded(object sender, RoutedEventArgs e)
    {
   SaveColumnWidths();
    }

    private void RestoreColumnWidths()
    {
    if (WindowStateService == null || string.IsNullOrWhiteSpace(GridKey) || AssociatedObject == null)
      return;

        var widths = WindowStateService.RestoreColumnWidths(GridKey);
        if (widths == null)
    return;

   var widthsList = widths.ToList();
   for (int i = 0; i < Math.Min(widthsList.Count, AssociatedObject.Columns.Count); i++)
   {
   var column = AssociatedObject.Columns[i];
         var width = widthsList[i];
          
  // Only restore if it's a star or pixel width (not auto)
  if (width > 0)
     {
     column.Width = new DataGridLength(width, DataGridLengthUnitType.Pixel);
            }
        }
    }

    private void SaveColumnWidths()
{
 if (WindowStateService == null || string.IsNullOrWhiteSpace(GridKey) || AssociatedObject == null)
    return;

var widths = AssociatedObject.Columns
   .Select(c => c.ActualWidth)
   .ToList();

WindowStateService.SaveColumnWidths(GridKey, widths);
    }
}
