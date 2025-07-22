#!/usr/bin/env python3
"""
Season Matchup Generator for Golf League Manager
Generates matchups for an entire season using various strategies
"""

import argparse
import psycopg2
import json
import random
from itertools import combinations
from collections import defaultdict
from typing import List, Dict, Set, Tuple, Optional
import uuid
from datetime import datetime

# Database connection settings
DB_CONFIG = {
    'host': 'localhost',
    'port': '5432',
    'user': 'golfuser',
    'password': 'golfpassword'
}

class MatchupGenerator:
    def __init__(self, database_name: str):
        self.database_name = database_name
        self.conn = None
        
    def connect(self):
        """Connect to the database"""
        try:
            self.conn = psycopg2.connect(
                database=self.database_name,
                **DB_CONFIG
            )
            print(f"✅ Connected to database: {self.database_name}")
        except Exception as e:
            print(f"❌ Failed to connect to database: {e}")
            raise
            
    def disconnect(self):
        """Disconnect from the database"""
        if self.conn:
            self.conn.close()
            print("✅ Database connection closed")
    
    def get_seasons(self) -> List[Dict]:
        """Get all seasons"""
        with self.conn.cursor() as cur:
            cur.execute('SELECT "Id", "Name", "Year", "StartDate", "EndDate" FROM "Seasons" ORDER BY "Year" DESC, "SeasonNumber" DESC')
            return [
                {
                    'id': row[0],
                    'name': row[1],
                    'year': row[2],
                    'start_date': row[3],
                    'end_date': row[4]
                }
                for row in cur.fetchall()
            ]
    
    def get_weeks_for_season(self, season_id: str) -> List[Dict]:
        """Get all weeks for a season"""
        with self.conn.cursor() as cur:
            cur.execute('''
                SELECT "Id", "WeekNumber", "Name", "Date", "IsActive"
                FROM "Weeks" 
                WHERE "SeasonId" = %s 
                ORDER BY "WeekNumber"
            ''', (season_id,))
            
            return [
                {
                    'id': row[0],
                    'week_number': row[1],
                    'name': row[2],
                    'date': row[3],
                    'is_active': row[4]
                }
                for row in cur.fetchall()
            ]
    
    def get_flights_for_season(self, season_id: str) -> List[Dict]:
        """Get all flights for a season"""
        with self.conn.cursor() as cur:
            cur.execute('''
                SELECT "Id", "Name", "Description", "MaxPlayers", "IsActive"
                FROM "Flights" 
                WHERE "SeasonId" = %s AND "IsActive" = true
                ORDER BY "Name"
            ''', (season_id,))
            
            return [
                {
                    'id': row[0],
                    'name': row[1],
                    'description': row[2],
                    'max_players': row[3],
                    'is_active': row[4]
                }
                for row in cur.fetchall()
            ]
    
    def get_players_in_flight(self, flight_id: str) -> List[Dict]:
        """Get all players assigned to a flight"""
        with self.conn.cursor() as cur:
            cur.execute('''
                SELECT p."Id", p."FirstName", p."LastName"
                FROM "Players" p
                JOIN "PlayerFlightAssignments" pfa ON p."Id" = pfa."PlayerId"
                WHERE pfa."FlightId" = %s
                ORDER BY p."LastName", p."FirstName"
            ''', (flight_id,))
            
            return [
                {
                    'id': row[0],
                    'first_name': row[1],
                    'last_name': row[2],
                    'full_name': f"{row[1]} {row[2]}"
                }
                for row in cur.fetchall()
            ]
    
    def clear_existing_matchups(self, season_id: str) -> int:
        """Clear all existing matchups for a season"""
        with self.conn.cursor() as cur:
            cur.execute('''
                DELETE FROM "Matchups" 
                WHERE "WeekId" IN (
                    SELECT "Id" FROM "Weeks" WHERE "SeasonId" = %s
                )
            ''', (season_id,))
            
            deleted_count = cur.rowcount
            self.conn.commit()
            print(f"🗑️  Cleared {deleted_count} existing matchups for season")
            return deleted_count
    
    def create_matchup(self, week_id: str, player_a_id: str, player_b_id: str) -> str:
        """Create a single matchup"""
        matchup_id = str(uuid.uuid4())
        
        with self.conn.cursor() as cur:
            cur.execute('''
                INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
                VALUES (%s, %s, %s, %s)
            ''', (matchup_id, week_id, player_a_id, player_b_id))
        
        return matchup_id
    
    def generate_round_robin_matchups(self, players: List[Dict], weeks: List[Dict]) -> List[Tuple[str, str, str]]:
        """
        Generate round robin matchups where every player plays every other player exactly once
        Returns list of (week_id, player_a_id, player_b_id) tuples
        """
        matchups = []
        all_pairings = list(combinations([p['id'] for p in players], 2))
        
        if len(all_pairings) > len(weeks):
            print(f"⚠️  Warning: {len(all_pairings)} pairings needed but only {len(weeks)} weeks available")
            print(f"   Some players won't play everyone in round robin format")
        
        # Distribute pairings across weeks
        for i, (player_a_id, player_b_id) in enumerate(all_pairings):
            if i < len(weeks):
                week_id = weeks[i]['id']
                matchups.append((week_id, player_a_id, player_b_id))
        
        return matchups
    
    def generate_random_weekly_matchups(self, players: List[Dict], weeks: List[Dict]) -> List[Tuple[str, str, str]]:
        """
        Generate random matchups for each week ensuring each player plays once per week
        Returns list of (week_id, player_a_id, player_b_id) tuples
        """
        matchups = []
        player_ids = [p['id'] for p in players]
        
        for week in weeks:
            week_matchups = self._generate_weekly_pairings(player_ids)
            for player_a_id, player_b_id in week_matchups:
                matchups.append((week['id'], player_a_id, player_b_id))
        
        return matchups
    
    def generate_balanced_matchups(self, players: List[Dict], weeks: List[Dict]) -> List[Tuple[str, str, str]]:
        """
        Generate balanced matchups trying to ensure players play each other roughly equally
        Returns list of (week_id, player_a_id, player_b_id) tuples
        """
        matchups = []
        player_ids = [p['id'] for p in players]
        
        # Track how many times each pair has played
        pairing_count = defaultdict(int)
        
        for week in weeks:
            week_matchups = self._generate_balanced_weekly_pairings(player_ids, pairing_count)
            for player_a_id, player_b_id in week_matchups:
                matchups.append((week['id'], player_a_id, player_b_id))
                # Update pairing count
                pair_key = tuple(sorted([player_a_id, player_b_id]))
                pairing_count[pair_key] += 1
        
        return matchups
    
    def _generate_weekly_pairings(self, player_ids: List[str]) -> List[Tuple[str, str]]:
        """Generate random pairings for a single week"""
        available_players = player_ids.copy()
        random.shuffle(available_players)
        
        pairings = []
        for i in range(0, len(available_players) - 1, 2):
            pairings.append((available_players[i], available_players[i + 1]))
        
        return pairings
    
    def _generate_balanced_weekly_pairings(self, player_ids: List[str], pairing_count: Dict) -> List[Tuple[str, str]]:
        """Generate balanced pairings for a single week"""
        available_players = player_ids.copy()
        pairings = []
        
        while len(available_players) >= 2:
            # Find the player who has played the least
            player_a = available_players[0]
            
            # Find the best opponent for player_a (least played against)
            best_opponent = None
            min_plays = float('inf')
            
            for potential_opponent in available_players[1:]:
                pair_key = tuple(sorted([player_a, potential_opponent]))
                plays = pairing_count.get(pair_key, 0)
                if plays < min_plays:
                    min_plays = plays
                    best_opponent = potential_opponent
            
            if best_opponent:
                pairings.append((player_a, best_opponent))
                available_players.remove(player_a)
                available_players.remove(best_opponent)
            else:
                break
        
        return pairings
    
    def generate_season_matchups(self, season_id: str, strategy: str = "balanced", clear_existing: bool = True) -> Dict:
        """
        Generate matchups for an entire season
        
        Args:
            season_id: The season ID to generate matchups for
            strategy: "random", "balanced", or "round_robin"
            clear_existing: Whether to clear existing matchups first
        
        Returns:
            Dictionary with generation results
        """
        print(f"🎯 Starting season matchup generation...")
        print(f"   Strategy: {strategy}")
        print(f"   Clear existing: {clear_existing}")
        
        # Clear existing matchups if requested
        cleared_count = 0
        if clear_existing:
            cleared_count = self.clear_existing_matchups(season_id)
        
        # Get weeks for the season
        weeks = self.get_weeks_for_season(season_id)
        if not weeks:
            raise ValueError(f"No weeks found for season {season_id}")
        
        print(f"📅 Found {len(weeks)} weeks in season")
        
        # Get flights for the season
        flights = self.get_flights_for_season(season_id)
        if not flights:
            raise ValueError(f"No flights found for season {season_id}")
        
        print(f"✈️  Found {len(flights)} flights in season")
        
        total_matchups_created = 0
        flight_results = []
        
        # Generate matchups for each flight
        for flight in flights:
            print(f"\n🏌️ Processing flight: {flight['name']}")
            
            # Get players in this flight
            players = self.get_players_in_flight(flight['id'])
            if len(players) < 2:
                print(f"⚠️  Skipping flight {flight['name']} - only {len(players)} players")
                continue
            
            print(f"   Players: {len(players)}")
            for player in players:
                print(f"     - {player['full_name']}")
            
            # Generate matchups based on strategy
            if strategy == "round_robin":
                matchups = self.generate_round_robin_matchups(players, weeks)
            elif strategy == "random":
                matchups = self.generate_random_weekly_matchups(players, weeks)
            elif strategy == "balanced":
                matchups = self.generate_balanced_matchups(players, weeks)
            else:
                raise ValueError(f"Unknown strategy: {strategy}")
            
            # Create the matchups in the database
            flight_matchups_created = 0
            for week_id, player_a_id, player_b_id in matchups:
                try:
                    self.create_matchup(week_id, player_a_id, player_b_id)
                    flight_matchups_created += 1
                except Exception as e:
                    print(f"❌ Failed to create matchup: {e}")
            
            total_matchups_created += flight_matchups_created
            
            flight_results.append({
                'flight_name': flight['name'],
                'players_count': len(players),
                'matchups_created': flight_matchups_created
            })
            
            print(f"   ✅ Created {flight_matchups_created} matchups for flight {flight['name']}")
        
        # Commit all changes
        self.conn.commit()
        
        result = {
            'season_id': season_id,
            'strategy': strategy,
            'weeks_count': len(weeks),
            'flights_count': len(flights),
            'cleared_matchups': cleared_count,
            'total_matchups_created': total_matchups_created,
            'flight_results': flight_results
        }
        
        print(f"\n🎉 Season matchup generation complete!")
        print(f"   Total matchups created: {total_matchups_created}")
        
        return result
    
    def analyze_season_matchups(self, season_id: str) -> Dict:
        """Analyze the matchup distribution for a season"""
        print(f"📊 Analyzing season matchups...")
        
        with self.conn.cursor() as cur:
            # Get total matchups per week
            cur.execute('''
                SELECT w."WeekNumber", w."Name", COUNT(m."Id") as matchup_count
                FROM "Weeks" w
                LEFT JOIN "Matchups" m ON w."Id" = m."WeekId"
                WHERE w."SeasonId" = %s
                GROUP BY w."Id", w."WeekNumber", w."Name"
                ORDER BY w."WeekNumber"
            ''', (season_id,))
            
            week_stats = []
            for row in cur.fetchall():
                week_stats.append({
                    'week_number': row[0],
                    'week_name': row[1],
                    'matchup_count': row[2]
                })
            
            # Get player matchup counts
            cur.execute('''
                SELECT 
                    p."FirstName" || ' ' || p."LastName" as player_name,
                    COUNT(m."Id") as matchup_count
                FROM "Players" p
                LEFT JOIN "Matchups" m ON (p."Id" = m."PlayerAId" OR p."Id" = m."PlayerBId")
                LEFT JOIN "Weeks" w ON m."WeekId" = w."Id"
                WHERE w."SeasonId" = %s OR w."SeasonId" IS NULL
                GROUP BY p."Id", player_name
                ORDER BY matchup_count DESC, player_name
            ''', (season_id,))
            
            player_stats = []
            for row in cur.fetchall():
                player_stats.append({
                    'player_name': row[0],
                    'matchup_count': row[1]
                })
            
            # Get total stats
            cur.execute('''
                SELECT COUNT(DISTINCT m."Id") as total_matchups
                FROM "Matchups" m
                JOIN "Weeks" w ON m."WeekId" = w."Id"
                WHERE w."SeasonId" = %s
            ''', (season_id,))
            
            total_matchups = cur.fetchone()[0]
        
        return {
            'season_id': season_id,
            'total_matchups': total_matchups,
            'week_stats': week_stats,
            'player_stats': player_stats
        }

