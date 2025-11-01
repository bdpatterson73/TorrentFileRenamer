# Phase 5 Quick Reference

## ?? Quick Implementation Guide

### Settings Dialog Modernization

#### Card Section Pattern
```xaml
<Border Style="{StaticResource SettingsCard}">
    <StackPanel>
        <TextBlock Text="?? Section Title" Style="{StaticResource SectionHeader}"/>
        
        <TextBlock Text="Setting Label" Style="{StaticResource SettingLabel}"/>
 <TextBox Text="{Binding Property, UpdateSourceTrigger=PropertyChanged}"
                 Style="{StaticResource ModernSettingTextBox}"/>
     <TextBlock Text="Helpful description text" 
       Style="{StaticResource SettingDescription}"/>
    </StackPanel>
</Border>
```

#### Path Input with Browse Button
```xaml
<Grid Style="{StaticResource PathInputGrid}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    
    <TextBox Grid.Column="0" 
 Text="{Binding FolderPath, UpdateSourceTrigger=PropertyChanged}"
   Style="{StaticResource ModernSettingTextBox}"/>
    <Button Grid.Column="1" 
            Content="Browse..." 
            Command="{Binding BrowseFolderCommand}"
    Style="{StaticResource SecondaryButton}"
   Margin="8,0,0,8"
            Width="90"/>
</Grid>
```

#### Info Panel
```xaml
<Border Background="#E3F2FD"
        BorderBrush="{StaticResource PrimaryBrush}"
        BorderThickness="1"
        CornerRadius="8"
        Padding="16">
    <StackPanel>
        <TextBlock Text="? Info Title" 
     FontSize="14"
   FontWeight="SemiBold"
        Foreground="{StaticResource PrimaryBrush}"
         Margin="0,0,0,8"/>
        <TextBlock TextWrapping="Wrap" 
    FontSize="12"
     Foreground="{StaticResource TextPrimaryBrush}">
            Helpful information text here...
        </TextBlock>
    </StackPanel>
</Border>
```

#### Warning Panel
```xaml
<Border Background="#FFF3E0"
        BorderBrush="{StaticResource WarningBrush}"
        BorderThickness="1"
        CornerRadius="8"
        Padding="16">
    <StackPanel>
        <TextBlock Text="? Warning" 
         FontSize="14"
         FontWeight="SemiBold"
Foreground="{StaticResource WarningBrush}"
                 Margin="0,0,0,8"/>
    <TextBlock TextWrapping="Wrap" 
FontSize="12"
      Foreground="{StaticResource TextPrimaryBrush}">
         Warning message here...
 </TextBlock>
    </StackPanel>
</Border>
```

### Settings ViewModel Commands

#### Export Settings Command
```csharp
private void ExecuteExportSettings()
{
    try
    {
  var filePath = _dialogService.ShowSaveFileDialog("settings.json", "JSON Files|*.json");
        if (string.IsNullOrEmpty(filePath))
        return;

     var exportSettings = new
        {
            // All settings properties
            Property1,
      Property2,
      ExportedAt = DateTime.Now,
            Version = "2.0"
     };

        var json = JsonSerializer.Serialize(exportSettings, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
     
        File.WriteAllText(filePath, json);
        
   _dialogService.ShowMessage("Export Successful", 
            $"Settings exported to:\n{filePath}");
    }
    catch (Exception ex)
    {
        _dialogService.ShowMessage("Export Failed", 
            $"Failed to export: {ex.Message}");
    }
}
```

#### Apply Preset Command
```csharp
private void ExecuteApplyPreset()
{
    var result = _dialogService.ShowConfirmation("Apply Preset",
        "This will apply preset configuration. Continue?");
    
    if (!result)
        return;

  // Set all settings to preset values
    Property1 = "preset value";
    Property2 = true;
    // ... more settings

  _dialogService.ShowMessage("Preset Applied", 
        "Preset applied. Review and click Save to confirm.");
}
```

### Keyboard Shortcuts

#### MainWindow Input Bindings
```xaml
<Window.InputBindings>
    <KeyBinding Key="F1" Command="{Binding ShowKeyboardShortcutsCommand}" />
    <KeyBinding Key="S" Modifiers="Control" Command="{Binding ScanCommand}" />
    <KeyBinding Key="P" Modifiers="Control" Command="{Binding ProcessCommand}" />
    <KeyBinding Key="D1" Modifiers="Control" Command="{Binding SwitchToTvTabCommand}" />
    <KeyBinding Key="D2" Modifiers="Control" Command="{Binding SwitchToMoviesTabCommand}" />
 <KeyBinding Key="D3" Modifiers="Control" Command="{Binding SwitchToAutoMonitorTabCommand}" />
    <KeyBinding Key="OemComma" Modifiers="Control" Command="{Binding ShowSettingsCommand}" />
</Window.InputBindings>
```

#### Tab Switching Commands
```csharp
SwitchToTvTabCommand = new RelayCommand(_ => SelectedTabIndex = 0);
SwitchToMoviesTabCommand = new RelayCommand(_ => SelectedTabIndex = 1);
SwitchToAutoMonitorTabCommand = new RelayCommand(_ => SelectedTabIndex = 2);
```

#### Show Shortcuts Dialog Command
```csharp
private void ExecuteShowKeyboardShortcuts(object? parameter)
{
    try
    {
        var dialog = new Views.KeyboardShortcutsDialog
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        dialog.ShowDialog();
        StatusMessage = "Keyboard shortcuts reference displayed";
    }
    catch (Exception ex)
    {
  StatusMessage = $"Error showing shortcuts: {ex.Message}";
    }
}
```

