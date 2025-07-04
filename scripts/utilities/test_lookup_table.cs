using System;

public class LookupTableTest
{
    public static void Main()
    {
        // Test the lookup table with Juan's progression data

        Console.WriteLine("Testing Legacy Handicap Lookup Table:");
        Console.WriteLine("=====================================");

        // Test data points from Juan's progression
        var testAverages = new[] { 47.95m, 48.36m, 48.30m, 47.69m, 47.23m, 46.98m };
        var expectedHandicaps = new[] { 8, 9, 8, 8, 7, 7 }; // From the other system

        for (int i = 0; i < testAverages.Length; i++)
        {
            var avg = testAverages[i];
            var expectedHcp = expectedHandicaps[i];
            var calculatedHcp = CalculateHandicapFromLookupTable(avg);

            var status = calculatedHcp == expectedHcp ? "✓ PASS" : "✗ FAIL";
            Console.WriteLine($"Average: {avg:F2} → Expected HC: {expectedHcp}, Calculated HC: {calculatedHcp} {status}");
        }

        Console.WriteLine("\nTesting edge cases:");
        Console.WriteLine("==================");

        // Test edge cases
        Console.WriteLine($"Average: 35.50 → HC: {CalculateHandicapFromLookupTable(35.5m)} (should be 0)");
        Console.WriteLine($"Average: 36.00 → HC: {CalculateHandicapFromLookupTable(36.0m)} (should be 0)");
        Console.WriteLine($"Average: 43.50 → HC: {CalculateHandicapFromLookupTable(43.5m)} (should be 6)");
        Console.WriteLine($"Average: 44.50 → HC: {CalculateHandicapFromLookupTable(44.5m)} (should be 7)");
        Console.WriteLine($"Average: 65.00 → HC: {CalculateHandicapFromLookupTable(65.0m)} (should be 18)");
    }

    public static int CalculateHandicapFromLookupTable(decimal averageScore)
    {
        // Legacy lookup table from the other system
        var lookupTable = new System.Collections.Generic.Dictionary<int, int>
        {
            { 36, 0 },
            { 37, 1 },
            { 38, 2 },
            { 39, 3 },
            { 40, 4 },
            { 41, 5 },
            { 42, 6 },
            { 43, 6 },
            { 44, 7 },
            { 45, 7 },
            { 46, 8 },
            { 47, 9 },
            { 48, 10 },
            { 49, 11 },
            { 50, 11 },
            { 51, 12 },
            { 52, 13 },
            { 53, 13 },
            { 54, 14 },
            { 55, 14 },
            { 56, 15 },
            { 57, 16 },
            { 58, 17 },
            { 59, 17 }
        };

        // Round average score to nearest integer for lookup
        int roundedAverage = (int)Math.Round(averageScore);

        // Handle edge cases
        if (roundedAverage <= 36) return 0;
        if (roundedAverage >= 60) return 18;

        // Lookup handicap in table
        return lookupTable.TryGetValue(roundedAverage, out int handicap) ? handicap : 18;
    }
}
