# Phase 1 - Quick Reference Guide

## Available Resources

### Converters (All globally registered in App.xaml)
```xml
{StaticResource StatusToBrushConverter}       - ProcessingStatus ? Brush
{StaticResource StatusToIconConverter}    - ProcessingStatus ? Unicode Icon
{StaticResource FileSizeConverter}         - bytes ? "1.2 GB"
{StaticResource DateTimeToRelativeConverter}  - DateTime ? "2 minutes ago"
{StaticResource BoolToVisibilityConverter}    - bool ? Visibility (existing)
```

### Styles (All defined in CardStyles.xaml)
```xml
{StaticResource FileCard}        - Base card container
{StaticResource StatusBorder}           - 4px accent bar
{StaticResource CardTitle}           - 18px SemiBold heading
{StaticResource CardSubtitle}    - 14px Medium subheading
{StaticResource CardBody}    - 14px Regular content
{StaticResource CardCaption}            - 12px caption text
{StaticResource CardMetadata}           - 11px metadata text
{StaticResource ViewToggleButton}       - Radio button for view modes
{StaticResource CardSectionBackground}  - Light gray section background
{StaticResource CardSuccessSection}     - Green success background
{StaticResource CardErrorSection}       - Red error background
{StaticResource CardWarningSection}     - Orange warning background
{StaticResource StatusBadge}            - Pill-shaped badge container
{StaticResource StatusBadgeText}      - White text for badges
```

### Animations (All defined in Animations.xaml)
```xml
{StaticResource CardEntranceAnimation}      - Fade + slide up (200ms)
{StaticResource ProcessingPulseAnimation}   - Infinite pulse for processing
{StaticResource CardHoverAnimation}       - Shadow increase on hover
{StaticResource FadeInAnimation}       - Simple fade in (300ms)
{StaticResource FadeOutAnimation}           - Simple fade out (200ms)
{StaticResource SlideInFromBottomAnimation} - Slide + fade entrance
{StaticResource ExpandAnimation}            - Height expansion
{StaticResource CollapseAnimation}    - Height collapse
```

### Material Design Colors
```
Pending:  #2196F3 (Blue)
Processing:  #FFC107 (Amber)
Completed:   #4CAF50 (Green)
Failed:      #F44336 (Red)
Unparsed:    #9E9E9E (Gray)

Background:  #FFFFFF (White)
Border:      #E0E0E0 (Light Gray)
Primary:     #212121 (Almost Black)
Secondary:   #616161 (Dark Gray)
Body:        #757575 (Medium Gray)
Caption:     #9E9E9E (Light Gray)
Metadata:    #BDBDBD (Very Light Gray)
```

### ViewModel Bindings

#### TvEpisodesViewModel
```xml
IsCardViewSelected      - bool (default: true)
IsCompactViewSelected   - bool
IsGridViewSelected      - bool
SearchText         - string (filters in real-time)
StatusFilter      - ProcessingStatus? (filters in real-time)
Episodes     - ObservableCollection<FileEpisodeModel> (filtered)

<!-- Commands remain unchanged -->
ScanCommand
ProcessCommand
RemoveSelectedCommand
ClearAllCommand
RemoveUnparsedCommand
SelectAllCommand
```

#### MoviesViewModel
```xml
IsCardViewSelected   - bool (default: true)
IsCompactViewSelected   - bool
IsGridViewSelected      - bool
SearchText    - string (filters in real-time)
StatusFilter    - ProcessingStatus? (filters in real-time)
Movies      - ObservableCollection<MovieFileModel> (filtered)

<!-- Commands remain unchanged -->
ScanCommand
ProcessCommand
RemoveSelectedCommand
ClearAllCommand
RemoveLowConfidenceCommand
SelectAllCommand
```

## Example Card Template (for Phase 2)

```xml
<Border Style="{StaticResource FileCard}">
    <Grid>
        <!-- Status Accent Bar -->
 <Border Grid.RowSpan="9" 
  Style="{StaticResource StatusBorder}"
           Background="{Binding Status, Converter={StaticResource StatusToBrushConverter}}"/>

        <Grid Margin="20,0,0,0">
 <Grid.RowDefinitions>
         <RowDefinition Height="Auto"/> <!-- Header -->
        <RowDefinition Height="Auto"/> <!-- Content -->
     <RowDefinition Height="Auto"/> <!-- Footer -->
    </Grid.RowDefinitions>
    
      <!-- Header -->
     <Grid Grid.Row="0">
        <Grid.ColumnDefinitions>
 <ColumnDefinition Width="Auto"/> <!-- Icon -->
          <ColumnDefinition Width="*"/>    <!-- Title -->
      <ColumnDefinition Width="Auto"/> <!-- Badge -->
      </Grid.ColumnDefinitions>
                
<!-- Status Icon -->
                <TextBlock Grid.Column="0"
      Text="{Binding Status, Converter={StaticResource StatusToIconConverter}}"
       FontSize="24"
      VerticalAlignment="Center"
     Margin="0,0,12,0"
      Foreground="{Binding Status, Converter={StaticResource StatusToBrushConverter}}"/>
           
     <!-- Title -->
    <TextBlock Grid.Column="1"
   Style="{StaticResource CardTitle}"
               Text="{Binding Title}"/>
    
  <!-- Status Badge -->
          <Border Grid.Column="2"
      Style="{StaticResource StatusBadge}"
                   Background="{Binding Status, Converter={StaticResource StatusToBrushConverter}}">
  <TextBlock Style="{StaticResource StatusBadgeText}"
              Text="{Binding StatusText}"/>
           </Border>
            </Grid>
   
            <!-- Content sections here -->
   
        </Grid>
    </Grid>
</Border>
```

## Example View Toggle

```xml
<StackPanel Orientation="Horizontal">
    <TextBlock Text="View:" 
      VerticalAlignment="Center"
   Margin="0,0,12,0"/>
    
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

## Example Search Box

```xml
<TextBox Width="200"
  Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
         ToolTip="Search by name, file, or path"/>
```

## Example Status Filter

```xml
<ComboBox SelectedValue="{Binding StatusFilter}"
          SelectedValuePath="Tag">
    <ComboBoxItem Content="All" Tag="{x:Null}"/>
    <ComboBoxItem Content="Pending" Tag="{x:Static models:ProcessingStatus.Pending}"/>
    <ComboBoxItem Content="Completed" Tag="{x:Static models:ProcessingStatus.Completed}"/>
    <ComboBoxItem Content="Failed" Tag="{x:Static models:ProcessingStatus.Failed}"/>
    <ComboBoxItem Content="Unparsed" Tag="{x:Static models:ProcessingStatus.Unparsed}"/>
</ComboBox>
```

## Ready for Phase 2

All foundation components are in place:
- ? Styles defined
- ? Animations created
- ? Converters implemented
- ? ViewModels enhanced
- ? Resources merged
- ? Build successful

Next: Create FileEpisodeCard and MovieFileCard user controls!
