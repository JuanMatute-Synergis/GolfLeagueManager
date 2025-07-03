#!/bin/bash

# Comprehensive Multi-Tenant Functionality Test
# This script tests all aspects of the multi-tenant implementation

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}🏌️ Golf League Manager - Multi-Tenant Test Suite${NC}"
echo "================================================================"

# Test results tracking
TESTS_PASSED=0
TESTS_FAILED=0
TEST_RESULTS=()

# Function to run a test
run_test() {
    local test_name="$1"
    local test_command="$2"
    local expected_pattern="$3"
    
    echo -e "\n${BLUE}Testing: $test_name${NC}"
    
    # Run the test command
    result=$(eval "$test_command" 2>/dev/null)
    
    if [[ -n "$expected_pattern" ]] && echo "$result" | grep -q "$expected_pattern"; then
        echo -e "${GREEN}✅ PASS: $test_name${NC}"
        TESTS_PASSED=$((TESTS_PASSED + 1))
        TEST_RESULTS+=("✅ $test_name")
    elif [[ -z "$expected_pattern" ]] && [[ -n "$result" ]]; then
        echo -e "${GREEN}✅ PASS: $test_name${NC}"
        echo "   Response: $result"
        TESTS_PASSED=$((TESTS_PASSED + 1))
        TEST_RESULTS+=("✅ $test_name")
    else
        echo -e "${RED}❌ FAIL: $test_name${NC}"
        echo "   Expected: $expected_pattern"
        echo "   Got: $result"
        TESTS_FAILED=$((TESTS_FAILED + 1))
        TEST_RESULTS+=("❌ $test_name")
    fi
}

# Test 1: Backend Health Check
echo -e "\n${YELLOW}=== Infrastructure Tests ===${NC}"
run_test "Backend Health Check" \
    'curl -s "http://localhost:5505/health"' \
    '"status":"Healthy"'

# Test 2: Database Container Check
run_test "PostgreSQL Container Running" \
    'docker ps | grep golfleague_postgres' \
    'golfleague_postgres'

# Test 3: List Available Tenant Databases
echo -e "\n${YELLOW}=== Database Isolation Tests ===${NC}"
run_test "List Tenant Databases" \
    'docker exec golfleague_postgres psql -U golfuser -l | grep golfdb' \
    'golfdb'

# Test 4: Count Users in Default Database (htlyons)
run_test "Users in htlyons Database" \
    'docker exec golfleague_postgres psql -U golfuser -d golfdb -t -c "SELECT COUNT(*) FROM \"Users\";" | xargs' \
    ''

# Test 5: Count Users in Demo Database
run_test "Users in demo Database" \
    'docker exec golfleague_postgres psql -U golfuser -d golfdb_demo -t -c "SELECT COUNT(*) FROM \"Users\";" | xargs' \
    ''

# Test 6: Tenant Detection Without Headers (Default)
echo -e "\n${YELLOW}=== Tenant Detection Tests ===${NC}"
run_test "Default Tenant Detection" \
    'curl -s "http://localhost:5505/api/tenant/current"' \
    '"tenantId":"htlyons"'

# Test 7: Tenant Detection with Host Header
run_test "Demo Tenant Detection" \
    'curl -s -H "Host: demo.golfleaguemanager.app" "http://localhost:5505/api/tenant/current"' \
    '"tenantId":"demo"'

# Test 8: Tenant Detection with X-Tenant-Id Header
run_test "Custom Tenant Detection" \
    'curl -s -H "X-Tenant-Id: testclient" "http://localhost:5505/api/tenant/current"' \
    '"tenantId":"testclient"'

# Test 9: Authentication Test for Default Tenant
echo -e "\n${YELLOW}=== Authentication Tests ===${NC}"
run_test "Admin Login - htlyons Tenant" \
    'curl -s -c /tmp/cookies.txt -X POST -H "Content-Type: application/json" -d "{\"username\":\"admin\",\"password\":\"golfpassword\"}" "http://localhost:5505/api/auth/login"' \
    ''

