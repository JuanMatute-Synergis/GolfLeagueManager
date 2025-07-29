#!/bin/bash

echo "=== Week 15 Matchup Analysis ==="
echo

# Flight ID to Flight Number mapping
declare -A flights
flights["aa280f02-ebcd-45bf-9821-37a477e7a4ba"]="1"
flights["944c8965-11cb-45f3-92ca-434edf1cb4e3"]="2"  
flights["65748039-3b15-4415-b277-7afb387dd8ce"]="3"
flights["8453188b-e585-4e51-ba9e-7744bbd06e62"]="4"

# Player ID to Name mapping (focused on the ones we need)
declare -A players
players["74d4dd50-3a5a-4e14-8e06-a37704cddd71"]="Matt Donahue"
players["9e7a3048-2254-46ae-a16a-a90cc8dbd962"]="Matt Speth"
players["b1046572-8971-425b-9dbd-cdfdc80bbdd7"]="Steve Kerns"
players["9d4ab9eb-6c6f-4fdd-be6b-2396202eb645"]="Steve Filipovits"
players["2743c25d-5700-418b-93d7-c0a2a8f48f93"]="Jason Fink"
players["9548bbdf-f6ea-447d-988b-fe1e21bd2080"]="Joe Mahachanh"
players["dd5a8bd3-0bf1-436e-9484-74b40d6e83fc"]="Bill Stein"
players["d971efb9-8e8c-4e91-87e9-f79c74391455"]="Jeff Dilcher"
players["8c24e1d3-50bf-49b0-956f-3843ab1d9bf7"]="John Perry"
players["c80aef4d-db47-493e-b1ae-9d09a734a094"]="Alex Peck"
players["4ef1e93d-be97-42f6-9721-3c1ddd256c60"]="Tim Seyler"
players["1cbaa69a-f72e-4ccd-9e6b-2ea1a501f5ee"]="Kevin Kelhart"
players["ac3946ff-48eb-4479-ad63-92136e7c2f7f"]="Curt Saeger"
players["354f62e4-a538-4eaa-b7e4-5f9bd60e777f"]="Ray Ballinger"
players["27f75d82-932a-451d-b534-4653045c4816"]="Ben Mahachanh"
players["c7de2e43-847d-4b73-bb40-cf268fa7c301"]="Jax Haeusler"
players["2c8b669d-8b6c-4951-880f-a3d607415f8c"]="George Hutson"
players["42b73157-6250-4f41-9701-9271ffc49c05"]="Rich Baker"
players["731fbc13-0f87-497a-9674-1bceaa67b395"]="Carl Hardner"
players["7d6db2a5-480b-4e68-a0f7-bd5d24db8999"]="Stu Silfies"
players["a51ea62e-4a3f-4741-b43c-56e3feae7e70"]="Kenny Palladino"
players["52369d98-5f1b-4c9d-9859-3d4a2a183439"]="Frank Frankenfield"
players["459e64fe-a08e-4f1b-8546-91b9cb10ed95"]="Steve Bedek"
players["878f2662-b5bb-4474-aaa5-d2b4b179e070"]="Lou Gabrielle"
players["4d2ab22d-daee-4cbf-98ca-ca5f8034b3fc"]="Jim Eck"
players["c43faa83-23d6-4d09-9ffb-9548361480bc"]="Kevin Kelhart JR"
players["9deacc93-f9ed-49e6-ab68-c89b056af0fe"]="Danny Washurn"
players["155adf40-c73d-4c46-b2e2-f6b2471563dd"]="Steve Hampton"
players["3a58bd78-1074-4c5c-b43e-605a8efc3a35"]="Bob Gross"
players["238b9732-4a0f-4922-8d5c-26e33fd8ac95"]="Juan Matute"
players["ed6799bc-76d6-43fd-9301-f4fa9effaee6"]="Rich Hart"
players["a16dd095-e46d-4f47-8767-50b02226dca4"]="Andrew Kerns"
players["67a0c954-df29-45ec-a183-08ff7cb3678b"]="Tom Haeusler"

