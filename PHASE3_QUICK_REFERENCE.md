# Phase 3 Quick Reference Guide

## Card Components

### FileEpisodeCompactCard
**Usage:**
```xaml
<controls:FileEpisodeCompactCard/>
```

**Binding Context:** `FileEpisodeModel`

**Required DataContext Commands:**
- `ViewDetailsCommand`
- `OpenFolderCommand`
- `RetryCommand`
- `RemoveSelectedCommand`

**Key Bindings:**
```xaml
{Binding ShowName}
{Binding SeasonNumber}
{Binding EpisodeNumbers}
{Binding IsMultiEpisode}
{Binding CoreEpisode.FileName}
{Binding NewFileName}
{Binding Status}
{Binding StatusText}
{Binding ErrorMessage}
{Binding Extension}
{Binding CoreEpisode.FileSize}
```

---

### MovieFileCard
**Usage:**
```xaml
<controls:MovieFileCard Margin="0,0,0,12"/>
```

**Binding Context:** `MovieFileModel`

**Required DataContext Commands:**
- `ViewDetailsCommand`
- `OpenFolderCommand`
- `RetryCommand`
- `RemoveSelectedCommand`

**Key Bindings:**
```xaml
{Binding MovieName}
{Binding MovieYear}
{Binding Confidence}
{Binding ConfidenceLevel}
{Binding FileName}
{Binding NewFileName}
{Binding SourcePath}
{Binding DestinationPath}
{Binding Status}
{Binding StatusText}
{Binding ErrorMessage}
{Binding FileSize}
{Binding Extension}
```

---

### MovieFileCompactCard
**Usage:**
```xaml
<controls:MovieFileCompactCard/>
```

**Binding Context:** `MovieFileModel`

**Same commands and bindings as MovieFileCard**

---

## View Patterns

### View Mode Selector
```xaml
<StackPanel Orientation="Horizontal">
    <TextBlock Text="View:" VerticalAlignment="Center" FontWeight="Medium" Margin="0,0,12,0"/>
    
    <RadioButton Content="Cards"
        Style="{StaticResource ViewToggleButton}"
IsChecked="{Binding IsCardViewSelected}"
          Margin="0,0,8,0"/>
    
    <RadioButton Content="Compact"
       Style="{StaticResource ViewToggleButton}"
       IsChecked="{Binding IsCompactViewSelected}"
Margin="0,0,8,0"/>
  
    <RadioButton Content="Grid"
 Style="{StaticResource ViewToggleButton}"
        IsChecked="{Binding IsGridViewSelected}"/>
</StackPanel>
```

### Search Box
```xaml
<StackPanel Orientation="Horizontal">
    <TextBlock Text="&#x1F50D;" FontSize="16" VerticalAlignment="Center" Margin="0,0,8,0"/>
    <TextBox Width="250"
         Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
        ToolTip="Search by [field description]">
        <!-- Style with hover/focus effects -->
    </TextBox>
</StackPanel>
```

### Card View Layout
```xaml
<ScrollViewer VerticalScrollBarVisibility="Auto"
         Visibility="{Binding IsCardViewSelected, Converter={StaticResource BoolToVisibilityConverter}}">
    <ItemsControl ItemsSource="{Binding [Collection]}" Margin="16">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <StackPanel/>
            </ItemsPanelTemplate>
 </ItemsControl.ItemsPanel>
     <ItemsControl.ItemTemplate>
   <DataTemplate>
    <controls:[CardControl] Margin="0,0,0,12"/>
 </DataTemplate>
      </ItemsControl.ItemTemplate>
    </ItemsControl>
</ScrollViewer>
```

### Compact View Layout
```xaml
<ScrollViewer VerticalScrollBarVisibility="Auto"
   Visibility="{Binding IsCompactViewSelected, Converter={StaticResource BoolToVisibilityConverter}}">
    <ItemsControl ItemsSource="{Binding [Collection]}" Margin="16">
     <ItemsControl.ItemsPanel>
   <ItemsPanelTemplate>
       <StackPanel/>
       </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ItemsControl.ItemTemplate>
            <DataTemplate>
      <controls:[CompactCardControl]/>
     </DataTemplate>
     </ItemsControl.ItemTemplate>
    </ItemsControl>
</ScrollViewer>
```

