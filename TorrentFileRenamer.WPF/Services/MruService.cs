using TorrentFileRenamer.Core.Configuration;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Service for managing Most Recently Used (MRU) lists
/// </summary>
public class MruService : IMruService
{
    private readonly AppSettings _appSettings;
    private const int MaxMruItems = 20;

    public MruService(AppSettings appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    /// <summary>
    /// Adds a path to the MRU list
    /// </summary>
    public void AddRecentPath(string category, string path)
    {
   if (string.IsNullOrWhiteSpace(category)) 
 throw new ArgumentException("Category cannot be empty", nameof(category));
  if (string.IsNullOrWhiteSpace(path)) 
          throw new ArgumentException("Path cannot be empty", nameof(path));

        // Normalize path
   path = System.IO.Path.GetFullPath(path);

   // Get or create the MRU list for this category
   if (!_appSettings.MruLists.TryGetValue(category, out var mruList))
 {
  mruList = new List<string>();
       _appSettings.MruLists[category] = mruList;
 }

  // Remove if already exists (we'll add it to the front)
 mruList.Remove(path);

  // Add to front
mruList.Insert(0, path);

     // Limit size
 if (mruList.Count > MaxMruItems)
    {
            mruList.RemoveRange(MaxMruItems, mruList.Count - MaxMruItems);
}

        _appSettings.Save();
    }

    /// <summary>
  /// Gets the recent paths for a category
    /// </summary>
    public IEnumerable<string> GetRecentPaths(string category, int maxCount = 10)
    {
        if (string.IsNullOrWhiteSpace(category)) 
     throw new ArgumentException("Category cannot be empty", nameof(category));

        if (_appSettings.MruLists.TryGetValue(category, out var mruList))
        {
        // Filter out paths that no longer exist
     var validPaths = mruList.Where(System.IO.Directory.Exists).ToList();

      // Update the list if we removed invalid paths
            if (validPaths.Count != mruList.Count)
       {
       _appSettings.MruLists[category] = validPaths;
        _appSettings.Save();
  }

     return validPaths.Take(maxCount);
        }

        return Enumerable.Empty<string>();
 }

  /// <summary>
 /// Clears recent paths for a category
    /// </summary>
    public void ClearRecentPaths(string category)
    {
        if (string.IsNullOrWhiteSpace(category)) 
       throw new ArgumentException("Category cannot be empty", nameof(category));

        if (_appSettings.MruLists.ContainsKey(category))
    {
   _appSettings.MruLists.Remove(category);
  _appSettings.Save();
   }
    }

    /// <summary>
 /// Removes a specific path from recent paths
  /// </summary>
    public void RemoveRecentPath(string category, string path)
    {
   if (string.IsNullOrWhiteSpace(category)) 
    throw new ArgumentException("Category cannot be empty", nameof(category));
  if (string.IsNullOrWhiteSpace(path)) 
            throw new ArgumentException("Path cannot be empty", nameof(path));

        path = System.IO.Path.GetFullPath(path);

   if (_appSettings.MruLists.TryGetValue(category, out var mruList))
    {
       if (mruList.Remove(path))
            {
     _appSettings.Save();
      }
        }
    }
}
