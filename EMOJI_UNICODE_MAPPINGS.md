# Emoji to Unicode Escape Code Mappings for SettingsDialog.xaml

This document lists all emoji replacements needed for proper rendering in WPF.

## Header Icon
- `?` (Gear) ? `&#x2699;&#xFE0F;` (U+2699 GEAR + variation selector)

## Section Headers (General Tab)
- `?? Default Paths` ? `&#x1F4C1; Default Paths` (U+1F4C1 FILE FOLDER)
- `?? File Extensions` ? `&#x1F4C4; File Extensions` (U+1F4C4 PAGE FACING UP)
- `? Preferences` ? `&#x2611;&#xFE0F; Preferences` (U+2611 BALLOT BOX WITH CHECK + variation selector)
- `?? Processing Options` ? `&#x2699;&#xFE0F; Processing Options` (U+2699 GEAR + variation selector)

## Section Headers (Logging Tab)
- `?? Logging Configuration` ? `&#x1F4DD; Logging Configuration` (U+1F4DD MEMO)
- `? About Logging` ? `&#x2139;&#xFE0F; About Logging` (U+2139 INFORMATION SOURCE + variation selector)

## Section Headers (Plex Tab)
- `?? Plex Compatibility` ? `&#x1F4FA; Plex Compatibility` (U+1F4FA TELEVISION)
- `?? About Plex Compatibility` ? `&#x26A0;&#xFE0F; About Plex Compatibility` (U+26A0 WARNING SIGN + variation selector)

## Section Headers (Auto-Monitor Tab)
- `?? Monitoring Folders` ? `&#x1F4C1; Monitoring Folders` (U+1F4C1 FILE FOLDER)
- `? Monitoring Behavior` ? `&#x1F50D; Monitoring Behavior` (U+1F50D LEFT-POINTING MAGNIFYING GLASS)
- `?? Options` ? `&#x2699;&#xFE0F; Options` (U+2699 GEAR + variation selector)

## Section Headers (Advanced Tab)
- `?? Settings Management` ? `&#x1F4BE; Settings Management` (U+1F4BE FLOPPY DISK)
- `?? Quick Presets` ? `&#x26A1; Quick Presets` (U+26A1 HIGH VOLTAGE SIGN)
- `? Warning` ? `&#x26A0;&#xFE0F; Warning` (U+26A0 WARNING SIGN + variation selector)

## Button Icons
- `?? Export Settings` ? `&#x1F4E4; Export Settings` (U+1F4E4 OUTBOX TRAY)
- `?? Import Settings` ? `&#x1F4E5; Import Settings` (U+1F4E5 INBOX TRAY)
- `? Basic Configuration` ? `&#x1F195; Basic Configuration` (U+1F195 NEW BUTTON)
- `?? Advanced Configuration` ? `&#x1F680; Advanced Configuration` (U+1F680 ROCKET)
- `?? Plex-Optimized Configuration` ? `&#x1F4FA; Plex-Optimized Configuration` (U+1F4FA TELEVISION)
- `?? Reset to Defaults` ? `&#x21BA; Reset to Defaults` (U+21BA ANTICLOCKWISE OPEN CIRCLE ARROW)
- `? Save` ? `&#x2714;&#xFE0F; Save` (U+2714 HEAVY CHECK MARK + variation selector)

## Slider Icon
- `? Current value:` ? `&#x1F3AF; Current value:` (U+1F3AF DIRECT HIT)

---

## Implementation Notes

1. **Variation Selector (&#xFE0F;)**: Added after some Unicode characters to ensure emoji-style rendering
2. **Alternative**: If emojis still don't render, use simple text alternatives or Font Awesome icons
3. **Testing**: Verify on actual WPF application as some emojis may not render in all system fonts

## Find and Replace Commands

To fix all at once in the XAML file, use these replacements:

1. Header: `Text="?"` ? `Text="&#x2699;&#xFE0F;"`
2. Default Paths: `Text="?? Default Paths"` ? `Text="&#x1F4C1; Default Paths"`
3. File Extensions: `Text="?? File Extensions"` ? `Text="&#x1F4C4; File Extensions"`
4. Preferences: `Text="? Preferences"` ? `Text="&#x2611;&#xFE0F; Preferences"`
5. Processing Options: `Text="?? Processing Options"` ? `Text="&#x2699;&#xFE0F; Processing Options"`
6. Logging Configuration: `Text="?? Logging Configuration"` ? `Text="&#x1F4DD; Logging Configuration"`
7. About Logging: `Text="? About Logging"` ? `Text="&#x2139;&#xFE0F; About Logging"`
8. Plex Compatibility: `Text="?? Plex Compatibility"` ? `Text="&#x1F4FA; Plex Compatibility"`
9. About Plex: `Text="?? About Plex Compatibility"` ? `Text="&#x26A0;&#xFE0F; About Plex Compatibility"`
10. Monitoring Folders: `Text="?? Monitoring Folders"` ? `Text="&#x1F4C1; Monitoring Folders"`
11. Monitoring Behavior: `Text="? Monitoring Behavior"` ? `Text="&#x1F50D; Monitoring Behavior"`
12. Options: `Text="?? Options"` ? `Text="&#x2699;&#xFE0F; Options"`
13. Settings Management: `Text="?? Settings Management"` ? `Text="&#x1F4BE; Settings Management"`
14. Quick Presets: `Text="?? Quick Presets"` ? `Text="&#x26A1; Quick Presets"`
15. Warning: `Text="? Warning"` ? `Text="&#x26A0;&#xFE0F; Warning"`
16. Export: `Content="?? Export Settings"` ? `Content="&#x1F4E4; Export Settings"`
17. Import: `Content="?? Import Settings"` ? `Content="&#x1F4E5; Import Settings"`
18. Basic: `Content="? Basic Configuration"` ? `Content="&#x1F195; Basic Configuration"`
19. Advanced: `Content="?? Advanced Configuration"` ? `Content="&#x1F680; Advanced Configuration"`
20. Plex Preset: `Content="?? Plex-Optimized Configuration"` ? `Content="&#x1F4FA; Plex-Optimized Configuration"`
21. Reset: `Content="?? Reset to Defaults"` ? `Content="&#x21BA; Reset to Defaults"`
22. Save: `Content="? Save"` ? `Content="&#x2714;&#xFE0F; Save"`
23. Slider: `Text="? Current value: "` ? `Text="&#x1F3AF; Current value: "`
