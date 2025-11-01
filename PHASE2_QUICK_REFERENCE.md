# Phase 2 - Quick Reference Guide

## FileEpisodeCard Layout

```xml
<controls:FileEpisodeCard DataContext="{Binding EpisodeModel}"/>
```

### Card Structure (Top to Bottom)

1. **Status Accent Bar** (4px left border)
   - Color from `StatusToBrushConverter`
   - Spans all rows

2. **Header** (Grid Row 0)
   - Status icon (24px)
   - Show name (Title style)
- Season/Episode (Subtitle style)
   - Status badge (pill)

3. **Multi-Episode Indicator** (Grid Row 2, conditional)
   - Visible only when `IsMultiEpisode = true`
   - Orange warning background
   - Lists all episode numbers

4. **Original Filename** (Grid Row 4)
   - Light gray background
   - "ORIGINAL FILENAME" caption
   - Full filename with truncation

5. **New Filename** (Grid Row 6)
   - Green success background
   - "NEW FILENAME" caption
   - Bold text

6. **Paths Section** (Grid Row 8)
   - Source path with folder icon
   - Destination path with folder icon
   - Metadata gray text

7. **Metadata Footer** (Grid Row 10)
   - Plex validation (left)
   - File size + extension (right)
   - Light gray background

8. **Error Section** (Grid Row 12, conditional)
   - Visible only when `ErrorMessage` exists
   - Red background
   - Warning icon
   - Full error text

9. **Action Buttons** (Grid Row 14)
 - View Details
   - Open Folder
   - Remove
   - Retry (only for Failed status)

## View Controls Usage

### View Mode Selector
```xml
<RadioButton Content="Cards"
     Style="{StaticResource ViewToggleButton}"
        IsChecked="{Binding IsCardViewSelected}"/>
```

### Search Box
```xml
<TextBox Width="250"
  Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
         ToolTip="Search by show name, filename, or path"/>
```

### Content Switcher
```xml
<!-- Card View -->
<ScrollViewer Visibility="{Binding IsCardViewSelected, Converter={StaticResource BoolToVisibilityConverter}}">
    <ItemsControl ItemsSource="{Binding Episodes}">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <controls:FileEpisodeCard Margin="0,0,0,12"/>
        </DataTemplate>
  </ItemsControl.ItemTemplate>
    </ItemsControl>
</ScrollViewer>

<!-- Grid View -->
<DataGrid Visibility="{Binding IsGridViewSelected, Converter={StaticResource BoolToVisibilityConverter}}"
   ItemsSource="{Binding Episodes}"/>
```

## New Converters

### StatusToRetryVisibilityConverter
```xml
Visibility="{Binding Status, Converter={StaticResource StatusToRetryVisibilityConverter}}"
```
- Returns `Visible` for `ProcessingStatus.Failed`
- Returns `Collapsed` for all other statuses

### StringToVisibilityConverter
```xml
Visibility="{Binding ErrorMessage, Converter={StaticResource StringToVisibilityConverter}}"
```
- Returns `Visible` if string is not null/empty/whitespace
- Returns `Collapsed` otherwise

## ViewModel Commands

### Card Action Commands
```csharp
// In TvEpisodesViewModel
public ICommand ViewDetailsCommand { get; }    // Shows episode details
public ICommand OpenFolderCommand { get; }     // Opens file explorer
public ICommand RetryCommand { get; }      // Retries failed episode
```

### Command Bindings in Card
```xml
Command="{Binding DataContext.ViewDetailsCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
CommandParameter="{Binding}"
```

### Command Usage
- **ViewDetails**: Shows message dialog with all episode info
- **OpenFolder**: Opens Windows Explorer to source folder
- **Retry**: Re-processes failed episode, updates status
- **Remove**: Uses existing `RemoveSelectedCommand`

## FileEpisodeModel Properties

### Display Properties
```csharp
ShowName            // "Game of Thrones"
SeasonNumber        // 1
EpisodeNumber       // 1
EpisodeNumbers   // "1, 2, 3" (for multi-episode)
IsMultiEpisode      // bool
NewFileName         // "Game.of.Thrones.S01E01.mkv"
Extension     // ".mkv"
Status     // ProcessingStatus enum
StatusText          // "Pending", "Failed", etc.
ErrorMessage        // Error details
SourcePath      // Full source path
DestinationPath // Full destination path
PlexValidation      // Plex compatibility message
CoreEpisode.FileName   // Original filename
CoreEpisode.FileSize  // Bytes (long)
```

## Color Reference

### Status Colors (from StatusToBrushConverter)
- **Pending**: #2196F3 (Blue)
- **Processing**: #FFC107 (Amber)
- **Completed**: #4CAF50 (Green)
- **Failed**: #F44336 (Red)
- **Unparsed**: #9E9E9E (Gray)

### Section Colors
- **CardSectionBackground**: #F5F5F5 (Light Gray)
- **CardSuccessSection**: #E8F5E9 (Light Green)
- **CardErrorSection**: #FFEBEE (Light Red)
- **CardWarningSection**: #FFF3E0 (Light Orange)

### Text Colors
- **Primary**: #212121 (Almost Black)
- **Secondary**: #616161 (Dark Gray)
- **Body**: #757575 (Medium Gray)
- **Caption**: #9E9E9E (Light Gray)
- **Metadata**: #BDBDBD (Very Light Gray)

