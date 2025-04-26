using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        string inputFile = "jatekosok.txt";
        string lastWeekFile = "lastweek.txt";
        string thisWeekFile = "thisweek.txt";
        string resultFile = "fejlodes-eredmeny.txt";

        if (!File.Exists(inputFile))
        {
            Console.WriteLine("❌ A 'jatekosok.txt' fájl nem található.");
            return;
        }

        // Aktuális játékosok beolvasása (Regex meghagyva, ahogy kérted)
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

        // thisweek.txt-be mentés (ABC sorrendben)
        using (StreamWriter thisWeekWriter = new StreamWriter(thisWeekFile))
        {
            foreach (var player in currentPlayers.OrderBy(p => p.Key))
            {
                thisWeekWriter.WriteLine($"{player.Key} - {player.Value}");
            }
        }

        // Előző heti játékosok beolvasása (egyszerű split, Regex helyett)
        var lastWeekPlayers = new Dictionary<string, int>();
        if (File.Exists(lastWeekFile))
        {
            foreach (var line in File.ReadAllLines(lastWeekFile))
            {
                var parts = line.Split(" - ");
                if (parts.Length == 2 && int.TryParse(parts[1], out int level))
                {
                    lastWeekPlayers[parts[0].Trim()] = level;
                }
            }
        }

        // Fejlődés kiszámítása
        var results = new List<(string name, int level, int diff)>();
        foreach (var player in currentPlayers)
        {
            string name = player.Key;
            int currentLevel = player.Value;
            lastWeekPlayers.TryGetValue(name, out int lastLevel);
            int difference = currentLevel - lastLevel;
            results.Add((name, currentLevel, difference));
        }

        // Eredmények mentése
        using (StreamWriter resultWriter = new StreamWriter(resultFile))
        {
            bool lineWritten = false;

            foreach (var entry in results
                .OrderByDescending(r => r.diff)
                .ThenByDescending(r => r.level))
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

        Console.WriteLine("✅ Sikeresen kiszámolva a fejlődés minden céhtagnál.");
    }
}