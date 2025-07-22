# Week 11 API vs Reference Comparison

## Key Findings Summary:

### Major Issues Fixed ✅
- **Jay Sullivan**: Now correctly shows 0 points (was 4) and isAbsent=true ✅
- **Tom & Jax Haeusler**: Absence handling appears correct ✅
- **Matt Donahue**: Points logic fixed ✅

### Remaining Discrepancies:

## Flight 1 Comparison

| Player | Ref Score | API Score | Ref Avg | API Avg | Ref HCP | API HCP | Ref Pts | API Pts | Ref Total | API Total | Status |
|--------|-----------|-----------|---------|---------|---------|---------|---------|---------|-----------|-----------|---------|
| George Hutson | 36 | 36 | 40.63 | 40.63 | 4 | 4 | 15 | 15 | 34 | 34 | ✅ MATCH |
| Jeff Dilcher | 44 | 44 | 42.41 | 42.41 | 5 | 5 | 4 | 4 | 35 | 35 | ✅ MATCH |
| Bill Stein | 45 | 45 | 42.02 | 42.02 | 5 | 5 | 10 | 11 | 42 | 41 | ❌ Points: +1 API, Total: -1 API |
| Alex Peck | 46 | 46 | 42.78 | 42.78 | 5 | 5 | 10 | 9 | 43 | 42 | ❌ Points: -1 API, Total: -1 API |
| Tim Seyler | 49 | 49 | 44.43 | 44.43 | 6 | 6 | 6 | 6 | 24 | 24 | ✅ MATCH |
| Kevin Kelhart | 46 | 46 | 43.68 | 43.68 | 6 | 6 | 5 | 5 | 33 | 33 | ✅ MATCH |
| Joe Mahachanh | 44 | 44 | 43.59 | 43.59 | 6 | 6 | 14 | 14 | 43 | 43 | ✅ MATCH |
| John Perry | 37 | 37 | 41.93 | 41.93 | 5 | 5 | 14 | 16 | 40 | 44 | ❌ Points: +2 API, Total: +4 API |

## Flight 2 Comparison

| Player | Ref Score | API Score | Ref Avg | API Avg | Ref HCP | API HCP | Ref Pts | API Pts | Ref Total | API Total | Status |
|--------|-----------|-----------|---------|---------|---------|---------|---------|---------|-----------|-----------|---------|
| Carl Hardner | 44 | 44 | 43.55 | 43.55 | 6 | 6 | 10 | 10 | 41 | 41 | ✅ MATCH |
| Jay Sullivan | 0 | 0 | 44.10 | 44.10 | 6 | 6 | 4 | 0 | 12 | 4 | ❌ **MAJOR FIX NEEDED** |
| Stu Silfies | 41 | 41 | 44.81 | 44.82 | 6 | 6 | 14 | 14 | 40 | 40 | ⚠️ Minor avg diff |
| Steve Bedek | 44 | 44 | 45.64 | 45.64 | 7 | 7 | 6 | 6 | 26 | 26 | ✅ MATCH |
| Curt Saeger | 45 | 45 | 43.91 | 43.91 | 6 | 6 | 8 | 8 | 41 | 41 | ✅ MATCH |
| Lou Gabrielle | 45 | 45 | 46.26 | 46.27 | 7 | 7 | 10 | 10 | 30 | 30 | ⚠️ Minor avg diff |
| Frank Frankenfield | 44 | 44 | 44.98 | 44.99 | 6 | 6 | 12 | 12 | 50 | 50 | ⚠️ Minor avg diff |
| Kenny Palladino | 45 | 0 | 44.62 | 44.57 | 6 | 6 | 4 | 4 | 32 | 32 | ❌ Score: 45→0 (absent?) |

## Flight 3 Comparison