---

## ViewModel Requirements

### Properties for View Mode
```csharp
private bool _isCardViewSelected = true;
private bool _isCompactViewSelected;
private bool _isGridViewSelected;

public bool IsCardViewSelected
{
    get => _isCardViewSelected;
    set
    {
        if (SetProperty(ref _isCardViewSelected, value) && value)
        {
            IsCompactViewSelected = false;
  IsGridViewSelected = false;
        }
    }
}

// Similar for IsCompactViewSelected and IsGridViewSelected
```

### Properties for Search
```csharp
private string _searchText = string.Empty;

public string SearchText
{
    get => _searchText;
    set
{
        if (SetProperty(ref _searchText, value))
        {
            ApplyFilters();
        }
    }
}

private void ApplyFilters()
{
    var filtered = _allItems.AsEnumerable();
    
    if (!string.IsNullOrWhiteSpace(SearchText))
    {
        filtered = filtered.Where(item =>
         (item.Property1?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (item.Property2?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
    }
    
    Items.Clear();
    foreach (var item in filtered)
    {
        Items.Add(item);
    }
}
```

### Required Commands
```csharp
public ICommand ViewDetailsCommand { get; }
public ICommand OpenFolderCommand { get; }
public ICommand RetryCommand { get; }
public ICommand RemoveSelectedCommand { get; }

// In constructor:
ViewDetailsCommand = new RelayCommand(ViewDetails);
OpenFolderCommand = new RelayCommand(OpenFolder);
RetryCommand = new AsyncRelayCommand<[ModelType]>(RetryAsync);
RemoveSelectedCommand = new RelayCommand(RemoveSelected);
```

### Command Implementations
```csharp
private void ViewDetails(object? parameter)
{
    if (parameter is [ModelType] item)
    {
  var message = $"[Build detail string]";
        _dialogService.ShowMessageAsync("Details", message);
    }
}

private void OpenFolder(object? parameter)
{
    if (parameter is [ModelType] item && !string.IsNullOrEmpty(item.SourcePath))
    {
        try
      {
  var directory = Path.GetDirectoryName(item.SourcePath);
   if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
         {
        Process.Start("explorer.exe", directory);
            }
        }
      catch (Exception ex)
        {
      _dialogService.ShowErrorAsync("Open Folder", $"Failed to open folder: {ex.Message}");
        }
    }
}

private async Task RetryAsync([ModelType]? item)
{
    if (item == null || item.Status != ProcessingStatus.Failed)
        return;
        
    // Reset status and retry processing
}
```

---

## Model Requirements

### MovieFileModel Additions
```csharp
using System.IO;

// New properties:
public string NewFileName
{
    get
    {
        if (string.IsNullOrEmpty(_coreMovie.NewDestDirectory))
   return string.Empty;
        
        return Path.GetFileName(_coreMovie.NewDestDirectory) ?? string.Empty;
    }
}

public string Extension => Path.GetExtension(FileName);

public long FileSize
{
    get
    {
        if (string.IsNullOrEmpty(SourcePath) || !File.Exists(SourcePath))
            return 0;
        
     try
        {
            return new FileInfo(SourcePath).Length;
        }
        catch
        {
  return 0;
     }
    }
}
```

---

## Color Reference

### Status Colors
```csharp
Pending:    #E8F4F8 (Light Blue)
Processing: #FEF9E7 (Light Yellow)
Completed:  #E8F5E9 (Light Green)
Failed:     #FDECEA (Light Red)
Unparsed:   #F5F5F5 (Light Gray)
```

### Confidence Colors (Movies)
```csharp
High (?70%):      #4CAF50 (Green)
Medium (40-69%):  #FF9800 (Orange)
Low (<40%):  #F44336 (Red)
```

### Section Backgrounds
```csharp
Original Filename:  #F5F5F5 (Gray)
New Filename: #E8F5E9 (Light Green)
Error Section:      #FFEBEE (Light Red)
Metadata Footer:    #FAFAFA (Off-White)
Action Footer:      #F5F5F5 (Gray)
```

---

## Common Patterns

