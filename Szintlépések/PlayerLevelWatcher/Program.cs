using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        string inputFile = "players.txt";
        string lastWeekFile = "lastweek.txt";
        string thisWeekFile = "thisweek.txt";
        string resultFile = "result.txt";

        if (!File.Exists(inputFile))
        {
            Console.WriteLine("❌ A 'players.txt' fájl nem található.");
            return;
        }

        // Aktuális játékosok beolvasása
        string[] currentLines = File.ReadAllLines(inputFile);
        var currentPlayers = new Dictionary<string, int>();

        foreach (string line in currentLines)
        {
            var match = Regex.Match(line, @"> ([^<]+) \((\d+)\. szint\)");
            if (match.Success)
            {
                string name = match.Groups[1].Value.Trim();
                int level = int.Parse(match.Groups[2].Value);
                currentPlayers[name] = level;
            }
        }

        // thisweek.txt-be mentés
        using (StreamWriter thisWeekWriter = new StreamWriter(thisWeekFile))
        {
            foreach (var player in currentPlayers)
            {
                thisWeekWriter.WriteLine($"{player.Key} - {player.Value}");
            }
        }

        // Előző heti játékosok beolvasása
        var lastWeekPlayers = new Dictionary<string, int>();
        if (File.Exists(lastWeekFile))
        {
            foreach (var line in File.ReadAllLines(lastWeekFile))
            {
                var match = Regex.Match(line, @"^(.*?) - (\d+)$");
                if (match.Success)
                {
                    string name = match.Groups[1].Value.Trim();
                    int level = int.Parse(match.Groups[2].Value);
                    lastWeekPlayers[name] = level;
                }
            }
        }

        // Fejlődés kiszámítása
        var results = new List<(string name, int level, int diff)>();
        foreach (var player in currentPlayers)
        {
            string name = player.Key;
            int currentLevel = player.Value;
            int lastLevel = lastWeekPlayers.ContainsKey(name) ? lastWeekPlayers[name] : 0;
            int difference = currentLevel - lastLevel;
            results.Add((name, currentLevel, difference));
        }

        // Eredmények mentése, +1 alatti fejlődés előtt vonal húzása
        using (StreamWriter resultWriter = new StreamWriter(resultFile))
        {
            bool lineWritten = false;

            foreach (var entry in results.OrderByDescending(r => r.diff))
            {
                if (!lineWritten && entry.diff < 1)
                {
                    resultWriter.WriteLine("----------------------");
                    lineWritten = true;
                }

                string diffText = entry.diff >= 0 ? $"+{entry.diff}" : entry.diff.ToString();
                resultWriter.WriteLine($"{entry.name} - {entry.level} | {diffText}");
            }
        }

        Console.WriteLine("✅ result.txt frissítve, fejlődés szerint rendezve, vonallal elválasztva a +1 alattiakat.");
    }
}
