using System.Collections.ObjectModel;
using System.Windows.Input;
using TorrentFileRenamer.WPF.Models;
using TorrentFileRenamer.WPF.Services;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for advanced filtering
/// </summary>
public class FilterViewModel : ViewModelBase
{
    private readonly ISearchService _searchService;
    private readonly IDialogService _dialogService;
    private SearchCriteria _currentCriteria = new();
    private bool _isPanelVisible;
    private ObservableCollection<FilterPreset> _presets = new();
    private FilterPreset? _selectedPreset;
    private ObservableCollection<string> _availableExtensions = new();
    private ObservableCollection<string> _selectedExtensions = new();

    public FilterViewModel(ISearchService searchService, IDialogService dialogService)
    {
_searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Load predefined presets
        foreach (var preset in FilterPreset.GetPredefinedPresets())
        {
         Presets.Add(preset);
        }

        // Commands
     ApplyFilterCommand = new RelayCommand(ExecuteApplyFilter);
     ResetFilterCommand = new RelayCommand(ExecuteResetFilter, _ => CurrentCriteria.HasActiveFilters);
    TogglePanelVisibilityCommand = new RelayCommand(_ => IsPanelVisible = !IsPanelVisible);
   SavePresetCommand = new RelayCommand(ExecuteSavePreset, _ => CurrentCriteria.HasActiveFilters);
    DeletePresetCommand = new RelayCommand(ExecuteDeletePreset, _ => SelectedPreset != null && !SelectedPreset.IsPredefined);
    ApplyPresetCommand = new RelayCommand(ExecuteApplyPreset);
    }

    #region Properties

    /// <summary>
 /// Current search/filter criteria
    /// </summary>
    public SearchCriteria CurrentCriteria
    {
        get => _currentCriteria;
        set
        {
        if (SetProperty(ref _currentCriteria, value))
   {
                OnPropertyChanged(nameof(HasActiveFilters));
      OnPropertyChanged(nameof(ActiveFilterCount));
     ((RelayCommand)ResetFilterCommand).RaiseCanExecuteChanged();
       ((RelayCommand)SavePresetCommand).RaiseCanExecuteChanged();
         }
    }
    }

    /// <summary>
    /// Whether the filter panel is visible
    /// </summary>
    public bool IsPanelVisible
    {
        get => _isPanelVisible;
      set => SetProperty(ref _isPanelVisible, value);
    }

 /// <summary>
    /// Available filter presets
    /// </summary>
    public ObservableCollection<FilterPreset> Presets
    {
     get => _presets;
        set => SetProperty(ref _presets, value);
    }

    /// <summary>
    /// Selected preset
  /// </summary>
    public FilterPreset? SelectedPreset
    {
    get => _selectedPreset;
        set
    {
            if (SetProperty(ref _selectedPreset, value))
 {
          ((RelayCommand)DeletePresetCommand).RaiseCanExecuteChanged();
     }
     }
    }

    /// <summary>
    /// Available file extensions
    /// </summary>
    public ObservableCollection<string> AvailableExtensions
    {
        get => _availableExtensions;
        set => SetProperty(ref _availableExtensions, value);
    }

    /// <summary>
    /// Selected file extensions for filtering
    /// </summary>
    public ObservableCollection<string> SelectedExtensions
    {
        get => _selectedExtensions;
    set => SetProperty(ref _selectedExtensions, value);
}

    /// <summary>
    /// Whether any filters are active
    /// </summary>
    public bool HasActiveFilters => CurrentCriteria.HasActiveFilters;

    /// <summary>
    /// Number of active filters
    /// </summary>
    public int ActiveFilterCount
    {
        get
        {
    int count = 0;
            if (!string.IsNullOrWhiteSpace(CurrentCriteria.SearchText)) count++;
      if (CurrentCriteria.MinConfidence > 0 || CurrentCriteria.MaxConfidence < 100) count++;
        if (CurrentCriteria.MinFileSize > 0 || CurrentCriteria.MaxFileSize < long.MaxValue) count++;
      if (CurrentCriteria.DateAddedFrom.HasValue || CurrentCriteria.DateAddedTo.HasValue) count++;
            if (CurrentCriteria.SelectedExtensions.Count > 0) count++;
     if (CurrentCriteria.SelectedStatuses.Count > 0) count++;
    return count;
   }
    }

