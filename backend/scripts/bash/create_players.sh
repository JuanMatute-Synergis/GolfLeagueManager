#!/bin/bash

# Array of first names
first_names=("John" "Emma" "Michael" "Olivia" "William" "Sophia" "James" "Isabella" "Benjamin" "Charlotte" "Lucas" "Amelia" "Henry" "Mia" "Alexander" "Harper" "Mason" "Evelyn" "Ethan" "Abigail" "Daniel" "Emily" "Matthew" "Elizabeth" "Anthony" "Sofia" "Christopher" "Avery" "Joshua" "Ella" "Andrew" "Scarlett" "Ryan" "Grace" "Noah" "Chloe" "Tyler" "Camila" "Jackson" "Aria")

# Array of last names
last_names=("Smith" "Johnson" "Williams" "Brown" "Jones" "Garcia" "Miller" "Davis" "Rodriguez" "Martinez" "Hernandez" "Lopez" "Gonzalez" "Wilson" "Anderson" "Thomas" "Taylor" "Moore" "Jackson" "Martin" "Lee" "Perez" "Thompson" "White" "Harris" "Sanchez" "Clark" "Ramirez" "Lewis" "Robinson" "Walker" "Young" "Allen" "King" "Wright" "Scott" "Torres" "Nguyen" "Hill" "Flores")

# API URL
API_URL="http://localhost:5274/api/players"

# Function to create a random email
create_email() {
    local first_name=$1
    local last_name=$2
    echo "${first_name,,}.${last_name,,}@example.com"
}

# Function to create a random phone number
create_phone() {
    echo "$(shuf -i 100-999 -n 1)-$(shuf -i 100-999 -n 1)-$(shuf -i 1000-9999 -n 1)"
}

echo "Creating 20 random players..."
echo "================================"

# Create 20 players
for i in {1..20}; do
    # Get random first and last name
    first_name=${first_names[$RANDOM % ${#first_names[@]}]}
    last_name=${last_names[$RANDOM % ${#last_names[@]}]}
    
    # Create email and phone
    email=$(create_email "$first_name" "$last_name")
    phone=$(create_phone)
    
    # Create JSON payload
    json_payload=$(cat <<EOF
{
  "firstName": "$first_name",
  "lastName": "$last_name",
  "email": "$email",
  "phone": "$phone"
}
EOF
)
    
    echo "Creating player $i: $first_name $last_name"
    
    # Make API call
    response=$(curl -s -X POST "$API_URL" \
        -H "Content-Type: application/json" \
        -d "$json_payload")
    
    # Check if successful (response should contain an id)
    if echo "$response" | grep -q '"id"'; then
        echo "✓ Successfully created: $first_name $last_name"
    else
        echo "✗ Failed to create: $first_name $last_name"
        echo "Response: $response"
    fi
    
    # Small delay to avoid overwhelming the API
    sleep 0.1
done

echo "================================"
echo "Finished creating players!"
echo ""
echo "Fetching all players to verify:"
curl -s -X GET "$API_URL" | jq '.[] | "\(.firstName) \(.lastName) - \(.email)"' || curl -s -X GET "$API_URL"