### Keyboard Shortcuts Dialog Layout

#### Shortcut Row
```xaml
<Border Style="{StaticResource ShortcutRow}">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
        </Grid.ColumnDefinitions>
     
     <TextBlock Grid.Column="0" 
        Text="Action Description" 
     Style="{StaticResource ShortcutDescription}"/>
        
        <StackPanel Grid.Column="1" Orientation="Horizontal">
            <Border Style="{StaticResource ShortcutKey}">
      <TextBlock Text="Ctrl" Style="{StaticResource ShortcutKeyText}"/>
      </Border>
      <TextBlock Text="+" Margin="4,0" VerticalAlignment="Center"/>
          <Border Style="{StaticResource ShortcutKey}">
          <TextBlock Text="S" Style="{StaticResource ShortcutKeyText}"/>
    </Border>
        </StackPanel>
    </Grid>
</Border>
```

### Toolbar Enhancements

#### Direct Action Buttons
```xaml
<Button Content="?? Scan TV" 
        Style="{StaticResource PrimaryButton}"
        Command="{Binding ScanTvCommand}"
        ToolTip="Scan for TV episodes (Ctrl+S when on TV tab)"/>

<Button Content="?? Scan Movies" 
        Style="{StaticResource PrimaryButton}"
        Command="{Binding ScanMoviesCommand}"
        ToolTip="Scan for movies (Ctrl+S when on Movies tab)"/>
```

#### Scan with Tab Switch
```csharp
private void ExecuteScanTv(object? parameter)
{
    SelectedTabIndex = 0; // Switch to TV tab
    if (_tvEpisodesViewModel?.ScanCommand.CanExecute(null) == true)
    {
        _tvEpisodesViewModel.ScanCommand.Execute(null);
    }
}
```

## ?? Style Reference

### Colors
```xaml
<!-- Primary Palette -->
PrimaryBrush: #2196F3
PrimaryDarkBrush: #1976D2
PrimaryLightBrush: #BBDEFB

<!-- Status Colors -->
SuccessBrush: #4CAF50
WarningBrush: #FF9800
ErrorBrush: #F44336

<!-- Text Colors -->
TextPrimaryBrush: #212121
TextSecondaryBrush: #757575

<!-- Background -->
BackgroundBrush: #FFFFFF
SurfaceBrush: #FAFAFA
BorderBrush: #E0E0E0
```

### Typography Scale
```
Section Header:   18px, SemiBold, Primary color
Section Subheader: 14px, Medium, Primary text
Setting Label:    13px, Medium, Primary text
Setting Input:    13px, Regular
Description:      11px, Regular, Secondary text
```

### Spacing
```
Card margin:      0,0,0,12
Card padding:     16px
Input margin:0,0,0,8
Section margin: 0,0,0,16
```

## ?? Checklist for Adding New Settings

- [ ] Add private field to ViewModel
- [ ] Add public property with SetProperty
- [ ] Load value in LoadSettings()
- [ ] Save value in SaveSettings()
- [ ] Add validation in ValidateSettings() if needed
- [ ] Add UI control in appropriate XAML tab
- [ ] Add label and description
- [ ] Test with Export/Import
- [ ] Update presets if relevant
- [ ] Document in comments

## ?? Common Patterns

### Validation with Visual Feedback
```csharp
private bool ValidateSettings()
{
    if (SomeValue < 1 || SomeValue > 100)
 {
        _dialogService.ShowMessage("Invalid Setting", 
          "Value must be between 1 and 100.");
    return false;
    }
    return true;
}
```

### Confirmation Before Destructive Action
```csharp
private void ExecuteDestructiveAction()
{
    var result = _dialogService.ShowConfirmation(
        "Confirm Action",
        "This will overwrite existing data. Continue?");
    
    if (!result)
        return;
    
    // Perform action
}
```

### Status Bar Updates
```csharp
StatusMessage = "Action in progress...";
// Perform action
StatusMessage = "Action completed successfully";
```

### Slider with Live Value Display
```xaml
<Slider Value="{Binding SliderValue}" 
    Minimum="5" Maximum="300" 
        TickFrequency="5"
  IsSnapToTickEnabled="True"/>

<TextBlock>
    <Run Text="Current value: "/>
    <Run Text="{Binding SliderValue}" FontWeight="SemiBold"/>
    <Run Text=" units"/>
</TextBlock>
```

## ?? Troubleshooting

### Settings Not Saving
- Check ValidateSettings() return value
- Verify SaveSettings() called before _settings.Save()
- Check file write permissions

### Commands Not Firing
- Verify Command binding in XAML
- Check CanExecute logic
- Ensure RelayCommand initialized in constructor

### Shortcuts Not Working
- Verify KeyBinding in Window.InputBindings
- Check if command is registered
- Test with F1 (should always work)

### Import/Export Issues
- Ensure JSON format matches expected structure
- Check file permissions
- Verify JsonSerializer namespace (System.Text.Json)

## ?? Resources

### Files to Reference
- `SettingsDialog.xaml` - Complete UI implementation
- `SettingsViewModel.cs` - All commands and logic
- `KeyboardShortcutsDialog.xaml` - Shortcuts layout
- `MainViewModel.cs` - Global shortcuts
- `Colors.xaml` - Color palette
- `CardStyles.xaml` - Card and badge styles

### Key Namespaces
```csharp
using System.Text.Json;
using System.IO;
using System.Windows;
using System.Windows.Input;
```

---

**Quick Reference Version**: 1.0  
**Phase**: 5  
**Last Updated**: Phase 5 Completion  
**Status**: ? Complete