## Icon Reference (Unicode Escapes)

```xml
<!-- Folder Icons -->
&#x1F4C1;  <!-- ?? Open folder (source) -->
&#x1F4C2;  <!-- ?? Closed folder (destination) -->

<!-- Other Icons -->
&#x1F4D1;  <!-- ?? Multi-document (multi-episode) -->
&#x1F50D;  <!-- ?? Magnifying glass (search) -->
&#x26A0;&#xFE0F;  <!-- ?? Warning sign (error) -->
```

## Status Icons (from StatusToIconConverter)

Automatically displays appropriate icon based on status:
- **Pending**: ? Hourglass
- **Processing**: ?? Gear
- **Completed**: ? Checkmark
- **Failed**: ? X
- **Unparsed**: ? Question mark

## Layout Measurements

### Card
- Border radius: 8px
- Shadow: 4px offset, 8px blur, 15% opacity
- Padding: 16px (base), 20px (with accent bar)

### Sections
- Border radius: 6px
- Padding: 12px (standard), 8px vertical (metadata)
- Spacing: 8-12px between sections

### Typography Sizes
- Title: 18px SemiBold
- Subtitle: 14px Medium
- Body: 14px Regular
- Caption: 12px
- Metadata: 11px

### Accent Bar
- Width: 4px
- Position: Left edge
- Span: Full card height

### Status Badge
- Padding: 8px horizontal, 4px vertical
- Border radius: 12px
- Text: 12px White

## TvEpisodesView Layout

```
???????????????????????????????????
? Toolbar (Row 0)                 ?
???????????????????????????????????
? View: ?Cards ?Compact ?Grid  ?? ? (Row 1)
???????????????????????????????????
?    ?
?  ????????????????????????????? ?
?  ? Card 1        ? ?
?  ????????????????????????????? ?
?           ?
?  ????????????????????????????? ? (Row 2 - Content)
?  ? Card 2                    ? ?
?  ????????????????????????????? ?
?          ?
?  ????????????????????????????? ?
?  ? Card 3    ? ?
?  ????????????????????????????? ?
?               ?
???????????????????????????????????
? Status: X episodes | Count: X   ? (Row 3)
???????????????????????????????????
```

## Search Behavior

The search filter checks:
1. `ShowName` (case-insensitive)
2. `NewFileName` (case-insensitive)
3. `SourcePath` (case-insensitive)

Real-time filtering with `UpdateSourceTrigger=PropertyChanged`

## View Mode Persistence

Currently sets view mode properties but doesn't persist.
TODO: Integrate with `IWindowStateService` for preference saving.

## Testing Checklist

- [ ] Switch between Card/Grid views
- [ ] Search by show name
- [ ] Search by filename
- [ ] Search by path
- [ ] View Details button
- [ ] Open Folder button
- [ ] Remove button
- [ ] Retry button (on failed episode)
- [ ] Multi-episode indicator visibility
- [ ] Error section visibility
- [ ] Card entrance animation
- [ ] Plex validation display
- [ ] File size formatting
- [ ] Tooltip on truncated text
- [ ] Status color accuracy
- [ ] Empty state (no episodes)

## Common Issues & Solutions

### Issue: Card doesn't show
**Solution**: Check `Episodes` collection has data and `IsCardViewSelected = true`

### Issue: Commands don't fire
**Solution**: Verify `RelativeSource={RelativeSource AncestorType=UserControl}` in binding

### Issue: Search doesn't filter
**Solution**: Ensure `SearchText` has `UpdateSourceTrigger=PropertyChanged`

### Issue: Icons don't display
**Solution**: Font may not support emoji. Use Unicode escapes (&#xHEXCODE;)

### Issue: Retry button always visible
**Solution**: Check `StatusToRetryVisibilityConverter` is registered in App.xaml

## Extension Points

### Custom Details Dialog
Replace `ViewDetails` method with custom dialog:
```csharp
var dialog = new EpisodeDetailsDialog { DataContext = episode };
dialog.ShowDialog();
```

### Add Status Filter
```xml
<ComboBox SelectedValue="{Binding StatusFilter}">
    <ComboBoxItem Content="All" Tag="{x:Null}"/>
    <ComboBoxItem Content="Pending" Tag="{x:Static models:ProcessingStatus.Pending}"/>
    <!-- ... -->
</ComboBox>
```

### Add Sort Options
Implement sort commands in ViewModel:
```csharp
public ICommand SortByNameCommand { get; }
public ICommand SortByStatusCommand { get; }
```

### Virtual Scrolling
For large lists (>1000 items), use `VirtualizingStackPanel`:
```xml
<ItemsControl.ItemsPanel>
    <ItemsPanelTemplate>
 <VirtualizingStackPanel/>
    </ItemsPanelTemplate>
</ItemsControl.ItemsPanel>
```

## Performance Tips

1. **Bind with Mode=OneWay** for read-only data
2. **Use TextTrimming** instead of custom logic
3. **Collapse empty sections** with converters
4. **Avoid nested ItemsControls** in card template
5. **Use cached brushes** from resources

## Accessibility

- High contrast status colors
- Large button targets (min 44x44)
- Clear visual hierarchy
- Descriptive button text
- Tooltips for truncated content
- Keyboard navigation support (built-in)

---

**Phase 2 Complete! Ready for Phase 3: Compact View & Movies Card View**
