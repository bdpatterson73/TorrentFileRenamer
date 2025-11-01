# Phase 4 Testing Guide

## Overview
This guide provides comprehensive testing procedures for Phase 4 features: Status Filters and Enhanced Context Menus.

## Test Environment Setup

### Prerequisites
- TorrentFileRenamer WPF application running
- Sample TV episode files and/or movie files
- Access to source and destination folders

### Initial Setup
1. Launch the application
2. Navigate to either TV Episodes or Movies tab
3. Perform a scan to populate the list with test data
4. Ensure you have items in various states (Pending, Failed, Completed, Unparsed)

## Feature 1: Status Filter Dropdown

### Location
Both TV Episodes and Movies tabs, in the toolbar area between View Mode selector and Search box.

### Test 1.1: Filter UI Appearance
**Steps:**
1. Observe the status filter dropdown
2. Verify label reads "Status:"
3. Verify dropdown width is consistent
4. Hover over dropdown

**Expected Results:**
- ? Dropdown is visible and properly aligned
- ? Label is bold and readable
- ? Border turns blue (#2196F3) on hover
- ? Tooltip appears: "Filter by processing status"

### Test 1.2: Filter Options
**Steps:**
1. Click the status filter dropdown
2. Review all available options

**Expected Results:**
- ? "All" option is present and selected by default
- ? "Pending" option is present
- ? "Processing" option is present
- ? "Completed" option is present
- ? "Failed" option is present
- ? "Unparsed" option is present

### Test 1.3: Filter by Pending
**Steps:**
1. Ensure list has items in various states
2. Note total item count
3. Select "Pending" from filter dropdown
4. Observe the list

**Expected Results:**
- ? Only items with "Pending" status are visible
- ? Item count updates to show only pending items
- ? Items remain in correct order
- ? Status bar shows accurate count

### Test 1.4: Filter by Completed
**Steps:**
1. Process some items to completion
2. Select "Completed" from filter dropdown

**Expected Results:**
- ? Only items with "Completed" status are visible
- ? Green background color maintained in grid view
- ? Item count reflects completed items only

### Test 1.5: Filter by Failed
**Steps:**
1. Ensure you have some failed items
2. Select "Failed" from filter dropdown

**Expected Results:**
- ? Only items with "Failed" status are visible
- ? Red background color maintained in grid view
- ? Error messages are visible
- ? Item count reflects failed items only

### Test 1.6: Filter by Unparsed
**Steps:**
1. Have some unparsed files in the list
2. Select "Unparsed" from filter dropdown

**Expected Results:**
- ? Only unparsed items are visible
- ? Gray background and text maintained
- ? Item count reflects unparsed items only

### Test 1.7: Return to All
**Steps:**
1. After filtering, select "All" from dropdown

**Expected Results:**
- ? All items are visible again
- ? Item count returns to total count
- ? All status colors are visible

### Test 1.8: Filter Persistence During Operations
**Steps:**
1. Select "Pending" filter
2. Process some items
3. Observe the list during and after processing

**Expected Results:**
- ? Filter remains set to "Pending"
- ? Items that become "Completed" disappear from view
- ? Still-pending items remain visible
- ? Can manually reselect "All" to see processed items

### Test 1.9: Filter with Search Text
**Steps:**
1. Enter text in search box (e.g., a show/movie name)
2. Select a status filter (e.g., "Pending")
3. Observe results

**Expected Results:**
- ? Results match BOTH search text AND status filter
- ? Only items matching search text with selected status appear
- ? Item count is accurate

### Test 1.10: Filter in Different View Modes
**Steps:**
1. Select "Failed" filter
2. Switch between Cards, Compact, and Grid views

**Expected Results:**
- ? Filter persists across view mode changes
- ? Same filtered items visible in all view modes
- ? No items lost or duplicated

## Feature 2: Enhanced Context Menu (Grid View)

### Location
Right-click on any item in the DataGrid (Grid view mode)

### Test 2.1: Context Menu Appearance
**Steps:**
1. Switch to Grid view
2. Right-click on any item
3. Observe the context menu

**Expected Results:**
- ? Context menu appears at cursor position
- ? All menu items are visible and readable
- ? Icons display correctly for each item
- ? Separators divide menu into logical sections
- ? Disabled items are grayed out appropriately

### Test 2.2: View Details Command
**Steps:**
1. Right-click on an item
2. Select "View Details"
3. Read the dialog

**Expected Results:**
- ? Dialog appears with item details
- ? Shows show/movie name, season/year, episode numbers
- ? Shows source and destination paths
- ? Shows status and error messages if applicable
- ? Dialog has OK button to close

### Test 2.3: Open Source Folder (Existing)
**Steps:**
1. Right-click on an item with valid source path
2. Select "Open Source Folder"

**Expected Results:**
- ? Windows Explorer opens
- ? Opens to correct folder containing source file
- ? File is visible in Explorer window
- ? No error messages

### Test 2.4: Open Source Folder (Non-existent)
**Steps:**
1. Create item with invalid/deleted source path
2. Right-click and select "Open Source Folder"

**Expected Results:**
- ? Error dialog appears
- ? Message indicates folder cannot be opened
- ? No Explorer window opens
- ? Application remains stable

### Test 2.5: Open Destination Folder (Existing)
**Steps:**
1. Right-click on completed item
2. Select "Open Destination Folder"

**Expected Results:**
- ? Windows Explorer opens
- ? Opens to destination folder
- ? Processed file is visible
- ? No error messages

### Test 2.6: Open Destination Folder (Non-existent)
**Steps:**
1. Right-click on pending item (not yet processed)
2. Select "Open Destination Folder"

**Expected Results:**
- ? Information dialog appears
- ? Message states folder doesn't exist yet
- ? Message mentions it will be created during processing
- ? No Explorer window opens
- ? No error, just informative message

### Test 2.7: Copy Source Path
**Steps:**
1. Right-click on any item
2. Select "Copy Source Path"
3. Paste into Notepad or text editor

**Expected Results:**
- ? Status bar shows "Source path copied to clipboard"
- ? Clipboard contains correct source path
- ? Path is full absolute path
- ? Path matches item's source location

### Test 2.8: Copy Destination Path
**Steps:**
1. Right-click on any item
2. Select "Copy Destination Path"
3. Paste into Notepad or text editor

**Expected Results:**
- ? Status bar shows "Destination path copied to clipboard"
- ? Clipboard contains correct destination path
- ? Path is full absolute path
- ? Path matches item's destination location

### Test 2.9: Retry Failed (on Failed Item)
**Steps:**
1. Ensure you have a failed item
2. Right-click on failed item
3. Select "Retry Failed"
4. Wait for processing

**Expected Results:**
- ? Item status changes to "Processing" then "Completed" or "Failed"
- ? Error message clears if successful
- ? Success or error dialog appears
- ? If successful, item moves to destination

### Test 2.10: Retry Failed (on Non-Failed Item)
**Steps:**
1. Right-click on pending/completed item
2. Observe "Retry Failed" menu item

**Expected Results:**
- ? "Retry Failed" should not be visible or is disabled
- ? No action if accidentally clicked

### Test 2.11: Remove Selected
**Steps:**
1. Right-click on any item
2. Select "Remove"
3. Observe the list

**Expected Results:**
- ? Item is removed from list immediately
- ? Item count decreases by 1
- ? No confirmation dialog (quick action)
- ? Other items remain unchanged

### Test 2.12: Remove All Failed (with Failed Items)
**Steps:**
1. Ensure multiple failed items exist
2. Right-click anywhere in grid
3. Select "Remove All Failed"
4. Read confirmation dialog
5. Click Yes/OK

**Expected Results:**
- ? Confirmation dialog appears
- ? Dialog shows count of failed items
- ? After confirmation, all failed items are removed
- ? Status bar shows removal count and remaining count
- ? Other items (pending, completed) remain

### Test 2.13: Remove All Failed (Cancel)
**Steps:**
1. Right-click and select "Remove All Failed"
2. Click No/Cancel on confirmation

**Expected Results:**
- ? No items are removed
- ? List unchanged
- ? No status message

### Test 2.14: Remove All Failed (No Failed Items)
**Steps:**
1. Ensure no failed items in list
2. Right-click and observe "Remove All Failed"

**Expected Results:**
- ? Menu item is disabled (grayed out)
- ? Cannot be clicked
- ? OR shows message "No failed items to remove"

### Test 2.15: Remove All Completed (with Completed Items)
**Steps:**
1. Ensure multiple completed items exist
2. Right-click and select "Remove All Completed"
3. Confirm in dialog

**Expected Results:**
- ? Confirmation dialog shows count
- ? All completed items are removed
- ? Status bar shows removal count
- ? Pending/failed items remain

### Test 2.16: Remove All Completed (No Completed Items)
**Steps:**
1. Ensure no completed items
2. Observe "Remove All Completed" menu item

**Expected Results:**
- ? Menu item is disabled
- ? OR shows message "No completed items to remove"

### Test 2.17: Clear All
**Steps:**
1. Right-click and select "Clear All"
2. Confirm in dialog

**Expected Results:**
- ? Confirmation shows total count
- ? All items removed after confirmation
- ? Status bar shows "No episodes/movies scanned"
- ? Empty list displayed

### Test 2.18: Select All
**Steps:**
1. Right-click and select "Select All"

**Expected Results:**
- ? All visible items in grid are selected
- ? Selection is highlighted
- ? Item count shows all selected

## Feature 3: Integration Testing

### Test 3.1: Filter + Context Menu
**Steps:**
1. Set filter to "Failed"
2. Right-click on failed item
3. Select "Remove All Failed"
4. Confirm

**Expected Results:**
- ? All failed items removed
- ? List now empty (since filter is still "Failed")
- ? Changing filter to "All" shows remaining items

### Test 3.2: Search + Filter + Context Menu
**Steps:**
1. Enter search text
2. Set status filter
3. Right-click on filtered item
4. Copy path
5. Open folder

**Expected Results:**
- ? All operations work on filtered items
- ? Correct paths are copied
- ? Correct folders open
- ? Filter and search remain active

### Test 3.3: View Mode Switching
**Steps:**
1. Set a filter
2. Switch between Cards, Compact, Grid views
3. Use context menu in Grid view

**Expected Results:**
- ? Filter persists across view modes
- ? Same items visible in all modes
- ? Context menu only in Grid view (by design)
- ? Card/Compact views use inline buttons

### Test 3.4: Processing with Filter Active
**Steps:**
1. Set filter to "Pending"
2. Process items
3. Watch items disappear as they complete

**Expected Results:**
- ? Completed items removed from view (filter is Pending)
- ? Filter stays active
- ? Status bar count updates
- ? Can switch to "All" or "Completed" to see processed items

### Test 3.5: Bulk Operations
**Steps:**
1. Scan 50+ items
2. Process 20 (some succeed, some fail)
3. Filter to "Failed"
4. Remove All Failed
5. Filter to "Completed"
6. Remove All Completed
7. Check remaining items

**Expected Results:**
- ? Failed items removed correctly
- ? Completed items removed correctly
- ? Pending items remain
- ? Counts are accurate throughout
- ? No performance issues

## Feature 4: Edge Cases and Error Handling

### Test 4.1: Empty List
**Steps:**
1. Clear all items
2. Open status filter dropdown
3. Right-click in empty grid

**Expected Results:**
- ? Filter dropdown still works
- ? Context menu doesn't appear (no items to click)
- ? No errors or crashes

### Test 4.2: Single Item
**Steps:**
1. Have only one item in list
2. Apply all filter options
3. Use all context menu commands

**Expected Results:**
- ? Filter shows/hides item correctly
- ? All commands work on single item
- ? No errors

### Test 4.3: Network Path (UNC)
**Steps:**
1. Scan from network location (\\server\share\)
2. Open source folder
3. Copy path

**Expected Results:**
- ? Network paths handled correctly
- ? Explorer opens to network location
- ? Full UNC path copied to clipboard

### Test 4.4: Long Paths
**Steps:**
1. Use items with very long file paths (>200 characters)
2. Copy paths
3. Open folders

**Expected Results:**
- ? Long paths handled without truncation
- ? Clipboard contains full path
- ? Folders open correctly

### Test 4.5: Special Characters
**Steps:**
1. Use files with special characters in names
2. Use all context menu operations

**Expected Results:**
- ? Special characters handled correctly
- ? Paths copy accurately
- ? Explorer opens correctly

### Test 4.6: Concurrent Operations
**Steps:**
1. Start processing items
2. Try to use context menu during processing

**Expected Results:**
- ? Most commands disabled during processing
- ? Status remains accurate
- ? No conflicts or errors

### Test 4.7: Rapid Filter Changes
**Steps:**
1. Quickly change filter multiple times
2. Observe list updates

**Expected Results:**
- ? List updates smoothly
- ? No flickering or errors
- ? Final filter state is correct

## Performance Testing

### Test 5.1: Large Dataset (100+ items)
**Steps:**
1. Scan folder with 100+ files
2. Change filters repeatedly
3. Use context menu operations

**Expected Results:**
- ? Filter changes are instant (<100ms)
- ? Context menu appears immediately
- ? No lag or freezing

### Test 5.2: Bulk Removal Performance
**Steps:**
1. Have 500+ items
2. Process many to Failed
3. Remove All Failed

**Expected Results:**
- ? Removal completes quickly
- ? UI remains responsive
- ? Status updates smoothly

## Compatibility Testing

### Test 6.1: Windows Explorer Integration
**Steps:**
1. Test "Open Folder" commands on:
   - Local drives (C:, D:)
   - Network drives
   - External drives

**Expected Results:**
- ? Explorer opens for all drive types
- ? Correct folders displayed

### Test 6.2: Clipboard Integration
**Steps:**
1. Copy paths
2. Paste into various applications:
   - Notepad
   - Word
   - Command Prompt
   - Browser address bar

**Expected Results:**
- ? Paths paste correctly in all apps
- ? No formatting issues

## Regression Testing

### Test 7.1: Existing Features Still Work
**Steps:**
1. Verify toolbar buttons work
2. Verify view mode switching works
3. Verify search box works
4. Verify card actions work
5. Verify processing works

**Expected Results:**
- ? All Phase 1-3 features still functional
- ? No new bugs introduced
- ? Consistent behavior

### Test 7.2: Settings Persistence
**Steps:**
1. Set filter to "Pending"
2. Close and reopen app (if settings persist)

**Expected Results:**
- ? Filter state may or may not persist (check requirements)
- ? Other settings still work

## Accessibility Testing

### Test 8.1: Keyboard Navigation
**Steps:**
1. Tab through controls
2. Use arrow keys in context menu
3. Press Enter to activate

**Expected Results:**
- ? Can reach filter dropdown via Tab
- ? Can navigate context menu with arrows
- ? Enter key activates commands

### Test 8.2: Screen Reader (Optional)
**Steps:**
1. Use with screen reader enabled
2. Navigate to filter dropdown
3. Open context menu

**Expected Results:**
- ? Filter options announced
- ? Context menu items announced
- ? Current selection announced

## Bug Reporting Template

If you find an issue during testing, report it using this template:

```
**Bug Title**: [Brief description]

**Test Case**: [Test number, e.g., Test 2.7]

**Steps to Reproduce**:
1. [First step]
2. [Second step]
3. [etc.]

**Expected Result**:
[What should happen]

**Actual Result**:
[What actually happened]

**Severity**: [Critical / High / Medium / Low]

**Screenshots**: [If applicable]

**Additional Notes**: [Any other relevant information]
```

## Test Results Tracking

### TV Episodes Tab
| Test # | Description | Pass | Fail | Notes |
|--------|-------------|------|------|-------|
| 1.1 | Filter UI | ? | ? | |
| 1.2 | Filter Options | ? | ? | |
| 1.3 | Filter Pending | ? | ? | |
| ... | | | | |

### Movies Tab
| Test # | Description | Pass | Fail | Notes |
|--------|-------------|------|------|-------|
| 1.1 | Filter UI | ? | ? | |
| 1.2 | Filter Options | ? | ? | |
| ... | | | | |

## Sign-off

**Tester Name**: ___________________  
**Date**: ___________________  
**Build Version**: ___________________  
**Test Result**: ? Pass  ? Fail  ? Pass with Issues  

**Comments**:
_________________________________
_________________________________
_________________________________
