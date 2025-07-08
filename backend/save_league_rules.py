#!/usr/bin/env python3
"""
Script to save comprehensive league rules to the PostgreSQL database
"""

import psycopg2
import json
import sys
from datetime import datetime
import uuid

# Comprehensive league rules content
LEAGUE_RULES_CONTENT = """
<h1>🏌️ Golf League Rules &amp; Scoring System</h1>

<h2>📋 Overview</h2>
<p>This golf league operates on a <strong>Match Play format</strong> using a <strong>Simple Average Handicap System</strong> for fair competition across all skill levels. The league emphasizes sportsmanship, consistency, and competitive play through a structured points-based system.</p>

<hr>

<h2>🏌️ Handicap System</h2>

<h3>Simple Average Handicap</h3>
<ul>
<li><strong>Calculation Method</strong>: Player handicaps are calculated using a simple average of their gross scores from previous rounds</li>
<li><strong>Handicap Updates</strong>: Handicaps are recalculated weekly based on all completed rounds in the current season</li>
<li><strong>Initial Handicap</strong>: New players receive an estimated handicap based on their first few rounds</li>
<li><strong>Handicap Application</strong>: Handicaps are applied hole-by-hole using the course's stroke index (difficulty rating)</li>
</ul>

<h3>Stroke Allocation</h3>
<ul>
<li>Strokes are given on holes based on the <strong>hole handicap</strong> (stroke index) from hardest (1) to easiest (9)</li>
<li>If a player's handicap is 9, they receive one stroke on the 9 most difficult holes</li>
<li>If handicap difference between players is 5, the higher-handicap player gets strokes on the 5 hardest holes</li>
</ul>

<hr>

<h2>⚡ Match Play Scoring System</h2>

<h3>Hole-by-Hole Scoring</h3>
<ul>
<li><strong>2 Points</strong> awarded to the player with the lowest <strong>net score</strong> on each hole</li>
<li><strong>1 Point</strong> awarded to each player in case of a <strong>tie</strong> on any hole</li>
<li><strong>0 Points</strong> awarded to the player with the higher net score</li>
</ul>

<h3>Match Completion Bonus</h3>
<ul>
<li><strong>2 Additional Points</strong> awarded to the player with the <strong>lowest total net score</strong> for the entire match</li>
<li>This bonus is awarded regardless of how many individual holes were won</li>
</ul>

<h3>Maximum Points Per Match</h3>
<ul>
<li><strong>20 Points Maximum</strong>: 18 points possible from holes (2 × 9 holes) + 2 points for net score winner</li>
<li><strong>Typical Distribution</strong>: Most matches will see point totals between 8-12 points per player</li>
</ul>

<hr>

<h2>📊 Example Scoring Scenarios</h2>

<h3>Scenario 1: Close Match</h3>
<ul>
<li>Player A wins 5 holes, Player B wins 4 holes</li>
<li>Player A hole points: 10, Player B hole points: 8</li>
<li>Player A has lower total net score: +2 bonus points</li>
<li><strong>Final Score</strong>: Player A: 12 points, Player B: 8 points</li>
</ul>

<h3>Scenario 2: Tied Holes</h3>
<ul>
<li>Player A wins 4 holes, Player B wins 3 holes, 2 holes tied</li>
<li>Player A hole points: 9, Player B hole points: 7</li>
<li>Player B has lower total net score: +2 bonus points</li>
<li><strong>Final Score</strong>: Player A: 9 points, Player B: 9 points</li>
</ul>

<hr>

<h2>🚫 Absence &amp; Special Circumstances</h2>

<h3>Player Absence Policies</h3>
<ul>
<li><strong>No Notice Absence</strong>: 0 points awarded</li>
<li><strong>Advance Notice Absence</strong>: Points equal to 20% of maximum possible points (typically 4 points)</li>
<li><strong>Both Players Absent</strong>: Points distributed based on who gave notice</li>
</ul>

<h3>Present Player Scoring (When Opponent is Absent)</h3>
<ul>
<li>Present player must complete their round and submit scores</li>
<li><strong>High Performance</strong>: 16 points awarded if player beats their average score by a whole number</li>
<li><strong>Standard Performance</strong>: 8 points awarded if player does not beat their average by a whole number</li>
<li><strong>No Match Bonus</strong>: The 2-point match winner bonus is not applicable in absence scenarios</li>
</ul>

<h3>Special Weeks</h3>
<ul>
<li>League may designate special weeks where predetermined points are awarded to all participants</li>
<li>Special circumstances (weather, course issues) may result in modified scoring at league discretion</li>
</ul>

<hr>

<h2>📈 Season Standings &amp; Statistics</h2>

<h3>Individual Rankings</h3>
<ul>
<li>Players ranked by <strong>total points accumulated</strong> throughout the season</li>
<li><strong>Average points per match</strong> used as tiebreaker for players with different numbers of matches</li>
<li><strong>Handicap improvement</strong> tracked and celebrated</li>
</ul>

<h3>Performance Metrics</h3>
<ul>
<li>Total points earned</li>
<li>Average points per match</li>
<li>Matches won vs. lost</li>
<li>Handicap progression</li>
<li>Attendance rate</li>
</ul>

<hr>

<h2>⚖️ Fair Play &amp; Sportsmanship</h2>

<h3>Score Integrity</h3>
<ul>
<li>All players must accurately record and verify scores</li>
<li>Questionable scores may be reviewed by league administrators</li>
<li>Deliberate score misrepresentation results in match forfeiture</li>
</ul>

<h3>Course Etiquette</h3>
<ul>
<li>Maintain pace of play</li>
<li>Repair divots and ball marks</li>
<li>Follow course rules and cart policies</li>
<li>Respect fellow competitors and course staff</li>
</ul>

<h3>Dispute Resolution</h3>
<ul>
<li>On-course disputes should be resolved between players when possible</li>
<li>Unresolved issues escalated to league administrators</li>
<li>Final decisions rest with league management</li>
</ul>

<hr>

<h2>📅 Administrative Details</h2>

<h3>Match Scheduling</h3>
<ul>
<li>Matches scheduled weekly during the active season</li>
<li>Players responsible for coordinating tee times within assigned week</li>
<li>Makeup matches allowed for legitimate scheduling conflicts</li>
</ul>

<h3>Score Submission</h3>
<ul>
<li>Scores must be submitted within 48 hours of match completion</li>
<li>Late submissions may result in point penalties</li>
<li>Digital score entry preferred for accuracy and timeliness</li>
</ul>

<h3>Communication</h3>
<ul>
<li>League updates communicated via official channels</li>
<li>Players expected to maintain current contact information</li>
<li>Emergency notifications sent for weather or course-related changes</li>
</ul>

<hr>

<h2>🏆 Season Format</h2>

<h3>Regular Season</h3>
<ul>
<li>Weekly match play throughout the golf season</li>
<li>Consistent pairings or rotating matchups based on league size</li>
<li>Points accumulate toward season championship</li>
</ul>

<h3>Playoffs/Championship</h3>
<ul>
<li>End-of-season tournament format may apply</li>
<li>Qualification based on regular season performance</li>
<li>Special championship scoring rules may apply</li>
</ul>

<hr>

<p><strong>This scoring system ensures competitive balance while rewarding both individual hole performance and overall round management. The simple average handicap system promotes improvement and maintains fair competition across all skill levels.</strong></p>
"""

