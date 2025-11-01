namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Service for managing Most Recently Used (MRU) lists
/// </summary>
public interface IMruService
{
    /// <summary>
    /// Adds a path to the MRU list
    /// </summary>
    void AddRecentPath(string category, string path);

    /// <summary>
 /// Gets the recent paths for a category
    /// </summary>
    IEnumerable<string> GetRecentPaths(string category, int maxCount = 10);

    /// <summary>
    /// Clears recent paths for a category
    /// </summary>
    void ClearRecentPaths(string category);

    /// <summary>
    /// Removes a specific path from recent paths
    /// </summary>
    void RemoveRecentPath(string category, string path);
}