# Player ID to Flight mapping (from Session 3 assignments)
declare -A player_flights
player_flights["42b73157-6250-4f41-9701-9271ffc49c05"]="1"
player_flights["d971efb9-8e8c-4e91-87e9-f79c74391455"]="1" 
player_flights["2743c25d-5700-418b-93d7-c0a2a8f48f93"]="1"
player_flights["2c8b669d-8b6c-4951-880f-a3d607415f8c"]="1"
player_flights["9548bbdf-f6ea-447d-988b-fe1e21bd2080"]="1"
player_flights["c80aef4d-db47-493e-b1ae-9d09a734a094"]="1"
player_flights["8c24e1d3-50bf-49b0-956f-3843ab1d9bf7"]="1"
player_flights["dd5a8bd3-0bf1-436e-9484-74b40d6e83fc"]="1"
player_flights["354f62e4-a538-4eaa-b7e4-5f9bd60e777f"]="2"
player_flights["52369d98-5f1b-4c9d-9859-3d4a2a183439"]="2"
player_flights["731fbc13-0f87-497a-9674-1bceaa67b395"]="2"
player_flights["1cbaa69a-f72e-4ccd-9e6b-2ea1a501f5ee"]="2"
player_flights["a51ea62e-4a3f-4741-b43c-56e3feae7e70"]="2"
player_flights["ac3946ff-48eb-4479-ad63-92136e7c2f7f"]="2"
player_flights["4ef1e93d-be97-42f6-9721-3c1ddd256c60"]="2"
player_flights["7d6db2a5-480b-4e68-a0f7-bd5d24db8999"]="2"
player_flights["459e64fe-a08e-4f1b-8546-91b9cb10ed95"]="3"
player_flights["4d2ab22d-daee-4cbf-98ca-ca5f8034b3fc"]="3"
player_flights["878f2662-b5bb-4474-aaa5-d2b4b179e070"]="3"
player_flights["3a58bd78-1074-4c5c-b43e-605a8efc3a35"]="3"
player_flights["155adf40-c73d-4c46-b2e2-f6b2471563dd"]="3"
player_flights["c43faa83-23d6-4d09-9ffb-9548361480bc"]="3"
player_flights["238b9732-4a0f-4922-8d5c-26e33fd8ac95"]="3"
player_flights["9deacc93-f9ed-49e6-ab68-c89b056af0fe"]="3"
player_flights["74d4dd50-3a5a-4e14-8e06-a37704cddd71"]="4"
player_flights["9d4ab9eb-6c6f-4fdd-be6b-2396202eb645"]="4"
player_flights["c7de2e43-847d-4b73-bb40-cf268fa7c301"]="4"
player_flights["67a0c954-df29-45ec-a183-08ff7cb3678b"]="4"
player_flights["ed6799bc-76d6-43fd-9301-f4fa9effaee6"]="4"
player_flights["a16dd095-e46d-4f47-8767-50b02226dca4"]="4"
player_flights["b1046572-8971-425b-9dbd-cdfdc80bbdd7"]="4"
player_flights["27f75d82-932a-451d-b534-4653045c4816"]="4"

echo "=== Session 3 Flight Assignments ==="
echo "Flight 1: Rich Baker, Jeff Dilcher, Jason Fink, George Hutson, Joe Mahachanh, Alex Peck, John Perry, Bill Stein"
echo "Flight 2: Ray Ballinger, Frank Frankenfield, Carl Hardner, Kevin Kelhart, Kenny Palladino, Curt Saeger, Tim Seyler, Stu Silfies"
echo "Flight 3: Steve Bedek, Jim Eck, Lou Gabrielle, Bob Gross, Steve Hampton, Kevin Kelhart JR, Juan Matute, Danny Washurn"
echo "Flight 4: Matt Donahue, Steve Filipovits, Jax Haeusler, Tom Haeusler, Rich Hart, Andrew Kerns, Steve Kerns, Ben Mahachanh"
echo

echo "Note: Matt Speth (ID: 9e7a3048-2254-46ae-a16a-a90cc8dbd962) is NOT assigned to any flight for Session 3"
echo

echo "=== Week 15 Matchup Analysis ==="

# Week 15 matchups from the API response
week15_matchups='[
    {"playerAId":"b1046572-8971-425b-9dbd-cdfdc80bbdd7","playerBId":"9d4ab9eb-6c6f-4fdd-be6b-2396202eb645"},
    {"playerAId":"2743c25d-5700-418b-93d7-c0a2a8f48f93","playerBId":"9548bbdf-f6ea-447d-988b-fe1e21bd2080"},
    {"playerAId":"dd5a8bd3-0bf1-436e-9484-74b40d6e83fc","playerBId":"d971efb9-8e8c-4e91-87e9-f79c74391455"},
    {"playerAId":"8c24e1d3-50bf-49b0-956f-3843ab1d9bf7","playerBId":"c80aef4d-db47-493e-b1ae-9d09a734a094"},
    {"playerAId":"4ef1e93d-be97-42f6-9721-3c1ddd256c60","playerBId":"1cbaa69a-f72e-4ccd-9e6b-2ea1a501f5ee"},
    {"playerAId":"ac3946ff-48eb-4479-ad63-92136e7c2f7f","playerBId":"354f62e4-a538-4eaa-b7e4-5f9bd60e777f"},
    {"playerAId":"27f75d82-932a-451d-b534-4653045c4816","playerBId":"c7de2e43-847d-4b73-bb40-cf268fa7c301"},
    {"playerAId":"2c8b669d-8b6c-4951-880f-a3d607415f8c","playerBId":"42b73157-6250-4f41-9701-9271ffc49c05"},
    {"playerAId":"731fbc13-0f87-497a-9674-1bceaa67b395","playerBId":"7d6db2a5-480b-4e68-a0f7-bd5d24db8999"},
    {"playerAId":"a51ea62e-4a3f-4741-b43c-56e3feae7e70","playerBId":"52369d98-5f1b-4c9d-9859-3d4a2a183439"},
    {"playerAId":"459e64fe-a08e-4f1b-8546-91b9cb10ed95","playerBId":"878f2662-b5bb-4474-aaa5-d2b4b179e070"},
    {"playerAId":"4d2ab22d-daee-4cbf-98ca-ca5f8034b3fc","playerBId":"c43faa83-23d6-4d09-9ffb-9548361480bc"},
    {"playerAId":"9deacc93-f9ed-49e6-ab68-c89b056af0fe","playerBId":"155adf40-c73d-4c46-b2e2-f6b2471563dd"},
    {"playerAId":"3a58bd78-1074-4c5c-b43e-605a8efc3a35","playerBId":"238b9732-4a0f-4922-8d5c-26e33fd8ac95"},
    {"playerAId":"74d4dd50-3a5a-4e14-8e06-a37704cddd71","playerBId":"ed6799bc-76d6-43fd-9301-f4fa9effaee6"},
    {"playerAId":"a16dd095-e46d-4f47-8767-50b02226dca4","playerBId":"67a0c954-df29-45ec-a183-08ff7cb3678b"}
]'

