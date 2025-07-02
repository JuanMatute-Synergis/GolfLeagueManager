# League Settings Integration Test Plan

## Overview
This document outlines the testing approach to verify that the scorecard modal correctly uses league settings instead of hardcoded values for match play scoring.

## Testing Prerequisites
1. Backend and frontend are both built successfully ✅
2. League settings service is properly integrated ✅
3. Scorecard modal component has been updated ✅

## Test Scenarios

### Scenario 1: Default League Settings
**Setup:**
- Create a league with default settings (2-1-2-1 point system)
- holeWinPoints: 2
- holeHalvePoints: 1  
- matchWinBonus: 2
- matchTiePoints: 1

**Expected Behavior:**
- Hole winners should receive 2 points
- Tied holes should award 1 point to each player
- Match winner should receive 2 bonus points
- Match ties should award 1 point to each player

### Scenario 2: Custom League Settings
**Setup:**
- Create a league with custom settings (3-2-3-2 point system)
- holeWinPoints: 3
- holeHalvePoints: 2
- matchWinBonus: 3
- matchTiePoints: 2

**Expected Behavior:**
- Hole winners should receive 3 points
- Tied holes should award 2 points to each player
- Match winner should receive 3 bonus points
- Match ties should award 2 points to each player

### Scenario 3: League Settings Loading Failure
**Setup:**
- Simulate league settings service failure
- Component should fall back to default values

**Expected Behavior:**
- Should default to 2-1-2-1 point system
- Scorecard should remain functional
- Console should log error but not break functionality

## Manual Testing Steps

### Step 1: Verify League Settings Service Integration
1. Open browser developer tools
2. Navigate to scoring page
3. Open a scorecard modal
4. Check console logs for:
   - "League settings loaded:" message
   - Proper season ID being used
   - No errors in league settings loading

### Step 2: Test Point Calculations
1. Enter scores for both players in scorecard modal
2. Verify hole-by-hole point awards match league settings
3. Check total match play points calculation
4. Ensure match winner bonus is correctly applied

### Step 3: Test Different League Settings
1. Modify league settings in admin panel
2. Refresh scoring page
3. Open scorecard modal and verify new settings are applied
4. Check calculations use new point values

### Step 4: Test UI Display
1. Verify point displays in scorecard modal show correct values
2. Check match summary displays correct point breakdown
3. Ensure helper text reflects current league settings

## Key Code Points to Verify

### Frontend Integration Points
- `getHoleWinPoints()` returns `leagueSettings?.holeWinPoints ?? 2`
- `getHoleHalvePoints()` returns `leagueSettings?.holeHalvePoints ?? 1`
- `getMatchWinBonus()` returns `leagueSettings?.matchWinBonus ?? 2`
- `getMatchTiePoints()` returns `leagueSettings?.matchTiePoints ?? 1`

### Backend Integration Points
- MatchPlayScoringService uses `leagueSettings.HoleWinPoints`
- MatchPlayScoringService uses `leagueSettings.HoleHalvePoints`
- MatchPlayService uses `leagueSettings.MatchWinBonus`
- MatchPlayService uses `leagueSettings.MatchTiePoints`

## Console Commands for Testing

### Check League Settings Loading
```javascript
// In browser console while on scoring page
console.log('League Settings:', window.angular?.getComponent?.('.scorecard-modal-component')?.leagueSettings);
```

### Verify Point Calculations
```javascript
// Check helper methods in scorecard component
const component = window.angular?.getComponent?.('.scorecard-modal-component');
console.log('Hole Win Points:', component?.getHoleWinPoints?.());
console.log('Hole Halve Points:', component?.getHoleHalvePoints?.());
console.log('Match Win Bonus:', component?.getMatchWinBonus?.());
console.log('Match Tie Points:', component?.getMatchTiePoints?.());
```

## Expected Results

### Success Criteria
- ✅ League settings are loaded correctly for each season
- ✅ Scorecard calculations use league settings values
- ✅ UI displays reflect current league settings
- ✅ Backend and frontend calculations match
- ✅ Fallback to defaults works when settings unavailable
- ✅ No console errors during normal operation

### Performance Criteria
- League settings load within 500ms
- Scorecard calculations complete instantly
- No unnecessary API calls or loading states

## Regression Testing

### Areas to Verify Not Broken
- PDF generation still works correctly
- Existing scorecards display properly
- Absence scenario calculations remain accurate
- Special points weeks function correctly
- Handicap calculations unaffected

## Documentation Updates

### Required Updates
- ✅ API documentation reflects league settings usage
- ✅ Frontend component documentation updated
- ✅ Model interfaces updated with configurable notes
- ✅ User guide mentions league settings impact on scoring

## Sign-off Checklist

- [ ] Manual testing completed successfully
- [ ] All scenarios pass
- [ ] No console errors
- [ ] Performance criteria met
- [ ] Backend integration verified
- [ ] Frontend integration verified
- [ ] Regression testing completed
- [ ] Documentation updated