def load_db_config():
    """Load database configuration from appsettings.json"""
    try:
        with open('appsettings.json', 'r') as f:
            config = json.load(f)
        
        conn_str = config['ConnectionStrings']['DefaultConnection']
        
        # Parse PostgreSQL connection string
        # Format: Host=192.168.6.67;Port=5432;Database=golfdb_southmoore;Username=golfuser;Password=golfpassword
        parts = conn_str.split(';')
        db_config = {}
        
        for part in parts:
            key, value = part.split('=', 1)
            if key.lower() == 'host':
                db_config['host'] = value
            elif key.lower() == 'port':
                db_config['port'] = int(value)
            elif key.lower() == 'database':
                db_config['database'] = value
            elif key.lower() == 'username':
                db_config['user'] = value
            elif key.lower() == 'password':
                db_config['password'] = value
        
        return db_config
    except Exception as e:
        print(f"Error loading database config: {e}")
        return None

def get_db_connection():
    """Get PostgreSQL database connection"""
    config = load_db_config()
    if not config:
        return None
    
    try:
        conn = psycopg2.connect(**config)
        return conn
    except Exception as e:
        print(f"Error connecting to database: {e}")
        return None

def get_seasons():
    """Get available seasons from the database"""
    conn = get_db_connection()
    if not conn:
        return []
    
    try:
        with conn.cursor() as cur:
            cur.execute("""
                SELECT "Id", "Name", "StartDate", "EndDate" 
                FROM "Seasons" 
                ORDER BY "StartDate" DESC
            """)
            
            seasons = []
            for row in cur.fetchall():
                seasons.append({
                    'id': str(row[0]),
                    'name': row[1],
                    'startDate': row[2].isoformat() if row[2] else None,
                    'endDate': row[3].isoformat() if row[3] else None
                })
            
            return seasons
    except Exception as e:
        print(f"Error getting seasons: {e}")
        return []
    finally:
        conn.close()

