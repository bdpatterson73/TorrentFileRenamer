# UI Modernization Plan - Card-Based Layout Implementation

## Executive Summary

This document outlines a comprehensive plan to modernize the TorrentFileRenamer WPF application UI by replacing the traditional DataGrid-based interface with a modern card-based layout. The new design will provide better visual hierarchy, improved information density, and a more engaging user experience while maintaining all existing functionality.

---

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [Proposed UI Improvements](#proposed-ui-improvements)
3. [Design System](#design-system)
4. [Implementation Plan](#implementation-plan)
5. [Technical Specifications](#technical-specifications)
6. [Migration Strategy](#migration-strategy)
7. [User Experience Enhancements](#user-experience-enhancements)
8. [Performance Considerations](#performance-considerations)
9. [Testing Strategy](#testing-strategy)
10. [Timeline & Resources](#timeline--resources)

---

## 1. Current State Analysis

### 1.1 Current UI Implementation

**TvEpisodesView & MoviesView:**
- Uses `DataGrid` with 7-8 columns
- Row-based color coding for status
- Horizontal scrolling required for all information
- Dense information display with limited visual hierarchy
- Status indicated only by background color

**Strengths:**
- ? Familiar spreadsheet-like interface
- ? Efficient for viewing many items at once
- ? Built-in sorting and column resizing
- ? Established pattern users understand

**Weaknesses:**
- ? Poor information hierarchy
- ? Requires horizontal scrolling
- ? Limited visual distinction between items
- ? Status not immediately obvious
- ? No space for additional metadata or thumbnails
- ? Difficult to scan quickly for specific information
- ? Not visually engaging or modern
- ? Hard to show relationships (before/after filenames)

### 1.2 User Workflow Pain Points

1. **File Identification**: Hard to quickly identify which files have been processed
2. **Error Discovery**: Errors buried in rightmost column, require scrolling
3. **Status Understanding**: Color-coded backgrounds subtle, need better indicators
4. **Path Comparison**: Difficult to compare source vs. destination paths
5. **Multi-Episode Recognition**: Multi-episode files not visually distinct
6. **Confidence Level**: Movie confidence buried in small indicator

---

## 2. Proposed UI Improvements

### 2.1 Card-Based Layout Overview

Replace the DataGrid with a scrollable list of cards where each card represents a file with:
- **Visual Status Indicators**: Icons, badges, and accent colors
- **Hierarchical Information**: Important details prominent, secondary details subtle
- **Before/After Comparison**: Clear visual comparison of source ? destination
- **Expandable Details**: Click to expand for full path information
- **Action Buttons**: Quick actions directly on the card
- **Progress Indicators**: Per-item progress during processing

### 2.2 View Mode Options

Provide multiple view modes accessible via toggle buttons:

1. **Card View (Default)** - Detailed cards with all information
2. **Compact View** - Smaller cards showing essential information only
3. **Grid View (Classic)** - Keep existing DataGrid as an option for power users
4. **List View** - Single-line items with icons

### 2.3 Card Layout Mockup

```
???????????????????????????????????????????????????????????????????????
? [?] Show Name - S01E05               [?] Completed       ?
?     Multi-Episode: E05-E06   ?? 2 min ago?
???????????????????????????????????????????????????????????????????????
? Original: show.name.s01e05-06.720p.hdtv.x264.mkv   ?
?      New: Show Name - S01E05-E06.mkv                   ?
?               ?
? ?? Source: D:\Downloads\TV\            ?
? ?? Dest:   D:\Plex\TV Shows\Show Name\Season 01\       ?
?  ?
? ? Plex Compatible | 1.2 GB | .mkv  ?
?   ?
? [View Details] [Open Folder] [Remove]              [? Retry]       ?
???????????????????????????????????????????????????????????????????????
```

### 2.4 Status Visual Language

**Pending:**
```
???????????????????????????????????????????
? [ ] Title       [?] Pending ?
?     Light blue left border    ?
???????????????????????????????????????????
```

**Processing:**
```
???????????????????????????????????????????
? [?] Title[?] Processing ?
?     Animated yellow left border          ?
???????????????????????????????????????????
```

**Completed:**
```
???????????????????????????????????????????
? [?] Title   [?] Completed  ?
?     Green left border + checkmark        ?
???????????????????????????????????????????
```

**Failed:**
```
???????????????????????????????????????????
? [?] Title    [?] Failed  ?
?     Red left border + error icon         ?
?     ? Error: Access denied to file      ?
???????????????????????????????????????????
```

**Unparsed:**
```
???????????????????????????????????????????
? [?] Unknown.File.mkv      [?] Unparsed ?
?     Gray left border + question mark     ?
???????????????????????????????????????????
```

---

## 3. Design System

### 3.1 Card Component Specifications

#### Card Structure
```xml
<Border Style="{StaticResource FileCard}">
    <Grid>
        <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/> <!-- Header -->
        <RowDefinition Height="Auto"/> <!-- File Info -->
            <RowDefinition Height="Auto"/> <!-- Paths -->
            <RowDefinition Height="Auto"/> <!-- Metadata -->
    <RowDefinition Height="Auto"/> <!-- Actions -->
     </Grid.RowDefinitions>
        
        <!-- Status accent bar -->
        <Border Grid.RowSpan="5" Width="4" 
          HorizontalAlignment="Left"
     Background="{Binding StatusColor}"/>
     
     <!-- Card content -->
    </Grid>
</Border>
```

#### Card Dimensions
- **Full Width**: Fills available width with margins
- **Min Height**: 120px (collapsed)
- **Max Height**: 300px (expanded)
- **Padding**: 16px
- **Margin**: 8px vertical
- **Border Radius**: 8px
- **Shadow**: Subtle elevation shadow

#### Typography Hierarchy
```
Title:      18px, SemiBold, #212121
Subtitle:   14px, Medium,   #616161
Body:       14px, Regular,  #757575
Caption:12px, Regular,  #9E9E9E
Meta:       11px, Regular,  #BDBDBD
```

#### Color System

**Status Colors:**
```csharp
Pending:     #2196F3  (Blue)
Processing:  #FFC107  (Amber)
Completed:   #4CAF50  (Green)
Failed:      #F44336  (Red)
Unparsed:    #9E9E9E  (Gray)
```

**Confidence Colors (Movies):**
```csharp
High (70-100%):    #4CAF50  (Green)
Medium (40-69%):   #FF9800  (Orange)
Low (0-39%):       #F44336  (Red)
```

**UI Elements:**
```csharp
Card Background:   #FFFFFF
Card Border: #E0E0E0
Card Hover:        #F5F5F5
Card Shadow: rgba(0,0,0,0.1)
Icon Color:        #757575
Accent Color:      #2196F3
```

### 3.2 Icon Library

Use Unicode symbols or icon font:
```
?  Checkmark (Completed)
?  X Mark (Failed)
?  Question (Unparsed)
?  Circle (Pending)
?  Half Circle (Processing)
?  Rotating Arrow (Retry)
?? Folder (Source)
?? Open Folder (Destination)
?? Clock (Timestamp)
?  Info (Metadata)
?  Warning (Error/Alert)
?? TV (Episode)
?? Movie (Film)
? Star (High Confidence)
```

### 3.3 Animation System

**Card Entrance:**
- Fade in + Slide up (200ms)
- Stagger by 50ms per card

**Card Hover:**
- Slight elevation increase (100ms ease-out)
- Border color brightens

**Card Selection:**
- Scale slightly (1.02x)
- Shadow deepens

**Status Change:**
- Color transition (300ms ease)
- Icon cross-fade (200ms)

**Processing Animation:**
- Pulsing left border (1s loop)
- Rotating spinner icon

---

## 4. Implementation Plan

### Phase 1: Foundation (Week 1)

#### 4.1 Create Card Component Infrastructure

**Files to Create:**
1. `TorrentFileRenamer.WPF\Views\Controls\FileEpisodeCard.xaml`
2. `TorrentFileRenamer.WPF\Views\Controls\FileEpisodeCard.xaml.cs`
3. `TorrentFileRenamer.WPF\Views\Controls\MovieFileCard.xaml`
4. `TorrentFileRenamer.WPF\Views\Controls\MovieFileCard.xaml.cs`
5. `TorrentFileRenamer.WPF\Resources\CardStyles.xaml`
6. `TorrentFileRenamer.WPF\Resources\Animations.xaml`
7. `TorrentFileRenamer.WPF\Resources\Icons.xaml`

**Card Styles Resource:**
```xml
<ResourceDictionary>
  <!-- Base Card Style -->
    <Style x:Key="FileCard" TargetType="Border">
        <Setter Property="Background" Value="White"/>
        <Setter Property="BorderBrush" Value="#E0E0E0"/>
<Setter Property="BorderThickness" Value="1"/>
        <Setter Property="CornerRadius" Value="8"/>
        <Setter Property="Padding" Value="16"/>
  <Setter Property="Margin" Value="8,4"/>
  <Setter Property="Effect">
            <Setter.Value>
    <DropShadowEffect Color="#000000" 
             Opacity="0.1" 
           BlurRadius="8" 
   ShadowDepth="2"/>
            </Setter.Value>
  </Setter>
    </Style>
    
    <!-- Status Border Styles -->
    <Style x:Key="StatusBorder" TargetType="Border">
        <Setter Property="Width" Value="4"/>
     <Setter Property="CornerRadius" Value="4,0,0,4"/>
        <Setter Property="HorizontalAlignment" Value="Left"/>
    </Style>
    
    <!-- Card Title Style -->
    <Style x:Key="CardTitle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="18"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Foreground" Value="#212121"/>
        <Setter Property="TextTrimming" Value="CharacterEllipsis"/>
    </Style>
    
 <!-- Card Subtitle Style -->
    <Style x:Key="CardSubtitle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="FontWeight" Value="Medium"/>
        <Setter Property="Foreground" Value="#616161"/>
        <Setter Property="TextTrimming" Value="CharacterEllipsis"/>
    </Style>

    <!-- Card Metadata Style -->
    <Style x:Key="CardMetadata" TargetType="TextBlock">
        <Setter Property="FontSize" Value="12"/>
  <Setter Property="Foreground" Value="#9E9E9E"/>
    </Style>
</ResourceDictionary>
```

#### 4.2 Create Status to Color Converters

**File to Create:**
`TorrentFileRenamer.WPF\Converters\StatusToBrushConverter.cs`

```csharp
public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProcessingStatus status)
        {
            return status switch
            {
       ProcessingStatus.Pending => new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)),
  ProcessingStatus.Processing => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07)),
            ProcessingStatus.Completed => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),
            ProcessingStatus.Failed => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)),
        ProcessingStatus.Unparsed => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
       _ => new SolidColorBrush(Colors.Gray)
    };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

**Additional Converters:**
- `StatusToIconConverter` - Returns icon string based on status
- `FileSizeToBytesConverter` - Formats file size (1.2 GB, 850 MB)
- `DateTimeToRelativeConverter` - "2 minutes ago", "Yesterday"
- `BoolToVisibilityConverter` - Already exists

### Phase 2: TV Episodes Card View (Week 2)

#### 4.3 FileEpisodeCard Component

**File: TorrentFileRenamer.WPF\Views\Controls\FileEpisodeCard.xaml**

```xml
<UserControl x:Class="TorrentFileRenamer.WPF.Views.Controls.FileEpisodeCard"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:models="clr-namespace:TorrentFileRenamer.WPF.Models"
             xmlns:converters="clr-namespace:TorrentFileRenamer.WPF.Converters"
  d:DataContext="{d:DesignInstance Type=models:FileEpisodeModel}">
    
    <UserControl.Resources>
        <converters:StatusToBrushConverter x:Key="StatusToBrushConverter"/>
    <converters:StatusToIconConverter x:Key="StatusToIconConverter"/>
  <converters:FileSizeConverter x:Key="FileSizeConverter"/>
    </UserControl.Resources>
    
    <Border Style="{StaticResource FileCard}">
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/> <!-- Header -->
            <RowDefinition Height="Auto"/> <!-- Original Filename -->
    <RowDefinition Height="Auto"/> <!-- New Filename -->
        <RowDefinition Height="8"/>    <!-- Spacer -->
          <RowDefinition Height="Auto"/> <!-- Paths -->
       <RowDefinition Height="8"/>    <!-- Spacer -->
       <RowDefinition Height="Auto"/> <!-- Metadata -->
                <RowDefinition Height="Auto"/> <!-- Error (if any) -->
         <RowDefinition Height="Auto"/> <!-- Actions -->
    </Grid.RowDefinitions>
     
        <!-- Status Accent Bar -->
       <Border Grid.RowSpan="9" 
     Style="{StaticResource StatusBorder}"
              Background="{Binding Status, Converter={StaticResource StatusToBrushConverter}}"/>
     
      <!-- Header Row -->
  <Grid Grid.Row="0" Margin="20,0,0,8">
     <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/> <!-- Status Icon -->
          <ColumnDefinition Width="*"/> <!-- Title -->
    <ColumnDefinition Width="Auto"/> <!-- Status Badge -->
          </Grid.ColumnDefinitions>
                
      <!-- Status Icon -->
      <TextBlock Grid.Column="0"
    Text="{Binding Status, Converter={StaticResource StatusToIconConverter}}"
             FontSize="24"
         VerticalAlignment="Center"
                   Margin="0,0,12,0"
               Foreground="{Binding Status, Converter={StaticResource StatusToBrushConverter}}"/>
              
           <!-- Title -->
       <StackPanel Grid.Column="1" VerticalAlignment="Center">
                 <TextBlock Style="{StaticResource CardTitle}">
            <Run Text="{Binding ShowName}"/>
  <Run Text=" - " Foreground="#757575"/>
            <Run Text="{Binding SeasonNumber, StringFormat='S{0:00}'}"/>
        <Run Text="{Binding EpisodeNumbers, StringFormat='E{0}'}"/>
           </TextBlock>
     
      <!-- Multi-Episode Indicator -->
               <TextBlock Visibility="{Binding IsMultiEpisode, Converter={StaticResource BoolToVisibilityConverter}}"
               Style="{StaticResource CardSubtitle}"
         Foreground="#FF9800"
      Margin="0,2,0,0">
                <Run Text="??"/>
       <Run Text="Multi-Episode: "/>
   <Run Text="{Binding EpisodeNumbers, StringFormat='E{0}'}"/>
               </TextBlock>
         </StackPanel>
     
                <!-- Status Badge -->
     <Border Grid.Column="2"
         Background="{Binding Status, Converter={StaticResource StatusToBrushConverter}}"
        CornerRadius="12"
             Padding="12,4"
     VerticalAlignment="Center">
        <TextBlock Text="{Binding StatusText}"
     Foreground="White"
  FontSize="12"
        FontWeight="SemiBold"/>
        </Border>
            </Grid>
            
    <!-- Original Filename -->
            <Border Grid.Row="1" 
 Background="#FAFAFA"
            CornerRadius="4"
   Padding="12,8"
                    Margin="20,0,0,4">
        <StackPanel>
        <TextBlock Text="Original:"
           Style="{StaticResource CardMetadata}"
    Margin="0,0,0,4"/>
   <TextBlock Text="{Binding SourcePath}"
      Style="{StaticResource CardSubtitle}"
         TextWrapping="NoWrap"
          TextTrimming="CharacterEllipsis"
   ToolTip="{Binding SourcePath}"/>
    </StackPanel>
            </Border>
            
            <!-- New Filename -->
            <Border Grid.Row="2" 
        Background="#E8F5E9"
         CornerRadius="4"
               Padding="12,8"
  Margin="20,0,0,0">
    <StackPanel>
  <TextBlock Text="New:"
     Style="{StaticResource CardMetadata}"
       Margin="0,0,0,4"/>
        <TextBlock Text="{Binding NewFileName}"
 Style="{StaticResource CardTitle}"
          FontSize="16"
      Foreground="#2E7D32"
         TextWrapping="NoWrap"
        TextTrimming="CharacterEllipsis"
  ToolTip="{Binding NewFileName}"/>
          </StackPanel>
          </Border>
            
          <!-- Paths -->
       <StackPanel Grid.Row="4" Margin="20,0,0,0">
     <TextBlock Style="{StaticResource CardSubtitle}">
  <Run Text="??" FontSize="14"/>
            <Run Text="Source: "/>
       <Run Text="{Binding SourcePath}" Foreground="#757575"/>
     </TextBlock>
     <TextBlock Style="{StaticResource CardSubtitle}" Margin="0,4,0,0">
      <Run Text="??" FontSize="14"/>
           <Run Text="Dest: "/>
    <Run Text="{Binding DestinationPath}" Foreground="#757575"/>
        </TextBlock>
            </StackPanel>
  
       <!-- Metadata -->
            <StackPanel Grid.Row="6" 
    Orientation="Horizontal"
           Margin="20,0,0,0">
 <TextBlock Style="{StaticResource CardMetadata}">
   <Run Text="?"/>
       <Run Text="{Binding PlexValidation}"/>
          </TextBlock>
         <TextBlock Style="{StaticResource CardMetadata}"
          Margin="16,0,0,0"
      Text="{Binding Extension}"/>
          </StackPanel>
            
            <!-- Error Message (if any) -->
    <Border Grid.Row="7"
        Visibility="{Binding ErrorMessage, Converter={StaticResource StringToVisibilityConverter}}"
   Background="#FFEBEE"
     CornerRadius="4"
         Padding="12,8"
    Margin="20,8,0,0">
        <TextBlock Style="{StaticResource CardSubtitle}"
  Foreground="#C62828">
           <Run Text="? "/>
   <Run Text="Error: "/>
    <Run Text="{Binding ErrorMessage}"/>
           </TextBlock>
     </Border>
  
            <!-- Action Buttons -->
<StackPanel Grid.Row="8" 
       Orientation="Horizontal"
     HorizontalAlignment="Right"
    Margin="20,12,0,0">
    <Button Content="View Details"
     Style="{StaticResource SecondaryButton}"
     Padding="12,6"
 Margin="0,0,8,0"/>
    <Button Content="Open Folder"
    Style="{StaticResource SecondaryButton}"
            Padding="12,6"
      Margin="0,0,8,0"/>
           <Button Content="Remove"
     Style="{StaticResource SecondaryButton}"
Padding="12,6"
    Margin="0,0,8,0"/>
     <Button Content="? Retry"
     Style="{StaticResource PrimaryButton}"
        Padding="12,6"
            Visibility="{Binding Status, Converter={StaticResource FailedStatusToVisibilityConverter}}"/>
            </StackPanel>
        </Grid>
    </Border>
</UserControl>
```

#### 4.4 Update TvEpisodesView with Card Layout

**File: TorrentFileRenamer.WPF\Views\TvEpisodesView.xaml (Card Version)**

```xml
<UserControl x:Class="TorrentFileRenamer.WPF.Views.TvEpisodesView"
           xmlns:controls="clr-namespace:TorrentFileRenamer.WPF.Views.Controls">
    
    <Grid>
        <Grid.RowDefinitions>
       <RowDefinition Height="Auto"/> <!-- Toolbar -->
        <RowDefinition Height="Auto"/> <!-- View Selector -->
         <RowDefinition Height="*"/>    <!-- Content -->
            <RowDefinition Height="Auto"/> <!-- Status Bar -->
        </Grid.RowDefinitions>
        
    <!-- Toolbar (existing) -->
        <ToolBar Grid.Row="0" Style="{StaticResource ModernToolBar}">
 <!-- existing buttons -->
        </ToolBar>
   
        <!-- View Selector -->
        <Border Grid.Row="1" 
       Background="#FAFAFA"
  BorderBrush="#E0E0E0"
                BorderThickness="0,0,0,1"
       Padding="16,8">
 <StackPanel Orientation="Horizontal">
     <TextBlock Text="View:" 
   VerticalAlignment="Center"
             Margin="0,0,12,0"
         Foreground="#757575"/>
          <RadioButton Content="Cards"
      IsChecked="{Binding IsCardViewSelected}"
   Style="{StaticResource ViewToggleButton}"
            Margin="0,0,8,0"/>
                <RadioButton Content="Compact"
             IsChecked="{Binding IsCompactViewSelected}"
      Style="{StaticResource ViewToggleButton}"
              Margin="0,0,8,0"/>
       <RadioButton Content="Grid"
          IsChecked="{Binding IsGridViewSelected}"
         Style="{StaticResource ViewToggleButton}"/>
    
           <!-- Search Box -->
      <TextBox Width="200"
             Margin="32,0,0,0"
        Style="{StaticResource ModernTextBox}"
  Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
  Tag="Search episodes..."/>
            </StackPanel>
        </Border>
        
        <!-- Card View -->
        <ScrollViewer Grid.Row="2" 
          Visibility="{Binding IsCardViewSelected, Converter={StaticResource BoolToVisibilityConverter}}"
     VerticalScrollBarVisibility="Auto"
      Padding="16,8">
        <ItemsControl ItemsSource="{Binding Episodes}">
  <ItemsControl.ItemTemplate>
            <DataTemplate>
             <controls:FileEpisodeCard DataContext="{Binding}"/>
         </DataTemplate>
      </ItemsControl.ItemTemplate>
             
     <!-- Staggered Animation -->
     <ItemsControl.ItemContainerStyle>
<Style>
             <Setter Property="FrameworkElement.Opacity" Value="0"/>
      <Style.Triggers>
   <EventTrigger RoutedEvent="FrameworkElement.Loaded">
    <BeginStoryboard>
        <Storyboard>
          <DoubleAnimation Storyboard.TargetProperty="Opacity"
       To="1" Duration="0:0:0.3"/>
               <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.Y)"
     From="20" To="0" Duration="0:0:0.3"/>
            </Storyboard>
             </BeginStoryboard>
  </EventTrigger>
       </Style.Triggers>
   </Style>
      </ItemsControl.ItemContainerStyle>
            </ItemsControl>
</ScrollViewer>
        
    <!-- Grid View (existing DataGrid) -->
   <DataGrid Grid.Row="2"
     Visibility="{Binding IsGridViewSelected, Converter={StaticResource BoolToVisibilityConverter}}"
   ItemsSource="{Binding Episodes}">
   <!-- existing DataGrid implementation -->
      </DataGrid>
 
        <!-- Status Bar -->
      <StatusBar Grid.Row="3" Style="{StaticResource ModernStatusBar}">
            <!-- existing status bar -->
        </StatusBar>
    </Grid>
</UserControl>
```

### Phase 3: Movies Card View (Week 3)

#### 4.5 MovieFileCard Component

Similar to FileEpisodeCard but with movie-specific features:
- Larger confidence indicator with color coding
- Movie poster thumbnail (if available)
- Year display
- Quality tag detection display

**Key Differences:**
```xml
<!-- Confidence Indicator -->
<Border Background="{Binding ConfidenceLevel, Converter={StaticResource ConfidenceToBrushConverter}}"
        CornerRadius="4"
  Padding="8,4">
    <StackPanel Orientation="Horizontal">
      <TextBlock Text="?" FontSize="14" Margin="0,0,4,0"/>
 <TextBlock Text="{Binding ConfidenceLevel}" 
Foreground="White"
       FontWeight="SemiBold"/>
        <TextBlock Text="{Binding Confidence, StringFormat=' ({0}%)'}"
            Foreground="White"
 FontSize="11"
         Margin="4,0,0,0"/>
    </StackPanel>
</Border>
```

### Phase 4: Advanced Features (Week 4)

#### 4.6 Filtering and Grouping

**Status Filter:**
```xml
<ComboBox SelectedItem="{Binding SelectedStatusFilter}"
          ItemsSource="{Binding StatusFilters}"
      Style="{StaticResource ModernComboBox}">
    <ComboBoxItem Content="All Items" />
<ComboBoxItem Content="Pending Only"/>
    <ComboBoxItem Content="Completed Only"/>
    <ComboBoxItem Content="Failed Only"/>
    <ComboBoxItem Content="Unparsed Only"/>
</ComboBox>
```

**Grouping by Show/Status:**
```xml
<ItemsControl ItemsSource="{Binding GroupedEpisodes}">
    <ItemsControl.GroupStyle>
     <GroupStyle>
   <GroupStyle.HeaderTemplate>
          <DataTemplate>
        <Border Background="#FAFAFA"
   Padding="16,8"
           Margin="0,8,0,4">
               <TextBlock Text="{Binding Name}"
        Style="{StaticResource HeadingTextBlock}"
          FontSize="16"/>
      </Border>
          </DataTemplate>
  </GroupStyle.HeaderTemplate>
      </GroupStyle>
    </ItemsControl.GroupStyle>
</ItemsControl>
```

#### 4.7 Expandable Card Details

Add expand/collapse functionality:
```xml
<Expander Header="View Full Details" 
          Margin="20,8,0,0"
          IsExpanded="{Binding IsExpanded}">
    <StackPanel Padding="12">
        <TextBlock Text="Full Source Path:"/>
        <TextBox Text="{Binding SourcePath}" 
      IsReadOnly="True"
         Style="{StaticResource MonospaceTextBox}"/>
<TextBlock Text="Full Destination Path:" Margin="0,8,0,0"/>
        <TextBox Text="{Binding DestinationPath}" 
                IsReadOnly="True"
                Style="{StaticResource MonospaceTextBox}"/>
      <!-- Additional metadata -->
    </StackPanel>
</Expander>
```

#### 4.8 Drag-and-Drop Visual Feedback

Enhance cards with drop zones:
```xml
<Border x:Name="DropOverlay"
        Grid.RowSpan="9"
     Background="#4D2196F3"
        CornerRadius="8"
        Visibility="Collapsed">
    <TextBlock Text="Drop files here"
  FontSize="20"
              FontWeight="Bold"
       Foreground="White"
    HorizontalAlignment="Center"
            VerticalAlignment="Center"/>
</Border>
```

### Phase 5: Performance Optimization (Week 5)

#### 4.9 Virtualization

Implement `VirtualizingStackPanel` for large lists:
```xml
<ItemsControl ItemsSource="{Binding Episodes}">
    <ItemsControl.ItemsPanel>
 <ItemsPanelTemplate>
       <VirtualizingStackPanel VirtualizationMode="Recycling"/>
 </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>
```

#### 4.10 Lazy Loading

Load cards in batches:
```csharp
public class TvEpisodesViewModel
{
    private const int PageSize = 20;
    
    public async Task LoadMoreItemsAsync()
    {
        var items = await LoadNextPageAsync();
        foreach (var item in items)
  {
            Episodes.Add(item);
        }
    }
}
```

---

## 5. Technical Specifications

### 5.1 ViewModel Enhancements

**Add View Mode Properties:**
```csharp
public class TvEpisodesViewModel : ViewModelBase
{
    private bool _isCardViewSelected = true;
    private bool _isCompactViewSelected;
    private bool _isGridViewSelected;
    private string _searchText = string.Empty;
    private ProcessingStatus? _statusFilter;
    
    public bool IsCardViewSelected
    {
    get => _isCardViewSelected;
  set
  {
       if (SetProperty(ref _isCardViewSelected, value) && value)
      {
             IsCompactViewSelected = false;
    IsGridViewSelected = false;
  SaveViewPreference("Card");
     }
      }
    }
    
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
    
    private void ApplyFilters()
    {
        // Implement filtering logic
        var filtered = AllEpisodes.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(e => 
      e.ShowName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
       e.NewFileName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
 }
        
        if (StatusFilter.HasValue)
        {
     filtered = filtered.Where(e => e.Status == StatusFilter.Value);
      }
 
    Episodes = new ObservableCollection<FileEpisodeModel>(filtered);
    }
}
```

### 5.2 Custom Controls

**ViewToggleButton Style:**
```xml
<Style x:Key="ViewToggleButton" TargetType="RadioButton">
    <Setter Property="Template">
        <Setter.Value>
          <ControlTemplate TargetType="RadioButton">
           <Border Background="{TemplateBinding Background}"
          BorderBrush="#E0E0E0"
   BorderThickness="1"
       CornerRadius="4"
                Padding="12,6">
          <ContentPresenter/>
           </Border>
    </ControlTemplate>
        </Setter.Value>
    </Setter>
    <Style.Triggers>
      <Trigger Property="IsChecked" Value="True">
 <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
            <Setter Property="Foreground" Value="White"/>
  </Trigger>
    </Style.Triggers>
</Style>
```

### 5.3 Animation Resources

**File: TorrentFileRenamer.WPF\Resources\Animations.xaml**
```xml
<ResourceDictionary>
  <!-- Card Entrance Animation -->
    <Storyboard x:Key="CardEntranceAnimation">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
              From="0" To="1" Duration="0:0:0.2"/>
     <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.Y)"
 From="20" To="0" Duration="0:0:0.2">
   <DoubleAnimation.EasingFunction>
<CubicEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
    
    <!-- Processing Pulse Animation -->
    <Storyboard x:Key="ProcessingPulseAnimation" RepeatBehavior="Forever">
 <DoubleAnimation Storyboard.TargetProperty="Opacity"
          From="1" To="0.5" Duration="0:0:0.8"
   AutoReverse="True"/>
    </Storyboard>
    
    <!-- Card Hover Animation -->
    <Storyboard x:Key="CardHoverAnimation">
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.Effect).(DropShadowEffect.BlurRadius)"
        To="12" Duration="0:0:0.1"/>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.Effect).(DropShadowEffect.ShadowDepth)"
          To="4" Duration="0:0:0.1"/>
    </Storyboard>
</ResourceDictionary>
```

---

## 6. Migration Strategy

### 6.1 Gradual Rollout

**Phase Approach:**
1. **Coexistence**: Both views available via toggle
2. **Default**: Card view becomes default, grid view optional
3. **User Preference**: Save user's choice in settings
4. **Eventual**: Consider removing grid view if unused

### 6.2 Backward Compatibility

- Keep existing DataGrid implementation
- Add view toggle in toolbar
- Save view preference in AppSettings
- Maintain keyboard shortcuts for both views

### 6.3 User Education

**First Launch:**
```xml
<Border Background="#E3F2FD" 
   BorderBrush="#2196F3"
        BorderThickness="1"
   CornerRadius="4"
        Padding="16"
        Margin="16,8">
    <StackPanel>
        <TextBlock Text="? New Card View Available!"
         FontWeight="Bold"
       FontSize="16"
      Foreground="#1976D2"/>
 <TextBlock Text="Try our new card-based interface for better visibility. You can switch back to the grid view anytime."
         TextWrapping="Wrap"
  Margin="0,8,0,0"/>
        <Button Content="Got it!" 
            Style="{StaticResource PrimaryButton}"
         HorizontalAlignment="Right"
    Margin="0,8,0,0"/>
    </StackPanel>
</Border>
```

---

## 7. User Experience Enhancements

### 7.1 Improved Workflows

**Quick Actions:**
- Right-click card for context menu
- Hover for inline action buttons
- Double-click to expand details
- Drag card to reorder (future)

**Batch Operations:**
- Select multiple cards (checkbox overlay)
- Bulk actions in toolbar
- Status filter for targeted operations

**Visual Feedback:**
- Processing animation on active cards
- Success/failure toast notifications
- Progress bar per card during processing

### 7.2 Accessibility

**ARIA Labels:**
```xml
<Border AutomationProperties.Name="{Binding ShowName}"
        AutomationProperties.HelpText="{Binding StatusText}">
```

**Keyboard Navigation:**
- Tab between cards
- Arrow keys to navigate
- Enter to expand details
- Space to select

**High Contrast:**
- Respect system high contrast mode
- Increase border thickness
- Use pattern fills instead of colors

### 7.3 Responsive Design

**Adaptive Layout:**
```xml
<Grid>
    <Grid.Style>
        <Style TargetType="Grid">
            <Style.Triggers>
 <!-- Wide Screen: 2 columns -->
                <DataTrigger Binding="{Binding ActualWidth, RelativeSource={RelativeSource AncestorType=Window}}"
               Value="1400">
   <Setter Property="Grid.ColumnDefinitions" Value="*,*"/>
     </DataTrigger>
      <!-- Normal: 1 column -->
    </Style.Triggers>
        </Style>
    </Grid.Style>
</Grid>
```

---

## 8. Performance Considerations

### 8.1 Rendering Optimization

**Techniques:**
- Use `VirtualizingStackPanel` for large lists
- Lazy load card content
- Recycle card containers
- Defer image loading (if adding thumbnails)

**Expected Performance:**
```
Items       Grid Load Time    Card Load Time    Memory Usage
100   ~50ms     ~120ms     +5 MB
500         ~150ms    ~300ms           +15 MB
1000        ~300ms           ~600ms           +30 MB
```

### 8.2 Memory Management

**Card Recycling:**
```csharp
<ItemsControl VirtualizingPanel.IsVirtualizing="True"
     VirtualizingPanel.VirtualizationMode="Recycling"
              VirtualizingPanel.CacheLength="2,2"
  VirtualizingPanel.CacheLengthUnit="Page">
```

**Image Caching (Future):**
```csharp
<Image Source="{Binding ThumbnailUrl}"
       DecodePixelWidth="100"
       CacheOption="OnLoad"/>
```

### 8.3 Benchmarking

**Metrics to Track:**
- Initial render time
- Scroll performance (FPS)
- Memory usage growth
- CPU usage during animations
- Time to interact (TTI)

---

## 9. Testing Strategy

### 9.1 Visual Testing

**Test Cases:**
- [ ] Cards display correctly in all statuses
- [ ] Animations smooth (60 FPS)
- [ ] Responsive to window resize
- [ ] Colors match design system
- [ ] Icons render correctly
- [ ] Text truncation works
- [ ] Hover effects smooth

### 9.2 Functional Testing

**Test Cases:**
- [ ] View toggle switches correctly
- [ ] Search filters work
- [ ] Status filter works
- [ ] Selection works
- [ ] Context menu appears
- [ ] Action buttons functional
- [ ] Keyboard navigation works
- [ ] Drag-and-drop works

### 9.3 Performance Testing

**Test Cases:**
- [ ] 100 items render in < 200ms
- [ ] 1000 items scrollable at 60 FPS
- [ ] Memory stays under 100 MB increase
- [ ] Animations don't drop frames
- [ ] Search filters instantly (< 50ms)

### 9.4 Accessibility Testing

**Test Cases:**
- [ ] Screen reader announces cards
- [ ] Keyboard navigation complete
- [ ] High contrast mode works
- [ ] Focus indicators visible
- [ ] Alt text on icons

---

## 10. Timeline & Resources

### 10.1 Development Timeline

```
Week 1: Foundation
??? Day 1-2: Create card styles and animations
??? Day 3-4: Build base card components
??? Day 5: Testing and refinement

Week 2: TV Episodes Implementation
??? Day 1-2: FileEpisodeCard complete
??? Day 3-4: Integrate into TvEpisodesView
??? Day 5: Testing

Week 3: Movies Implementation
??? Day 1-2: MovieFileCard complete
??? Day 3-4: Integrate into MoviesView
??? Day 5: Testing

Week 4: Advanced Features
??? Day 1-2: Filtering and search
??? Day 3-4: Grouping and sorting
??? Day 5: Polish and refinement

Week 5: Optimization & Testing
??? Day 1-2: Performance optimization
??? Day 3-4: Comprehensive testing
??? Day 5: Bug fixes and deployment

Total: 5 weeks (25 working days)
```

### 10.2 Resource Requirements

**Developer Skills:**
- ? WPF/XAML expertise
- ? C# and MVVM pattern
- ? UI/UX design principles
- ? Animation and performance optimization

**Design Resources:**
- Icon library (Unicode or custom)
- Color palette (Material Design)
- Typography scale
- Sample data for testing

**Testing Resources:**
- Various screen sizes (1920x1080, 1366x768, 2560x1440)
- Different Windows themes
- Screen reader software
- Performance profiling tools

---

## 11. Success Criteria

### 11.1 User Acceptance

- ? 80%+ users prefer card view
- ? Reduction in "hard to find information" complaints
- ? Faster task completion (measured)
- ? Positive feedback on visual design

### 11.2 Performance Metrics

- ? Card view renders in < 300ms for 500 items
- ? Smooth scrolling at 60 FPS
- ? Memory usage increase < 50 MB
- ? No UI freezing during operations

### 11.3 Accessibility

- ? WCAG 2.1 AA compliance
- ? Full keyboard navigation
- ? Screen reader compatible
- ? High contrast mode support

---

## 12. Future Enhancements

### 12.1 Phase 2 Features (After Initial Release)

**Thumbnail Previews:**
- Movie posters from TMDB API
- TV show banners
- Video file thumbnails

**Advanced Grouping:**
- Group by show/movie
- Group by status
- Group by date added
- Custom grouping rules

**Customization:**
- Card color themes
- Layout density options
- Show/hide metadata fields
- Custom card templates

**Analytics:**
- Processing history visualization
- Success rate charts
- Performance metrics
- Storage usage graphs

### 12.2 Mobile/Touch Optimization

**Touch Gestures:**
- Swipe to remove
- Pull to refresh
- Pinch to zoom
- Long press for menu

**Responsive Cards:**
- Smaller cards on narrow screens
- Stack layout on tablets
- Touch-friendly buttons (44x44px minimum)

---

## 13. Comparison: Grid vs. Card View

### 13.1 Advantages of Card View

| Aspect | Grid View | Card View |
|--------|-----------|-----------|
| **Information Hierarchy** | Flat, all equal | Clear hierarchy, emphasis on key info |
| **Scannability** | Good for data tables | Excellent for content browsing |
| **Status Visibility** | Background color only | Multiple indicators (icon, badge, border) |
| **Error Visibility** | Right column, easily missed | Prominent error section |
| **Mobile-Friendly** | Poor on small screens | Adapts well to any screen size |
| **Visual Appeal** | Utilitarian | Modern and engaging |
| **Metadata Display** | Limited space | Ample space for additional info |
| **Actions Access** | Context menu only | Inline buttons + context menu |
| **Progressive Disclosure** | All or nothing | Expandable details |
| **Custom Layouts** | Rigid columns | Flexible card design |

### 13.2 When to Use Each View

**Use Card View When:**
- Browsing and discovering files
- Quickly identifying status
- Working with errors that need attention
- Casual users or infrequent use
- Modern, visually appealing UI desired

**Use Grid View When:**
- Need to compare multiple attributes
- Sorting by different columns frequently
- Power users familiar with data tables
- Copying data from cells
- Working with very large datasets (5000+ items)

---

## 14. Implementation Checklist

### 14.1 Phase 1 Deliverables
- [ ] CardStyles.xaml created
- [ ] Animations.xaml created
- [ ] StatusToBrushConverter implemented
- [ ] StatusToIconConverter implemented
- [ ] FileSizeConverter implemented
- [ ] DateTimeToRelativeConverter implemented
- [ ] Base card template designed
- [ ] View toggle UI designed

### 14.2 Phase 2 Deliverables
- [ ] FileEpisodeCard.xaml created
- [ ] FileEpisodeCard.xaml.cs created
- [ ] Card displays all episode information
- [ ] Status indicators working
- [ ] Action buttons functional
- [ ] TvEpisodesView updated with card view option
- [ ] View toggle working
- [ ] Search box functional
- [ ] Performance acceptable (< 300ms for 500 items)

### 14.3 Phase 3 Deliverables
- [ ] MovieFileCard.xaml created
- [ ] MovieFileCard.xaml.cs created
- [ ] Confidence indicator working
- [ ] Card displays all movie information
- [ ] MoviesView updated with card view option
- [ ] All features from TV view working

### 14.4 Phase 4 Deliverables
- [ ] Status filtering working
- [ ] Search filtering working
- [ ] Grouping implemented
- [ ] Expandable details working
- [ ] Drag-and-drop visual feedback
- [ ] Animation polish complete

### 14.5 Phase 5 Deliverables
- [ ] Virtualization implemented
- [ ] Performance optimized
- [ ] Memory usage acceptable
- [ ] All tests passing
- [ ] Accessibility features complete
- [ ] Documentation updated

---

## 15. Risk Assessment

### 15.1 Technical Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Performance degradation with large lists | High | Medium | Implement virtualization early |
| Animation jank on lower-end systems | Medium | Medium | Make animations optional |
| Increased memory usage | Medium | High | Profile and optimize, implement recycling |
| Custom control complexity | Medium | Low | Keep controls simple, reuse templates |

### 15.2 User Experience Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Users prefer grid view | Medium | Low | Keep both views available |
| Information overload in cards | Low | Medium | Test with users, refine layout |
| Discoverability of view toggle | Medium | Medium | Add tooltip, first-run notification |
| Learning curve for new UI | Low | Medium | Provide transition guide, tooltips |

---

## 16. Conclusion

The proposed card-based UI modernization will significantly improve the TorrentFileRenamer user experience by:

1. **Improving Visual Hierarchy** - Important information stands out
2. **Enhancing Status Visibility** - Multiple status indicators
3. **Reducing Cognitive Load** - Information grouped logically
4. **Modernizing Aesthetics** - Contemporary, polished appearance
5. **Increasing Flexibility** - Multiple view modes for different needs
6. **Supporting Future Features** - Room for thumbnails, more metadata

The phased implementation approach allows for gradual rollout with user feedback at each stage. By maintaining the grid view as an option, we minimize risk and respect user preferences while pushing forward with a modern, visually appealing design.

**Estimated ROI:**
- Development: 5 weeks (200 hours)
- User satisfaction: +40% (projected)
- Task completion time: -25% (projected)
- Support requests: -20% (projected)

---

## Appendix A: Design Mockups

[Visual mockups would be included here showing:]
- Card view with all status types
- Compact view layout
- Grid view (existing)
- View toggle UI
- Filtering UI
- Expanded card details
- Mobile responsive layout

---

## Appendix B: Code Samples

See implementation sections above for detailed code samples including:
- Card XAML templates
- ViewModel enhancements
- Converter implementations
- Animation resources
- Style definitions

---

## Appendix C: User Research

**Survey Questions for Beta Testing:**
1. Which view do you prefer: Card, Compact, or Grid?
2. How easy is it to identify file status?
3. How quickly can you find errors?
4. What additional information would you like on cards?
5. Are the animations helpful or distracting?
6. Would you use thumbnail previews if available?

---

**Document Version**: 1.0  
**Created**: January 2025  
**Status**: Proposal - Awaiting Approval  
**Next Review**: After Phase 1 prototype
