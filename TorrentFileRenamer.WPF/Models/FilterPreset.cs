using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// Represents a saved filter preset
/// </summary>
public class FilterPreset : ObservableObject
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private SearchCriteria _criteria = new();
    private DateTime _createdDate = DateTime.Now;
    private DateTime _lastUsedDate = DateTime.Now;
    private bool _isPredefined;

    /// <summary>
    /// Preset name
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Preset description
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// The search criteria for this preset
    /// </summary>
    public SearchCriteria Criteria
    {
        get => _criteria;
        set => SetProperty(ref _criteria, value);
    }

    /// <summary>
    /// When this preset was created
    /// </summary>
    public DateTime CreatedDate
    {
        get => _createdDate;
        set => SetProperty(ref _createdDate, value);
    }

    /// <summary>
    /// When this preset was last used
    /// </summary>
    public DateTime LastUsedDate
    {
        get => _lastUsedDate;
        set => SetProperty(ref _lastUsedDate, value);
    }

    /// <summary>
    /// Whether this is a predefined (built-in) preset
    /// </summary>
    public bool IsPredefined
    {
        get => _isPredefined;
        set => SetProperty(ref _isPredefined, value);
    }

    /// <summary>
    /// Creates predefined filter presets
    /// </summary>
    public static List<FilterPreset> GetPredefinedPresets()
    {
        return new List<FilterPreset>
        {
            new FilterPreset
            {
                Name = "High Confidence Only",
                Description = "Show only files with 70%+ confidence",
                IsPredefined = true,
                Criteria = new SearchCriteria
                {
                    MinConfidence = 70,
                    MaxConfidence = 100
                }
            },
            new FilterPreset
            {
                Name = "Needs Review",
                Description = "Files with low confidence or errors",
                IsPredefined = true,
                Criteria = new SearchCriteria
                {
                    MaxConfidence = 40,
                    SelectedStatuses = new List<ProcessingStatus>
                    {
                        ProcessingStatus.Failed,
                        ProcessingStatus.Unparsed
                    }
                }
            },
            new FilterPreset
            {
                Name = "Large Files (>1GB)",
                Description = "Show only files larger than 1GB",
                IsPredefined = true,
                Criteria = new SearchCriteria
                {
                    MinFileSize = 1024L * 1024L * 1024L // 1GB in bytes
                }
            },
            new FilterPreset
            {
                Name = "MKV Files",
                Description = "Show only MKV format files",
                IsPredefined = true,
                Criteria = new SearchCriteria
                {
                    SelectedExtensions = new List<string> { ".mkv" }
                }
            },
            new FilterPreset
            {
                Name = "Processed Successfully",
                Description = "Files that completed successfully",
                IsPredefined = true,
                Criteria = new SearchCriteria
                {
                    SelectedStatuses = new List<ProcessingStatus> { ProcessingStatus.Completed }
                }
            }
        };
    }
}