def main():
    parser = argparse.ArgumentParser(description="Generate matchups for a golf league season")
    parser.add_argument('tenant', help='Tenant name (e.g., southmoore)')
    parser.add_argument('--season-id', help='Season ID (if not provided, will list available seasons)')
    parser.add_argument('--strategy', choices=['random', 'balanced', 'round_robin'], 
                       default='balanced', help='Matchup generation strategy')
    parser.add_argument('--no-clear', action='store_true', 
                       help='Do not clear existing matchups (append mode)')
    parser.add_argument('--analyze-only', action='store_true',
                       help='Only analyze existing matchups, do not generate new ones')
    parser.add_argument('--list-seasons', action='store_true',
                       help='List available seasons and exit')
    
    args = parser.parse_args()
    
    # Construct database name
    database_name = f"golfdb_{args.tenant}"
    
    # Initialize generator
    generator = MatchupGenerator(database_name)
    
    try:
        generator.connect()
        
        # List seasons if requested
        if args.list_seasons or not args.season_id:
            seasons = generator.get_seasons()
            print("\n📋 Available seasons:")
            print("-" * 80)
            print(f"{'ID':<38} {'Name':<20} {'Year':<6} {'Start Date':<12} {'End Date'}")
            print("-" * 80)
            for season in seasons:
                print(f"{season['id']:<38} {season['name']:<20} {season['year']:<6} "
                     f"{season['start_date'].strftime('%Y-%m-%d'):<12} "
                     f"{season['end_date'].strftime('%Y-%m-%d')}")
            
            if args.list_seasons:
                return
            
            if not args.season_id:
                print("\nPlease specify --season-id with one of the IDs above")
                return
        
        # Analyze existing matchups if requested
        if args.analyze_only:
            result = generator.analyze_season_matchups(args.season_id)
            
            print(f"\n📊 Season Analysis Results")
            print(f"Season ID: {result['season_id']}")
            print(f"Total Matchups: {result['total_matchups']}")
            
            print(f"\n📅 Matchups per Week:")
            for week in result['week_stats']:
                print(f"  Week {week['week_number']:2d} ({week['week_name']:<15}): {week['matchup_count']:2d} matchups")
            
            print(f"\n👥 Player Matchup Counts:")
            for player in result['player_stats']:
                print(f"  {player['player_name']:<25}: {player['matchup_count']:2d} matchups")
            
        else:
            # Generate matchups
            result = generator.generate_season_matchups(
                season_id=args.season_id,
                strategy=args.strategy,
                clear_existing=not args.no_clear
            )
            
            print(f"\n✅ Generation Summary:")
            print(f"   Season ID: {result['season_id']}")
            print(f"   Strategy: {result['strategy']}")
            print(f"   Weeks: {result['weeks_count']}")
            print(f"   Flights: {result['flights_count']}")
            print(f"   Cleared existing: {result['cleared_matchups']}")
            print(f"   Total matchups created: {result['total_matchups_created']}")
            
            print(f"\n📊 Flight Results:")
            for flight_result in result['flight_results']:
                print(f"   {flight_result['flight_name']:<15}: "
                     f"{flight_result['players_count']:2d} players, "
                     f"{flight_result['matchups_created']:3d} matchups")
    
    except Exception as e:
        print(f"❌ Error: {e}")
        raise
    
    finally:
        generator.disconnect()

if __name__ == "__main__":
    main()
