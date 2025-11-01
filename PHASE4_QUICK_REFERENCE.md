# Phase 4 Quick Reference Guide

## Status Filter Usage

### XAML Implementation
```xaml
<ComboBox Width="150"
  SelectedValue="{Binding StatusFilter, Mode=TwoWay}">
  <ComboBoxItem Content="All" Tag="{x:Null}" IsSelected="True"/>
  <ComboBoxItem Content="Pending" Tag="{x:Static models:ProcessingStatus.Pending}"/>
  <ComboBoxItem Content="Processing" Tag="{x:Static models:ProcessingStatus.Processing}"/>
  <ComboBoxItem Content="Completed" Tag="{x:Static models:ProcessingStatus.Completed}"/>
  <ComboBoxItem Content="Failed" Tag="{x:Static models:ProcessingStatus.Failed}"/>
  <ComboBoxItem Content="Unparsed" Tag="{x:Static models:ProcessingStatus.Unparsed}"/>
</ComboBox>
```

### ViewModel Property
```csharp
private ProcessingStatus? _statusFilter;

public ProcessingStatus? StatusFilter
{
    get => _statusFilter;
 set
    {
        if (SetProperty(ref _statusFilter, value))
        {
    ApplyFilters();
   }
    }
}
```

### Filter Logic
```csharp
private void ApplyFilters()
{
    var filtered = _allEpisodes.AsEnumerable();

    // Apply search filter
    if (!string.IsNullOrWhiteSpace(SearchText))
    {
  filtered = filtered.Where(e =>
            (e.ShowName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
         (e.NewFileName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
  (e.SourcePath?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
    }

    // Apply status filter
    if (StatusFilter.HasValue)
    {
 filtered = filtered.Where(e => e.Status == StatusFilter.Value);
    }

    // Update collection
    Episodes.Clear();
    foreach (var episode in filtered)
    {
     Episodes.Add(episode);
    }
    
    // Refresh command states
    ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
    ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
}
```

## Enhanced Context Menu

### XAML Structure
```xaml
<DataGrid.ContextMenu>
  <ContextMenu>
    <!-- View Details -->
    <MenuItem Header="View Details" 
      Command="{Binding ViewDetailsCommand}"
      CommandParameter="{Binding PlacementTarget.SelectedItem, RelativeSource={RelativeSource AncestorType=ContextMenu}}">
  <MenuItem.Icon>
        <TextBlock Text="&#x1F4CB;" FontSize="14"/>
      </MenuItem.Icon>
    </MenuItem>
    
    <Separator/>
    
    <!-- Folder Operations -->
    <MenuItem Header="Open Source Folder" 
 Command="{Binding OpenSourceFolderCommand}"
      CommandParameter="{Binding PlacementTarget.SelectedItem, RelativeSource={RelativeSource AncestorType=ContextMenu}}">
    <MenuItem.Icon>
        <TextBlock Text="&#x1F4C1;" FontSize="14"/>
      </MenuItem.Icon>
    </MenuItem>
    
    <MenuItem Header="Open Destination Folder" 
      Command="{Binding OpenDestinationFolderCommand}"
   CommandParameter="{Binding PlacementTarget.SelectedItem, RelativeSource={RelativeSource AncestorType=ContextMenu}}">
      <MenuItem.Icon>
        <TextBlock Text="&#x1F4C2;" FontSize="14"/>
      </MenuItem.Icon>
    </MenuItem>
    
    <Separator/>
    
    <!-- Clipboard Operations -->
    <MenuItem Header="Copy Source Path" 
      Command="{Binding CopySourcePathCommand}"
      CommandParameter="{Binding PlacementTarget.SelectedItem, RelativeSource={RelativeSource AncestorType=ContextMenu}}">
      <MenuItem.Icon>
      <TextBlock Text="&#x1F4CB;" FontSize="14"/>
      </MenuItem.Icon>
    </MenuItem>
    
    <MenuItem Header="Copy Destination Path" 
      Command="{Binding CopyDestinationPathCommand}"
      CommandParameter="{Binding PlacementTarget.SelectedItem, RelativeSource={RelativeSource AncestorType=ContextMenu}}">
      <MenuItem.Icon>
        <TextBlock Text="&#x1F4CB;" FontSize="14"/>
      </MenuItem.Icon>
    </MenuItem>
    
  <Separator/>
    
  <!-- Retry -->
  <MenuItem Header="Retry Failed" 
      Command="{Binding RetryCommand}"
   CommandParameter="{Binding PlacementTarget.SelectedItem, RelativeSource={RelativeSource AncestorType=ContextMenu}}">
      <MenuItem.Icon>
        <TextBlock Text="&#x1F504;" FontSize="14"/>
      </MenuItem.Icon>
    </MenuItem>
    
    <Separator/>
    
    <!-- Removal Operations -->
    <MenuItem Header="Remove" 
      Command="{Binding RemoveSelectedCommand}"
      CommandParameter="{Binding PlacementTarget.SelectedItem, RelativeSource={RelativeSource AncestorType=ContextMenu}}">
      <MenuItem.Icon>
        <TextBlock Text="&#x1F5D1;" FontSize="14"/>
    </MenuItem.Icon>
    </MenuItem>
    
    <MenuItem Header="Remove All Failed" 
  Command="{Binding RemoveAllFailedCommand}">
      <MenuItem.Icon>
        <TextBlock Text="&#x26A0;&#xFE0F;" FontSize="14"/>
      </MenuItem.Icon>
    </MenuItem>
    
    <MenuItem Header="Remove All Completed" 
      Command="{Binding RemoveAllCompletedCommand}">
      <MenuItem.Icon>
        <TextBlock Text="&#x2705;" FontSize="14"/>
      </MenuItem.Icon>
</MenuItem>
    
    <MenuItem Header="Clear All" 
      Command="{Binding ClearAllCommand}">
   <MenuItem.Icon>
        <TextBlock Text="&#x1F5D1;" FontSize="14"/>
      </MenuItem.Icon>
    </MenuItem>
    
    <Separator/>
    
    <!-- Selection -->
    <MenuItem Header="Select All"
      Command="{Binding SelectAllCommand}">
      <MenuItem.Icon>
        <TextBlock Text="&#x2611;&#xFE0F;" FontSize="14"/>
      </MenuItem.Icon>
    </MenuItem>
  </ContextMenu>
</DataGrid.ContextMenu>
```

