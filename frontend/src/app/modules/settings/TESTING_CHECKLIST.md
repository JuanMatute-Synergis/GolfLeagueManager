# Manual Testing Checklist for Refactored Settings Components

## Prerequisites
- Navigate to http://localhost:4201/settings
- Ensure the application is running without errors

## Test Cases

### 1. Settings Main Component Navigation
- [ ] Verify all four tabs are visible: Players, Seasons, Scoring, Schedule
- [ ] Click each tab and verify it switches content
- [ ] Verify the active tab is highlighted appropriately
- [ ] Verify URL doesn't change when switching tabs (single-page navigation)

### 2. Players Settings Component
- [ ] Verify players list loads with sample data
- [ ] Test "Add New Player" functionality:
  - [ ] Click "Add New Player" button
  - [ ] Fill in player form (Name, Email, Handicap)
  - [ ] Submit and verify player is added to list
  - [ ] Verify form validation (required fields)
- [ ] Test "Edit Player" functionality:
  - [ ] Click edit button on a player
  - [ ] Modify player details
  - [ ] Save and verify changes are reflected
- [ ] Test "Delete Player" functionality:
  - [ ] Click delete button on a player
  - [ ] Confirm deletion
  - [ ] Verify player is removed from list

### 3. Seasons Settings Component
- [ ] Verify seasons list loads with sample data
- [ ] Test "Add New Season" functionality:
  - [ ] Click "Add New Season" button
  - [ ] Fill in season form (Name, Start Date, End Date)
  - [ ] Submit and verify season is added
- [ ] Test Flight Management:
  - [ ] Click "Manage Flights" for a season
  - [ ] Add a new flight
  - [ ] Assign players to flight
  - [ ] Assign flight leader
  - [ ] Verify flight assignments are saved
- [ ] Test Edit/Delete season functionality

### 4. Scoring Settings Component
- [ ] Verify scoring form loads with default values
- [ ] Test points configuration:
  - [ ] Modify win points value
  - [ ] Modify loss points value
  - [ ] Modify tie points value
  - [ ] Save settings and verify confirmation
- [ ] Test form validation:
  - [ ] Enter negative values (should be invalid)
  - [ ] Enter non-numeric values (should be invalid)
  - [ ] Verify required field validation

### 5. Schedule Settings Component
- [ ] Verify schedule templates list loads
- [ ] Test "Add New Template" functionality:
  - [ ] Fill in template form
  - [ ] Submit and verify template is added
- [ ] Test Settings Configuration:
  - [ ] Modify default play day
  - [ ] Modify default start time
  - [ ] Toggle auto-generation settings
  - [ ] Save and verify settings are updated
- [ ] Test template edit/delete functionality

### 6. General UI/UX Testing
- [ ] Verify Bootstrap styling is consistent across all tabs
- [ ] Test responsive design (resize browser window)
- [ ] Verify loading states (if implemented)
- [ ] Verify error messages display properly
- [ ] Test keyboard navigation
- [ ] Verify accessibility features

### 7. Data Persistence Testing
- [ ] Make changes in multiple tabs
- [ ] Refresh the page
- [ ] Navigate away and back to settings
- [ ] Verify data persistence (mock data will reset, but forms should maintain state)

### 8. Performance Testing
- [ ] Switch rapidly between tabs
- [ ] Add multiple items quickly
- [ ] Verify no memory leaks or performance issues
- [ ] Check browser console for errors

## Expected Results
- All components should load without errors
- Form validation should work as expected
- CRUD operations should work with mock data
- UI should be responsive and user-friendly
- No console errors or warnings
- Smooth navigation between tabs

## Notes
- This is using mock data, so changes won't persist between page refreshes
- Real API integration would be needed for production
- All components are standalone and can be easily integrated into other parts of the application