def save_rules_to_season(season_id):
    """Save rules to a specific season in PostgreSQL"""
    conn = get_db_connection()
    if not conn:
        return False
    
    try:
        with conn.cursor() as cur:
            # Check if rules already exist for this season
            cur.execute("""
                SELECT "Id" FROM "LeagueRules" 
                WHERE "SeasonId" = %s
            """, (season_id,))
            
            existing_rule = cur.fetchone()
            current_time = datetime.utcnow()
            
            if existing_rule:
                # Update existing rules
                cur.execute("""
                    UPDATE "LeagueRules" 
                    SET "Content" = %s, "UpdatedAt" = %s, "UpdatedBy" = %s
                    WHERE "SeasonId" = %s
                """, (LEAGUE_RULES_CONTENT, current_time, 'admin', season_id))
                print(f"✅ Updated existing rules for season {season_id}")
            else:
                # Insert new rules
                rule_id = str(uuid.uuid4())
                cur.execute("""
                    INSERT INTO "LeagueRules" ("Id", "SeasonId", "Content", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
                    VALUES (%s, %s, %s, %s, %s, %s, %s)
                """, (rule_id, season_id, LEAGUE_RULES_CONTENT, current_time, current_time, 'admin', 'admin'))
                print(f"✅ Created new rules for season {season_id}")
            
            conn.commit()
            return True
            
    except Exception as e:
        print(f"❌ Error saving rules: {e}")
        conn.rollback()
        return False
    finally:
        conn.close()

def main():
    print("🏌️ Golf League Rules Saver (PostgreSQL)")
    print("=" * 50)
    
    # Check database connection
    print("🔌 Testing database connection...")
    conn = get_db_connection()
    if not conn:
        print("❌ Unable to connect to PostgreSQL database.")
        print("Please check your appsettings.json file and ensure PostgreSQL is running.")
        sys.exit(1)
    else:
        print("✅ Database connection successful")
        conn.close()
    
    # Get available seasons
    print("\n📅 Getting available seasons...")
    seasons = get_seasons()
    
    if not seasons:
        print("❌ No seasons found in the database")
        print("You may need to create a season first using the Golf League Manager app.")
        sys.exit(1)
    
    print(f"✅ Found {len(seasons)} season(s)")
    
    # Display seasons
    for i, season in enumerate(seasons):
        season_name = season.get('name', f"Season {season.get('id', 'Unknown')}")
        start_date = season.get('startDate', 'Unknown')
        print(f"  {i+1}. {season_name} (Start: {start_date}) - ID: {season['id']}")
    
    # Let user choose a season or use the first one
    if len(seasons) == 1:
        selected_season = seasons[0]
        print(f"\n📝 Using the only available season: {selected_season.get('name', 'Unknown')}")
    else:
        try:
            choice = input(f"\nEnter season number (1-{len(seasons)}) or press Enter for first season: ").strip()
            if choice == "":
                selected_season = seasons[0]
            else:
                selected_season = seasons[int(choice) - 1]
        except (ValueError, IndexError):
            print("Invalid choice, using first season")
            selected_season = seasons[0]
    
    season_id = selected_season['id']
    season_name = selected_season.get('name', 'Unknown')
    
    print(f"\n💾 Saving comprehensive league rules to season: {season_name}")
    
    # Save rules directly to database
    success = save_rules_to_season(season_id)
    
    if success:
        print(f"\n🎉 Rules successfully saved to the PostgreSQL database!")
        print(f"📖 You can now view them in the Golf League Manager app under Rules")
        print(f"📄 Season: {season_name} (ID: {season_id})")
    else:
        print(f"\n⚠️  Rules were not saved successfully.")
        print(f"📋 Please check the error messages above and try again.")
        
    print(f"\n📊 Rules content includes:")
    print(f"   • Match Play scoring system (2 points per hole winner)")
    print(f"   • Simple Average handicap system")
    print(f"   • Absence policies and special circumstances")
    print(f"   • Administrative guidelines")
    print(f"   • Season format and standings")

if __name__ == "__main__":
    main()
