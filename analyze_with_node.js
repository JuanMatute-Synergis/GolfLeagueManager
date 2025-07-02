const fs = require('fs');

// Load the data
const scoreEntries = JSON.parse(fs.readFileSync('score_entries_data.json', 'utf8'));

console.log('=== HANDICAP CALCULATION ANALYSIS ===\n');

// Focus on Kevin Kelhart JR who has a discrepancy
const targetPlayer = "Kevin Kelhart JR";
const kevinScores = scoreEntries
    .filter(entry => entry.player.firstName === 'Kevin' && entry.player.lastName === 'Kelhart JR')
    .map(entry => ({
        weekId: entry.weekId,
        score: entry.score,
        points: entry.pointsEarned
    }))
    .sort((a, b) => a.weekId.localeCompare(b.weekId));

console.log(`${targetPlayer} week-by-week progression:`);
console.log('Week ID | Score | Points');
console.log('--------|-------|-------');
kevinScores.forEach(entry => {
    console.log(`${entry.weekId.substring(0, 8)}... | ${entry.score.toString().padStart(5)} | ${entry.points.toString().padStart(6)}`);
});

// Get his handicap info
const kevinHandicap = kevinScores[0] ? scoreEntries.find(e => e.player.firstName === 'Kevin' && e.player.lastName === 'Kelhart JR').player : null;
if (kevinHandicap) {
    console.log(`\nKevin Kelhart JR handicap info:`);
    console.log(`Initial Handicap: ${kevinHandicap.initialHandicap}`);
    console.log(`Current Handicap: ${kevinHandicap.currentHandicap}`);
    console.log(`Initial Avg Score: ${kevinHandicap.initialAverageScore}`);
    console.log(`Current Avg Score: ${kevinHandicap.currentAverageScore}`);
}

// Calculate various averages for Kevin
const validScores = kevinScores.filter(s => s.score > 0).map(s => s.score);
console.log(`\nValid scores: [${validScores.join(', ')}]`);
console.log(`Overall average: ${(validScores.reduce((a, b) => a + b, 0) / validScores.length).toFixed(2)}`);

if (validScores.length >= 3) {
    const recent3 = validScores.slice(-3);
    console.log(`Recent 3 average: ${(recent3.reduce((a, b) => a + b, 0) / 3).toFixed(2)}`);

    const best3 = [...validScores].sort((a, b) => a - b).slice(0, 3);
    console.log(`Best 3 average: ${(best3.reduce((a, b) => a + b, 0) / 3).toFixed(2)}`);
}

// Now analyze the week structure
console.log('\n=== WEEK STRUCTURE ANALYSIS ===');
const weekCounts = {};
const weekPlayerCounts = {};

scoreEntries.forEach(entry => {
    if (!weekCounts[entry.weekId]) {
        weekCounts[entry.weekId] = 0;
        weekPlayerCounts[entry.weekId] = new Set();
    }
    weekCounts[entry.weekId]++;
    weekPlayerCounts[entry.weekId].add(entry.player.firstName + ' ' + entry.player.lastName);
});

console.log('Week ID | Total Entries | Unique Players');
console.log('--------|---------------|---------------');
Object.keys(weekCounts).sort().forEach(weekId => {
    console.log(`${weekId.substring(0, 8)}... | ${weekCounts[weekId].toString().padStart(13)} | ${weekPlayerCounts[weekId].size.toString().padStart(14)}`);
});

// Analyze a few more players with discrepancies
console.log('\n=== OTHER PLAYERS WITH DISCREPANCIES ===');
const otherPlayers = ['Ray Ballinger', 'Tom Haeusler', 'Jax Haeusler'];

otherPlayers.forEach(playerName => {
    const [firstName, lastName] = playerName.split(' ');
    const playerScores = scoreEntries
        .filter(entry => entry.player.firstName === firstName && entry.player.lastName === lastName)
        .map(entry => ({ score: entry.score, points: entry.pointsEarned }))
        .filter(entry => entry.score > 0);

    if (playerScores.length > 0) {
        const scores = playerScores.map(s => s.score);
        const avg = scores.reduce((a, b) => a + b, 0) / scores.length;
        console.log(`\n${playerName}:`);
        console.log(`  Scores: [${scores.join(', ')}]`);
        console.log(`  Average: ${avg.toFixed(2)}`);

        const playerInfo = scoreEntries.find(e => e.player.firstName === firstName && e.player.lastName === lastName).player;
        console.log(`  Initial Handicap: ${playerInfo.initialHandicap}, Current: ${playerInfo.currentHandicap}`);
        console.log(`  Initial Avg: ${playerInfo.initialAverageScore}, Current: ${playerInfo.currentAverageScore}`);
    }
});

console.log('\n=== HANDICAP CALCULATION HYPOTHESIS ===');
console.log('Based on the analysis, the previous system likely used:');
console.log('1. Simple overall average of all played rounds');
console.log('2. Handicap = (Average Score - Par)');
console.log('3. Par appears to be 36 for this course');
console.log('4. Current system uses World Handicap System which:');
console.log('   - Uses best 8 of last 20 scores');
console.log('   - Applies course/slope ratings');
console.log('   - Uses more complex differential calculations');
console.log('\nThis explains the 1-2 stroke discrepancies we see.');
