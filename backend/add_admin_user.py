import hashlib
import base64
import psycopg2
import uuid

# Settings from docker-compose
PGUSER = 'golfuser'
PGPASSWORD = 'golfpassword'
PGDATABASE = 'golfdb'
PGHOST = 'localhost'
PGPORT = 5432

# User details
USERNAME = 'admin'
PASSWORD = 'golfpassword'  # same as POSTGRES_PASSWORD for demo
IS_ADMIN = True

# Hash password using the same method as AuthController
sha = hashlib.sha256()
sha.update(PASSWORD.encode('utf-8'))
hash_bytes = sha.digest()
password_hash = base64.b64encode(hash_bytes).decode('utf-8')

# Connect to Postgres
conn = psycopg2.connect(
    dbname=PGDATABASE,
    user=PGUSER,
    password=PGPASSWORD,
    host=PGHOST,
    port=PGPORT
)
cur = conn.cursor()

user_id = str(uuid.uuid4())

cur.execute('''
    INSERT INTO "Users" ("Id", "Username", "PasswordHash", "IsAdmin")
    VALUES (%s, %s, %s, %s)
    ON CONFLICT ("Username") DO NOTHING;
''', (user_id, USERNAME, password_hash, IS_ADMIN))

conn.commit()
cur.close()
conn.close()

print(f"User '{USERNAME}' added with admin rights.")
