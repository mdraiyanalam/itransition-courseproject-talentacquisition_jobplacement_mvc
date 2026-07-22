# Phase 3: UI Improvements & Real-time Discussions - Implementation Plan

## Overview
Phase 3 focuses on enhancing the user interface, implementing inline editing for CVs, and ensuring real-time discussions via SignalR work correctly.

---

## Feature 1: Edit CV UI Improvements

### Current State
- Edit CV view at `Views/CVs/Edit.cshtml`
- Shows basic form with text inputs for CV fields
- Attribute values displayed as separate form fields

### Improvements to Implement

#### 1.1 Organize Form Layout
- [ ] Group CV sections (Personal, Education, Experience, etc.)
- [ ] Use cards/sections for better visual hierarchy
- [ ] Add collapsible sections for optional fields
- [ ] Highlight missing required fields in red

#### 1.2 Inline Editing Support
- [ ] Make form fields editable directly (not modal-based)
- [ ] Save button at bottom of each section
- [ ] Unsaved changes indicator (orange/yellow border)
- [ ] Confirmation dialog before leaving with unsaved changes

#### 1.3 Attribute Value Input Improvements
- [ ] Render appropriate input based on attribute type
  - Text → text input
  - Number → number input
  - Boolean → checkbox
  - Dropdown → select element
- [ ] Add attribute descriptions as tooltips/help text
- [ ] Validation feedback inline

#### 1.4 Template System
- [ ] Implement CV template selector
- [ ] Multiple professionally designed templates
- [ ] Live preview as template changes
- [ ] Template-specific styling in PDF

---

## Feature 2: EditProject Form Tag Autocomplete

### Current State
- EditProject view uses Tagify for tag input
- Tags stored as comma-separated string in database
- JSON format support for Tagify data

### Improvements to Implement

#### 2.1 Autocomplete Enhancements
- [ ] Fetch tag suggestions from `TagSuggestions` API endpoint
- [ ] Show autocomplete dropdown as user types
- [ ] Highlight matches in suggestion list
- [ ] Show tag frequency/popularity

#### 2.2 Tag Categories
- [ ] Group tags by technology category
  - Languages: C#, JavaScript, Python, etc.
  - Frameworks: .NET, React, Vue, etc.
  - Databases: SQL Server, PostgreSQL, etc.
  - Tools: Docker, Kubernetes, etc.
- [ ] Category filtering in dropdown
- [ ] Color coding by category

#### 2.3 Validation
- [ ] Validate tag format
- [ ] Prevent duplicates
- [ ] Tag length limits
- [ ] Real-time validation feedback

---

## Feature 3: Real-time Discussions SignalR

### Current State
- DiscussionHub implemented in `Hubs/DiscussionHub.cs`
- Hub route mapped in `Program.cs` as `/discussionHub`
- Client JavaScript in `Views/Positions/Details.cshtml`

### Improvements to Implement

#### 3.1 Verify Hub Configuration
- [ ] Check hub mapping is correct
- [ ] Verify hub methods are accessible
- [ ] Test with browser DevTools Network tab
- [ ] Check for connection errors in console

#### 3.2 Fix Client-Side Code
- [ ] Verify SignalR client library loading
- [ ] Check connection URL matches hub route
- [ ] Implement proper error handling
- [ ] Add reconnection logic with exponential backoff
- [ ] Display connection status to user

#### 3.3 Message Broadcasting
- [ ] Ensure message broadcasts to all clients
- [ ] Show message author name and timestamp
- [ ] Update UI without page reload
- [ ] Scroll to newest message

#### 3.4 Real-time Updates
- [ ] Discussion posts appear immediately for all users
- [ ] User typing indicators (optional)
- [ ] Online user count
- [ ] Notification for new messages

---

## Feature 4: Profile Sync with CV

### Current State
- CV creation extracts values from CandidateProfile
- CandidateProfile stores overall candidate information
- Project completion happens independently

### Improvements to Implement

#### 4.1 Bi-directional Sync
- [ ] Update CandidateProfile from CV data
- [ ] Sync CV display with Profile changes
- [ ] Keep experience summary in sync
- [ ] Aggregate education/skills from CVs