    #endregion

    #region Commands

    public ICommand ApplyFilterCommand { get; }
    public ICommand ResetFilterCommand { get; }
    public ICommand TogglePanelVisibilityCommand { get; }
    public ICommand SavePresetCommand { get; }
    public ICommand DeletePresetCommand { get; }
    public ICommand ApplyPresetCommand { get; }

    #endregion

    #region Events

    /// <summary>
    /// Raised when filters are applied
    /// </summary>
    public event EventHandler? FiltersApplied;

    /// <summary>
    /// Raised when filters are reset
    /// </summary>
    public event EventHandler? FiltersReset;

    #endregion

    #region Command Implementations

private void ExecuteApplyFilter(object? parameter)
    {
        // Sync selected extensions to criteria
   CurrentCriteria.SelectedExtensions = SelectedExtensions.ToList();
      
        FiltersApplied?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(HasActiveFilters));
      OnPropertyChanged(nameof(ActiveFilterCount));
    }

    private void ExecuteResetFilter(object? parameter)
    {
        CurrentCriteria.Reset();
        SelectedExtensions.Clear();
  
 FiltersReset?.Invoke(this, EventArgs.Empty);
     OnPropertyChanged(nameof(HasActiveFilters));
        OnPropertyChanged(nameof(ActiveFilterCount));
   
  ((RelayCommand)ResetFilterCommand).RaiseCanExecuteChanged();
      ((RelayCommand)SavePresetCommand).RaiseCanExecuteChanged();
    }

    private void ExecuteSavePreset(object? parameter)
    {
        // TODO: Show dialog to get preset name and description
        var presetName = $"Custom Filter {DateTime.Now:yyyy-MM-dd HH:mm}";
        
    var preset = new FilterPreset
        {
            Name = presetName,
         Description = "Custom filter preset",
     Criteria = CurrentCriteria.Clone(),
            IsPredefined = false
        };
        
   Presets.Add(preset);
    _dialogService.ShowMessage("Filter Preset Saved", $"Preset '{presetName}' has been saved.");
    }

    private void ExecuteDeletePreset(object? parameter)
    {
        if (SelectedPreset == null || SelectedPreset.IsPredefined)
       return;
    
        if (_dialogService.ShowConfirmation("Delete Preset", 
  $"Are you sure you want to delete the preset '{SelectedPreset.Name}'?"))
 {
  Presets.Remove(SelectedPreset);
SelectedPreset = null;
        }
    }

    private void ExecuteApplyPreset(object? parameter)
    {
    if (parameter is FilterPreset preset)
        {
      CurrentCriteria = preset.Criteria.Clone();
   
        // Update selected extensions
    SelectedExtensions.Clear();
   foreach (var ext in CurrentCriteria.SelectedExtensions)
 {
     SelectedExtensions.Add(ext);
     }
   
        // Update last used date
     preset.LastUsedDate = DateTime.Now;
      
        ExecuteApplyFilter(null);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
/// Updates available extensions based on current files
/// </summary>
    public void UpdateAvailableExtensions(List<string> extensions)
    {
        AvailableExtensions.Clear();
        foreach (var ext in extensions.OrderBy(e => e))
   {
            AvailableExtensions.Add(ext);
        }
    }

    /// <summary>
  /// Gets the current filter criteria
    /// </summary>
    public SearchCriteria GetCurrentCriteria()
    {
        CurrentCriteria.SelectedExtensions = SelectedExtensions.ToList();
        return CurrentCriteria;
    }

    /// <summary>
    /// Applies search text from external source (e.g., SearchViewModel)
    /// </summary>
    public void ApplySearchText(string searchText)
    {
        CurrentCriteria.SearchText = searchText;
  ExecuteApplyFilter(null);
    }

    #endregion
}
