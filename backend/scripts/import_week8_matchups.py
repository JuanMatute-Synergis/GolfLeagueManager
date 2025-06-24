import csv
import psycopg2

# Database connection info from docker-compose.yml
DB_NAME = 'golfdb'
DB_USER = 'golfuser'
DB_PASSWORD = 'golfpassword'
DB_HOST = 'localhost'
DB_PORT = 5432

# Week 8 info
WEEK_ID = '73ac012e-8eea-48d8-b40f-9fabca024d68'
CSV_PATH = 'scripts/data/schedule_with_names_corrected.csv'

# Helper: get player id by name
def get_player_id(conn, name):
    first, last = name.strip().split(' ', 1)
    with conn.cursor() as cur:
        cur.execute('SELECT "Id" FROM "Players" WHERE "FirstName"=%s AND "LastName"=%s', (first, last))
        row = cur.fetchone()
        if row:
            return row[0]
        else:
            raise Exception(f'Player not found: {name}')

def main():
    conn = psycopg2.connect(dbname=DB_NAME, user=DB_USER, password=DB_PASSWORD, host=DB_HOST, port=DB_PORT)
    with open(CSV_PATH, newline='') as csvfile:
        reader = csv.DictReader(csvfile)
        for row in reader:
            if row['Week'] != '8':
                continue
            matchup = row['Matchup']
            try:
                player_a, player_b = [n.strip() for n in matchup.split('vs')]
                player_a_id = get_player_id(conn, player_a)
                player_b_id = get_player_id(conn, player_b)
            except Exception as e:
                print(f'Error parsing matchup {matchup}: {e}')
                continue
            # Check if matchup already exists for this week and players (either order)
            with conn.cursor() as cur:
                cur.execute('''
                    SELECT 1 FROM "Matchups" WHERE "WeekId"=%s AND (("PlayerAId"=%s AND "PlayerBId"=%s) OR ("PlayerAId"=%s AND "PlayerBId"=%s))
                ''', (WEEK_ID, player_a_id, player_b_id, player_b_id, player_a_id))
                if cur.fetchone():
                    print(f'Already exists: {player_a} vs {player_b} (week 8)')
                    continue
                cur.execute('''
                    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
                    VALUES (gen_random_uuid(), %s, %s, %s)
                ''', (WEEK_ID, player_a_id, player_b_id))
                print(f'Inserted: {player_a} vs {player_b} (week 8)')
        conn.commit()
    conn.close()

if __name__ == '__main__':
    main()