#### 4.2 Edit CV Updates Profile
- [ ] When candidate edits CV experience
- [ ] Offer to update profile
- [ ] Show diff between current profile and CV data
- [ ] Allow merge/overwrite options

---

## Implementation Tasks

### High Priority
- [ ] **Organize CV Edit form layout** (Day 1)
- [ ] **Fix EditProject tag autocomplete** (Day 1)
- [ ] **Test SignalR connection** (Day 1)
- [ ] **Implement reconnection logic** (Day 2)

### Medium Priority
- [ ] **Add attribute type-specific inputs** (Day 2)
- [ ] **Implement CV template selector** (Day 3)
- [ ] **Add inline editing for CV sections** (Day 3)

### Low Priority
- [ ] **Category-based tag grouping** (Day 4)
- [ ] **User typing indicators** (Day 4)
- [ ] **Profile-CV sync improvements** (Day 4)

---

## Testing Checklist

### CV Edit UI
- [ ] All form sections display correctly
- [ ] Collapsible sections work
- [ ] Inline editing saves changes
- [ ] Unsaved changes indicator appears
- [ ] Validation errors display
- [ ] PDF generation works with new layout

### EditProject Tags
- [ ] Autocomplete suggestions appear
- [ ] Tag filtering works
- [ ] Duplicate prevention works
- [ ] Tags save correctly to database
- [ ] Tags display on project view

### SignalR Discussions
- [ ] Connection establishes successfully
- [ ] Messages broadcast in real-time
- [ ] Connection status visible to user
- [ ] Reconnection works after disconnect
- [ ] Multiple clients see same messages

---

## Code Examples

### Attribute Type-Specific Input (Bootstrap Markup)
```html
@if (attribute.Type == "Text")
{
    <input type="text" class="form-control" name="attr_@attribute.Id" 
           placeholder="@attribute.Name" />
}
else if (attribute.Type == "Number")
{
    <input type="number" class="form-control" name="attr_@attribute.Id" />
}
else if (attribute.Type == "Boolean")
{
    <input type="checkbox" class="form-check-input" name="attr_@attribute.Id" />
}
else if (attribute.Type == "Dropdown")
{
    <select class="form-control" name="attr_@attribute.Id">
        @foreach (var option in attribute.Options?.Split(',') ?? new string[0])
        {
            <option value="@option.Trim()">@option.Trim()</option>
        }
    </select>
}
```

### SignalR Reconnection Logic (JavaScript)
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/discussionHub")
    .withAutomaticReconnect([0, 0, 0, 1000, 3000, 5000, 10000])
    .build();

connection.onreconnected(() => {
    console.log("✅ Reconnected to server");
    updateConnectionStatus("Connected");
});

connection.onreconnecting(() => {
    console.log("⚠️ Attempting to reconnect...");
    updateConnectionStatus("Reconnecting...");
});
```

---

## Post-Implementation Testing

1. **Regression Testing**: Ensure Phase 1 & 2 features still work
2. **UI Testing**: Test on multiple browsers (Chrome, Firefox, Edge)
3. **Performance Testing**: Monitor response times for tag autocomplete
4. **Load Testing**: Test SignalR with multiple concurrent connections
5. **Mobile Testing**: Responsive design verification

---

## Known Issues & Workarounds

### Issue 1: SignalR Connection Timeout
- **Cause**: Hub not responding
- **Workaround**: Check hub mapping in Program.cs, verify route is `/discussionHub`

### Issue 2: Tags Not Saving
- **Cause**: JSON parsing error
- **Workaround**: Verify Tagify data format, check for special characters

### Issue 3: CV Edit Form Too Long
- **Cause**: Too many fields on single page
- **Workaround**: Implement collapsible sections or multi-step form

---

## Future Enhancements (Phase 4+)

- [ ] AI-powered CV suggestions
- [ ] CV versioning and history
- [ ] CV comparison tool
- [ ] Bulk CV operations
- [ ] Advanced discussion features (threads, reactions)
- [ ] Typing indicators and presence
- [ ] Message search and filtering

---

**Estimated Effort:** 3-4 days
**Priority:** High
**Dependencies:** Phase 1 & 2 must be complete
**Status:** Ready to implement

Last Updated: 2026-07-22