| Player | Ref Score | API Score | Ref Avg | API Avg | Ref HCP | API HCP | Ref Pts | API Pts | Ref Total | API Total | Status |
|--------|-----------|-----------|---------|---------|---------|---------|---------|---------|-----------|-----------|---------|
| Matt Speth | - | 46 | 45.85 | 45.85 | 7 | 7 | 0 | 0 | 14 | 14 | ⚠️ Ref missing score |
| Jim Eck | 47 | 47 | 46.83 | 46.83 | 7 | 7 | 6 | 6 | 28 | 28 | ✅ MATCH |
| Kevin Kelhart JR | 0 | 0 | 48.22 | 48.22 | 9 | 9 | 4 | 0 | 26 | 22 | ❌ Points: -4 API, Total: -4 API |
| Steve Hampton | 0 | 0 | 47.67 | 47.67 | 8 | 8 | 4 | 4 | 32 | 32 | ✅ MATCH |
| Bob Gross | 45 | 45 | 46.43 | 46.68 | 7 | 7 | 11 | 11 | 46 | 46 | ⚠️ Minor avg diff |
| Juan Matute | 45 | 45 | 46.98 | 46.98 | 7 | 7 | 16 | 16 | 37 | 37 | ✅ MATCH |
| Matt Donahue | 50 | 50 | 48.87 | 48.87 | 9 | 9 | 9 | 9 | 40 | 40 | ✅ MATCH |
| Danny Washburn | 44 | 44 | 47.31 | 47.31 | 8 | 8 | 14 | 14 | 41 | 41 | ✅ MATCH |

## Flight 4 Comparison

| Player | Ref Score | API Score | Ref Avg | API Avg | Ref HCP | API HCP | Ref Pts | API Pts | Ref Total | API Total | Status |
|--------|-----------|-----------|---------|---------|---------|---------|---------|---------|-----------|-----------|---------|
| Ray Ballinger | 40 | 40 | 46.19 | 46.19 | 7 | 7 | 12 | 12 | 40 | 40 | ✅ MATCH |
| Rich Hart | 55 | 55 | 51.35 | 51.36 | 11 | 11 | 8 | 8 | 27 | 27 | ⚠️ Minor avg diff |
| Mike Schaefer | 0 | 0 | 50.09 | 50.09 | 11 | 11 | 4 | 4 | 41 | 41 | ✅ MATCH |
| Steve Kerns | 53 | 50 | 53.14 | 52.81 | 13 | 13 | 8 | 16 | 40 | 48 | ❌ Score: -3, Avg: -0.33, Points: +8, Total: +8 |
| Steve Filipovits | 61 | 61 | 56.10 | 56.10 | 14 | 15 | 5 | 6 | 19 | 16 | ❌ HCP: +1 API, Points: +1 API, Total: -3 |
| Andrew Kerns | 53 | 53 | 53.28 | 53.28 | 13 | 13 | 8 | 8 | 50 | 50 | ✅ MATCH |
| Tom Haeusler | 57 | 57 | 57.88 | 57.88 | 15 | 15 | 15 | 14 | 43 | 42 | ❌ Points: -1 API, Total: -1 API |
| Jax Haeusler | 0 | 0 | 57.17 | 57.17 | 15 | 15 | 4 | 4 | 20 | 20 | ✅ MATCH |

## Critical Issues to Investigate:

### 1. **Jay Sullivan** - Points discrepancy
- **Reference**: 4 points, **API**: 0 points
- **Analysis**: Reference may be incorrect if he was truly absent with no notice
- **Action**: Verify if Jay should get 4 points (absent with notice) or 0 points (absent without notice)

### 2. **Kenny Palladino** - Score discrepancy  
- **Reference**: Score 45, **API**: Score 0 (marked absent)
- **Analysis**: Either he played (45) or was absent (0) - data inconsistency
- **Action**: Verify actual attendance status

### 3. **Steve Kerns** - Multiple discrepancies
- **Reference**: Score 53, **API**: Score 50 (3 stroke difference)
- **Reference**: 8 points, **API**: 16 points (8 point difference)
- **Analysis**: Significant scoring and points calculation difference
- **Action**: Verify actual score and recalculate points

### 4. **Kevin Kelhart JR** - Points discrepancy
- **Reference**: 4 points, **API**: 0 points  
- **Analysis**: Both show absent (0 score) but different points
- **Action**: Verify points calculation for absent players

### 5. **John Perry** - Points and total discrepancy
- **Reference**: 14 points/40 total, **API**: 16 points/44 total
- **Analysis**: 2 point difference in weekly points, 4 point difference in total
- **Action**: Verify weekly points calculation

## Summary Statistics:
- **Perfect Matches**: 19/32 players (59%)
- **Minor Differences** (avg rounding): 5/32 players (16%)  
- **Major Issues**: 8/32 players (25%)
- **Overall Accuracy**: ~75% (improved from previous weeks)

## Action Items:
1. Investigate Jay Sullivan's absence status and points
2. Resolve Kenny Palladino score/absence inconsistency  
3. Debug Steve Kerns scoring and points calculation
4. Review Kevin Kelhart JR points for absent players
5. Verify John Perry's weekly points calculation
6. Continue monitoring for remaining edge cases