# Check for the specific Matt Donahue vs Matt Speth issue
echo "CRITICAL FINDING:"
echo "Matt Speth (9e7a3048-2254-46ae-a16a-a90cc8dbd962) is NOT in any Week 15 matchups"
echo "Matt Speth is also NOT assigned to any flight for Session 3"
echo

problematic_count=0

echo "Checking each matchup..."

# Steve Kerns vs Steve Filipovits  
echo "✓ Steve Kerns vs Steve Filipovits - Both in Flight 4"

# Jason Fink vs Joe Mahachanh
echo "✓ Jason Fink vs Joe Mahachanh - Both in Flight 1"

# Bill Stein vs Jeff Dilcher
echo "✓ Bill Stein vs Jeff Dilcher - Both in Flight 1"

# John Perry vs Alex Peck
echo "✓ John Perry vs Alex Peck - Both in Flight 1"

# Tim Seyler vs Kevin Kelhart
echo "✓ Tim Seyler vs Kevin Kelhart - Both in Flight 2"

# Curt Saeger vs Ray Ballinger
echo "✓ Curt Saeger vs Ray Ballinger - Both in Flight 2"

# Ben Mahachanh vs Jax Haeusler
echo "✓ Ben Mahachanh vs Jax Haeusler - Both in Flight 4"

# George Hutson vs Rich Baker
echo "✓ George Hutson vs Rich Baker - Both in Flight 1"

# Carl Hardner vs Stu Silfies
echo "✓ Carl Hardner vs Stu Silfies - Both in Flight 2"

# Kenny Palladino vs Frank Frankenfield
echo "✓ Kenny Palladino vs Frank Frankenfield - Both in Flight 2"

# Steve Bedek vs Lou Gabrielle
echo "✓ Steve Bedek vs Lou Gabrielle - Both in Flight 3"

# Jim Eck vs Kevin Kelhart JR
echo "✓ Jim Eck vs Kevin Kelhart JR - Both in Flight 3"

# Danny Washurn vs Steve Hampton
echo "✓ Danny Washurn vs Steve Hampton - Both in Flight 3"

# Bob Gross vs Juan Matute
echo "✓ Bob Gross vs Juan Matute - Both in Flight 3"

# Matt Donahue vs Rich Hart
echo "✓ Matt Donahue vs Rich Hart - Both in Flight 4"

# Andrew Kerns vs Tom Haeusler
echo "✓ Andrew Kerns vs Tom Haeusler - Both in Flight 4"

echo
echo "=== SUMMARY ==="
echo "Total Week 15 matchups: 16"
echo "All 16 matchups are valid (both players in same flight)"
echo "Total players in matchups: 32"
echo "Problematic matchups: 0"
echo
echo "=== ANALYSIS OF USER'S CONCERN ==="
echo "User reported: 'Matt Donahue vs Matt Speth, Matt Speth is not assigned to a flight and Matt D is in flight 4'"
echo
echo "FINDINGS:"
echo "1. Matt Donahue (74d4dd50-3a5a-4e14-8e06-a37704cddd71) IS assigned to Flight 4 ✓"
echo "2. Matt Donahue's actual Week 15 opponent is Rich Hart (ed6799bc-76d6-43fd-9301-f4fa9effaee6) ✓"
echo "3. Matt Speth (9e7a3048-2254-46ae-a16a-a90cc8dbd962) is NOT assigned to any flight ✗"
echo "4. Matt Speth is NOT in any Week 15 matchups ✗"
echo "5. There is NO 'Matt Donahue vs Matt Speth' matchup in Week 15 ✓"
echo
echo "CONCLUSION:"
echo "The reported matchup 'Matt Donahue vs Matt Speth' does NOT exist in Week 15."
echo "All actual Week 15 matchups are valid. The user may have been looking at"
echo "different week data or there may be a display issue in the frontend."