## Command Implementations

### Open Folder Commands
```csharp
private void OpenSourceFolder(object? parameter)
{
    if (parameter is FileEpisodeModel episode && !string.IsNullOrEmpty(episode.SourcePath))
    {
        try
{
        var folderPath = System.IO.Path.GetDirectoryName(episode.SourcePath);
     if (!string.IsNullOrEmpty(folderPath) && System.IO.Directory.Exists(folderPath))
            {
            System.Diagnostics.Process.Start("explorer.exe", folderPath);
   }
        }
        catch (Exception ex)
        {
       _dialogService.ShowErrorAsync("Open Folder Error", $"Failed to open source folder: {ex.Message}");
        }
    }
}

private void OpenDestinationFolder(object? parameter)
{
    if (parameter is FileEpisodeModel episode && !string.IsNullOrEmpty(episode.DestinationPath))
    {
        try
      {
            var folderPath = System.IO.Path.GetDirectoryName(episode.DestinationPath);
 if (!string.IsNullOrEmpty(folderPath))
          {
 if (!System.IO.Directory.Exists(folderPath))
                {
            _dialogService.ShowMessageAsync("Folder Not Found", 
  "Destination folder does not exist yet. It will be created during processing.");
            return;
    }
        System.Diagnostics.Process.Start("explorer.exe", folderPath);
      }
  }
        catch (Exception ex)
        {
   _dialogService.ShowErrorAsync("Open Folder Error", $"Failed to open destination folder: {ex.Message}");
     }
  }
}
```

### Copy Path Commands
```csharp
private void CopySourcePath(object? parameter)
{
    if (parameter is FileEpisodeModel episode && !string.IsNullOrEmpty(episode.SourcePath))
    {
  try
   {
            System.Windows.Clipboard.SetText(episode.SourcePath);
       StatusMessage = "Source path copied to clipboard";
        }
        catch (Exception ex)
      {
        _dialogService.ShowErrorAsync("Copy Error", $"Failed to copy source path: {ex.Message}");
        }
    }
}

private void CopyDestinationPath(object? parameter)
{
    if (parameter is FileEpisodeModel episode && !string.IsNullOrEmpty(episode.DestinationPath))
    {
        try
        {
     System.Windows.Clipboard.SetText(episode.DestinationPath);
     StatusMessage = "Destination path copied to clipboard";
        }
        catch (Exception ex)
   {
   _dialogService.ShowErrorAsync("Copy Error", $"Failed to copy destination path: {ex.Message}");
     }
    }
}
```

### Bulk Removal Commands
```csharp
private async void RemoveAllFailed(object? parameter)
{
    var failedEpisodes = _allEpisodes.Where(e => e.Status == ProcessingStatus.Failed).ToList();
  
    if (failedEpisodes.Count == 0)
    {
        await _dialogService.ShowMessageAsync("Remove Failed", "No failed episodes to remove.");
        return;
    }

    var result = await _dialogService.ShowConfirmationAsync(
        "Remove All Failed",
     $"Remove all {failedEpisodes.Count} failed episodes from the list?");

  if (result)
    {
        foreach (var episode in failedEpisodes)
        {
   _allEpisodes.Remove(episode);
        }

        ApplyFilters();
 StatusMessage = $"Removed {failedEpisodes.Count} failed episodes. {_allEpisodes.Count} remaining.";
        
        // Refresh command states
      ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
     ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
        ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
        ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
    }
}

private async void RemoveAllCompleted(object? parameter)
{
    var completedEpisodes = _allEpisodes.Where(e => e.Status == ProcessingStatus.Completed).ToList();

    if (completedEpisodes.Count == 0)
    {
        await _dialogService.ShowMessageAsync("Remove Completed", "No completed episodes to remove.");
     return;
    }

    var result = await _dialogService.ShowConfirmationAsync(
     "Remove All Completed",
        $"Remove all {completedEpisodes.Count} completed episodes from the list?");

    if (result)
    {
    foreach (var episode in completedEpisodes)
      {
     _allEpisodes.Remove(episode);
     }

        ApplyFilters();
        StatusMessage = $"Removed {completedEpisodes.Count} completed episodes. {_allEpisodes.Count} remaining.";
        
        // Refresh command states
        ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
        ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
     ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
     ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
 }
}
```

