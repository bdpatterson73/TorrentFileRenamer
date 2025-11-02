using System;
using System.Drawing;
using System.Windows.Forms;

namespace TorrentFileRenamer
{
    /// <summary>
    /// Utility class for UI-related operations that were previously in Form1.cs
    /// </summary>
    public static class UIUtilities
    {
        /// <summary>
        /// Configures a ListView for modern appearance with alternating row colors
        /// </summary>
        /// <param name="listView">The ListView to configure</param>
        public static void ConfigureListViewForModernAppearance(ListView listView)
        {
            listView.OwnerDraw = true;
            listView.DrawItem += ListView_DrawItem;
            listView.DrawSubItem += ListView_DrawSubItem;
            listView.DrawColumnHeader += ListView_DrawColumnHeader;
        }

        /// <summary>
        /// Draws a ListView column header with modern styling
        /// </summary>
        public static void ListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
            {
                e.Graphics.DrawRectangle(pen, e.Bounds);
            }

            var textRect = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 6, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, new Font("Segoe UI", 9F, FontStyle.Bold),
                textRect, Color.FromArgb(60, 60, 60),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        /// <summary>
        /// Draws a ListView item with modern styling
        /// </summary>
        public static void ListView_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            // This will be handled by DrawSubItem for detailed view
            if (sender is ListView listView && listView.View != View.Details)
            {
                e.DrawDefault = true;
            }
        }

        /// <summary>
        /// Draws ListView sub-items with alternating row colors and modern styling
        /// </summary>
        public static void ListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            var backColor = e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 248, 248);

            if (e.Item.Selected)
            {
                backColor = Color.FromArgb(220, 235, 252);
            }

            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            var textColor = e.Item.Selected ? Color.FromArgb(40, 40, 40) : Color.FromArgb(60, 60, 60);
            var textRect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height);

            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, new Font("Segoe UI", 9F),
                textRect, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // Draw grid lines
            using (var pen = new Pen(Color.FromArgb(230, 230, 230)))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        /// <summary>
        /// Gets the help text for the application
        /// </summary>
        /// <returns>Formatted help text</returns>
        public static string GetApplicationHelpText()
        {
            return @"Torrent File Renamer Help

TV EPISODES:
• Supports formats: S01E01, 1x01, Season 1 Episode 1
• Multi-episode files: S01E01-02, S01E01E02
• Creates structure: Show Name/Season X/Episode files

MOVIES:
• Organizes by first letter (A-Z, 0-9, #)
• Creates individual movie folders for each file
• Example: Lost In The Amazon (2024).mp4 ? L/Lost In The Amazon (2024)/Lost In The Amazon (2024).mp4
• Removes quality tags (720p, 1080p, etc.)
• Handles years in various formats

FEATURES:
• Smart file type detection
• Multi-select removal
• Progress tracking
• Disk space checking
• Network path support

TIPS:
• Always verify destination paths
• Use 'Remove Unparsed' for cleanup
• Check available disk space
• Review unparsed files (gray) before processing";
        }

        /// <summary>
        /// Gets the about text for the application
        /// </summary>
        /// <returns>Formatted about text</returns>
        public static string GetApplicationAboutText()
        {
            return @"Torrent File Renamer
Version 2.0 Enhanced

Features:
• TV Episode Organization (S01E01 format)
• Movie Organization (Alphabetical with individual folders)
• Smart File Type Detection
• Automatic HandBrake Monitoring
• Multi-select Operations
• Path Validation & Space Checking
• Network Drive Support
• Plex Compatibility Validation

© 2024 Enhanced for .NET 8
Built with intelligent media detection";
        }

        /// <summary>
        /// Shows application logs in a dedicated form
        /// </summary>
        /// <param name="parentForm">The parent form for positioning</param>
        /// <param name="maxLogEntries">Maximum number of log entries to display</param>
        public static void ShowApplicationLogs(Form parentForm, int maxLogEntries = 200)
        {
            try
            {
                var logs = LoggingService.GetRecentLogs(maxLogEntries);

                var logForm = new Form
                {
                    Text = "Application Logs",
                    Size = new Size(800, 600),
                    StartPosition = FormStartPosition.CenterParent
                };

                var textBox = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 9),
                    ReadOnly = true,
                    Text = string.Join(Environment.NewLine, logs)
                };

                logForm.Controls.Add(textBox);
                logForm.ShowDialog(parentForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to show logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}