### Conditional Visibility
```xaml
Visibility="{Binding [Property], Converter={StaticResource BoolToVisibilityConverter}}"
Visibility="{Binding [StringProperty], Converter={StaticResource StringToVisibilityConverter}}"
Visibility="{Binding Status, Converter={StaticResource StatusToRetryVisibilityConverter}}"
```

### Status Indicators
```xaml
<!-- Icon -->
<TextBlock Text="{Binding Status, Converter={StaticResource StatusToIconConverter}}"
         FontSize="24"
    Foreground="{Binding Status, Converter={StaticResource StatusToBrushConverter}}"/>

<!-- Border/Badge -->
<Border Background="{Binding Status, Converter={StaticResource StatusToBrushConverter}}">
    <TextBlock Text="{Binding StatusText}"/>
</Border>
```

### File Size Display
```xaml
<TextBlock Text="{Binding FileSize, Converter={StaticResource FileSizeConverter}}"/>
```

### Confidence Display (Movies)
```xaml
<!-- Badge -->
<TextBlock>
    <Run Text="{Binding Confidence, Mode=OneWay}"/>
    <Run Text="% "/>
    <Run Text="{Binding ConfidenceLevel}"/>
</TextBlock>

<!-- Progress Bar -->
<ProgressBar Value="{Binding Confidence}" Minimum="0" Maximum="100"/>
```

---

## Testing Checklist

### Visual Testing
- [ ] Card view displays correctly
- [ ] Compact view displays correctly
- [ ] Grid view displays correctly
- [ ] View switching works smoothly
- [ ] Search filters results in real-time
- [ ] Colors match status/confidence
- [ ] Icons display correctly
- [ ] Animations play on load

### Functional Testing
- [ ] ViewDetails command shows correct info
- [ ] OpenFolder opens correct directory
- [ ] Retry processes failed items
- [ ] Remove deletes items from list
- [ ] Search works across all fields
- [ ] View preference toggles correctly

### Responsive Testing
- [ ] Cards resize properly
- [ ] Text truncates with ellipsis
- [ ] Tooltips show full text
- [ ] ScrollViewer appears when needed
- [ ] Layout adapts to window size

---

## Troubleshooting

### Issue: InitializeComponent not found
**Solution:** Ensure UserControl is fully qualified:
```csharp
public partial class [CardName] : System.Windows.Controls.UserControl
```

### Issue: Resource not found
**Solution:** Check App.xaml includes:
```xaml
<ResourceDictionary Source="Resources/CardStyles.xaml"/>
<ResourceDictionary Source="Resources/Animations.xaml"/>
```

### Issue: Binding errors
**Solution:** Verify DataContext and property names:
- Check ViewModel has required properties
- Check Model exposes necessary data
- Use `d:DataContext` for design-time binding

### Issue: Cards not displaying
**Solution:** Check visibility bindings and view mode properties

---

## Performance Tips

1. **Virtualization**: For >1000 items, use `VirtualizingStackPanel`
2. **Caching**: FileSize is calculated on-demand; consider caching
3. **Filtering**: ApplyFilters creates new collection; optimize for large datasets
4. **Animations**: Keep animations under 500ms for smooth UX

---

## Quick Copy-Paste Templates

### Add New Card View
```xaml
<ScrollViewer VerticalScrollBarVisibility="Auto"
            Visibility="{Binding Is[ViewName]Selected, Converter={StaticResource BoolToVisibilityConverter}}">
 <ItemsControl ItemsSource="{Binding [Collection]}" Margin="16">
     <ItemsControl.ItemsPanel>
      <ItemsPanelTemplate><StackPanel/></ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ItemsControl.ItemTemplate>
            <DataTemplate>
        <controls:[CardControl] Margin="0,0,0,12"/>
   </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</ScrollViewer>
```

### Add View Mode Property
```csharp
private bool _is[ViewName]Selected;
public bool Is[ViewName]Selected
{
    get => _is[ViewName]Selected;
    set
    {
   if (SetProperty(ref _is[ViewName]Selected, value) && value)
        {
   // Deselect other views
   IsOtherView1Selected = false;
            IsOtherView2Selected = false;
  }
    }
}
```

---

**Last Updated:** Phase 3 Completion - January 2025