## Command Initialization
```csharp
// In ViewModel constructor
public TvEpisodesViewModel(...)
{
    // ... existing commands ...
    
    // Enhanced context menu commands
    OpenSourceFolderCommand = new RelayCommand(OpenSourceFolder);
    OpenDestinationFolderCommand = new RelayCommand(OpenDestinationFolder);
    CopySourcePathCommand = new RelayCommand(CopySourcePath);
    CopyDestinationPathCommand = new RelayCommand(CopyDestinationPath);
    RemoveAllFailedCommand = new RelayCommand(RemoveAllFailed, 
        _ => Episodes.Any(e => e.Status == ProcessingStatus.Failed) && !IsProcessing);
    RemoveAllCompletedCommand = new RelayCommand(RemoveAllCompleted, 
      _ => Episodes.Any(e => e.Status == ProcessingStatus.Completed) && !IsProcessing);
}
```

## Command Properties
```csharp
public ICommand OpenSourceFolderCommand { get; }
public ICommand OpenDestinationFolderCommand { get; }
public ICommand CopySourcePathCommand { get; }
public ICommand CopyDestinationPathCommand { get; }
public ICommand RemoveAllFailedCommand { get; }
public ICommand RemoveAllCompletedCommand { get; }
```

## IsProcessing Update
```csharp
public bool IsProcessing
{
    get => _isProcessing;
    set
    {
        if (SetProperty(ref _isProcessing, value))
        {
    // Refresh ALL command states including new ones
            ((AsyncRelayCommand)ScanCommand).RaiseCanExecuteChanged();
            ((AsyncRelayCommand)ProcessCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveSelectedCommand).RaiseCanExecuteChanged();
((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
 ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
       ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
   ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
        }
    }
}
```

## Icon Reference
| Operation | Unicode | Character |
|-----------|---------|-----------|
| View Details | `&#x1F4CB;` | ?? |
| Open Folder | `&#x1F4C1;` | ?? |
| Open Destination | `&#x1F4C2;` | ?? |
| Copy Path | `&#x1F4CB;` | ?? |
| Retry | `&#x1F504;` | ?? |
| Remove/Delete | `&#x1F5D1;` | ??? |
| Warning | `&#x26A0;&#xFE0F;` | ?? |
| Checkmark | `&#x2705;` | ? |
| Select All | `&#x2611;&#xFE0F;` | ?? |

## ProcessingStatus Enum Values
```csharp
public enum ProcessingStatus
{
    Pending,  // Not yet processed
    Processing,  // Currently being processed
    Completed, // Successfully processed
    Failed,      // Processing failed
    Unparsed   // Could not parse filename
}
```

## Common Patterns

### Error Handling
```csharp
try
{
    // Operation
}
catch (Exception ex)
{
    _dialogService.ShowErrorAsync("Operation Error", $"Failed: {ex.Message}");
}
```

### Confirmation Dialogs
```csharp
var result = await _dialogService.ShowConfirmationAsync(
    "Title",
    "Message with details");

if (result)
{
    // Proceed with operation
}
```

### Status Messages
```csharp
StatusMessage = "Operation completed successfully";
```

### Clipboard Operations
```csharp
System.Windows.Clipboard.SetText(textToCopy);
```

### Explorer Operations
```csharp
System.Diagnostics.Process.Start("explorer.exe", folderPath);
```

## Testing Checklist

### Status Filter
- [ ] Select "All" - shows all items
- [ ] Select "Pending" - shows only pending
- [ ] Select "Completed" - shows only completed
- [ ] Select "Failed" - shows only failed
- [ ] Filter works with search text
- [ ] Filter persists during operations

### Context Menu - Folder Operations
- [ ] Open Source Folder - existing folder
- [ ] Open Source Folder - non-existent folder
- [ ] Open Destination Folder - existing folder
- [ ] Open Destination Folder - non-existent folder

### Context Menu - Clipboard
- [ ] Copy Source Path - verify in clipboard
- [ ] Copy Destination Path - verify in clipboard
- [ ] Status message appears

### Context Menu - Bulk Removal
- [ ] Remove All Failed - with confirmation
- [ ] Remove All Failed - cancel confirmation
- [ ] Remove All Failed - when no failed items
- [ ] Remove All Completed - with confirmation
- [ ] Remove All Completed - cancel confirmation
- [ ] Remove All Completed - when no completed items

### Command States
- [ ] Commands disabled during processing
- [ ] Commands refresh after filter changes
- [ ] Bulk removal commands disabled when no applicable items