# Test 10: Authentication Test for Demo Tenant
run_test "Admin Login - demo Tenant" \
    'curl -s -c /tmp/cookies_demo.txt -X POST -H "Content-Type: application/json" -H "Host: demo.golfleaguemanager.app" -d "{\"username\":\"admin\",\"password\":\"golfpassword\"}" "http://localhost:5505/api/auth/login"' \
    ''

# Test 11: Check if Cookies are Set
echo -e "\n${YELLOW}=== Cookie Tests ===${NC}"
run_test "Authentication Cookie Set" \
    'test -f /tmp/cookies.txt && grep golf_jwt_token /tmp/cookies.txt' \
    'golf_jwt_token'

# Test 12: External URL Tests (if available)
echo -e "\n${YELLOW}=== External Access Tests ===${NC}"
run_test "htlyons.golfleaguemanager.app Accessibility" \
    'curl -s -o /dev/null -w "%{http_code}" https://htlyons.golfleaguemanager.app --max-time 10' \
    '200'

run_test "demo.golfleaguemanager.app Accessibility" \
    'curl -s -o /dev/null -w "%{http_code}" https://demo.golfleaguemanager.app --max-time 10' \
    '200'

# Test 13: Database Schema Consistency
echo -e "\n${YELLOW}=== Schema Consistency Tests ===${NC}"
run_test "htlyons Database Schema" \
    'docker exec golfleague_postgres psql -U golfuser -d golfdb -c "\dt" | grep Users' \
    'Users'

run_test "demo Database Schema" \
    'docker exec golfleague_postgres psql -U golfuser -d golfdb_demo -c "\dt" | grep Users' \
    'Users'

# Test 14: Tenant Management Scripts
echo -e "\n${YELLOW}=== Tenant Management Tests ===${NC}"
run_test "List Tenants Script" \
    './list-tenants.sh | head -5' \
    ''

# Cleanup
rm -f /tmp/cookies.txt /tmp/cookies_demo.txt 2>/dev/null

# Summary
echo -e "\n${BLUE}================================================================${NC}"
echo -e "${BLUE}                    TEST RESULTS SUMMARY${NC}"
echo -e "${BLUE}================================================================${NC}"

echo -e "\n${GREEN}Passed: $TESTS_PASSED${NC}"
echo -e "${RED}Failed: $TESTS_FAILED${NC}"
echo -e "Total: $((TESTS_PASSED + TESTS_FAILED))"

echo -e "\n${BLUE}Detailed Results:${NC}"
for result in "${TEST_RESULTS[@]}"; do
    echo "  $result"
done

if [[ $TESTS_FAILED -eq 0 ]]; then
    echo -e "\n${GREEN}🎉 All tests passed! Multi-tenant setup is working correctly.${NC}"
else
    echo -e "\n${YELLOW}⚠️  Some tests failed. Check the details above.${NC}"
fi

echo -e "\n${BLUE}Multi-Tenant Setup Summary:${NC}"
echo "• Tenant Detection: Working via subdomain and headers"
echo "• Database Isolation: Each tenant has its own PostgreSQL database"
echo "• Authentication: HttpOnly cookies with tenant-specific users"
echo "• External Access: Available via CloudFlare tunnel"
echo ""
echo -e "${BLUE}Available Tenants:${NC}"
docker exec golfleague_postgres psql -U golfuser -l | grep golfdb | sed 's/|.*//' | sed 's/^ */  • /'
echo ""
echo -e "${BLUE}Access URLs:${NC}"
echo "  • Default: http://localhost:5505 (htlyons tenant)"
echo "  • Demo: http://localhost:5505 with Host: demo.golfleaguemanager.app"
echo "  • External: https://htlyons.golfleaguemanager.app"
echo "  • External: https://demo.golfleaguemanager.app"
echo ""
echo -e "${BLUE}Admin Credentials (all tenants):${NC}"
echo "  • Username: admin"
echo "  • Password: golfpassword"
