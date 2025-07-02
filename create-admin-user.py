#!/usr/bin/env python3

import sys
import psycopg2
import hashlib
import base64
import uuid
from datetime import datetime

def hash_password(password):
    """Hash password using SHA256 to match AuthController"""
    sha = hashlib.sha256()
    sha.update(password.encode('utf-8'))
    hash_bytes = sha.digest()
    return base64.b64encode(hash_bytes).decode('utf-8')

def create_admin_user(tenant_name):
    """Create an admin user for the specified tenant database"""
    
    db_name = f"golfdb_{tenant_name}"
    connection_string = f"host=localhost port=5432 dbname={db_name} user=golfuser password=golfpassword"
    
    try:
        # Connect to the tenant database
        conn = psycopg2.connect(connection_string)
        cur = conn.cursor()
        
        # Check if admin user already exists
        cur.execute('SELECT "Id" FROM "Users" WHERE "Username" = %s', ('admin',))
        existing_user = cur.fetchone()
        
        if existing_user:
            print(f"Admin user already exists in {db_name}")
            return True
        
        # Hash the password using SHA256 to match AuthController
        password = "golfpassword"
        password_hash = hash_password(password)
        
        # Generate UUID for the admin user
        user_id = str(uuid.uuid4())
        created_at = datetime.now()
        
        # Insert admin user
        insert_query = '''
            INSERT INTO "Users" ("Id", "Username", "PasswordHash", "IsAdmin")
            VALUES (%s, %s, %s, %s)
        '''
        
        cur.execute(insert_query, (
            user_id,
            'admin',
            password_hash,
            True
        ))
        
        conn.commit()
        print(f"✅ Created admin user for tenant '{tenant_name}' in database '{db_name}'")
        print(f"   Username: admin")
        print(f"   Password: golfpassword")
        print(f"   User ID: {user_id}")
        
    except psycopg2.Error as e:
        print(f"❌ Database error: {e}")
        import traceback
        traceback.print_exc()
        return False
    except Exception as e:
        print(f"❌ Error creating admin user: {e}")
        import traceback
        traceback.print_exc()
        return False
    finally:
        if conn:
            conn.close()
    
    return True

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python3 create-admin-user.py <tenant_name>")
        sys.exit(1)
    
    tenant_name = sys.argv[1]
    
    if not create_admin_user(tenant_name):
        sys.exit(1)
