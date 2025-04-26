using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    const string JatekosokFile = "jatekosok.txt";
    const string HianyzasokFile = "hianyzasok.txt";
    const string NapiFile = "napi.txt";
    const string EredmenyFile = "eredmeny.txt";

    static void Main()
    {
        if (!File.Exists(JatekosokFile) || !File.Exists(HianyzasokFile) || !File.Exists(NapiFile))
        {
            Console.Error.WriteLine("❌ One or more required files are missing!");
            return;
        }

        var nevek = File.ReadAllLines(JatekosokFile)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line))
            .ToList();

        var hianyzasok = File.ReadAllLines(HianyzasokFile)
            .Select(line => double.Parse(line, CultureInfo.InvariantCulture))
            .ToList();

        // Jelenlévők beolvasása (HTML tag-ek eltávolítása, név kinyerés)
        var jelenlevok = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sor in File.ReadAllLines(NapiFile))
        {
            string tisztitott = Regex.Replace(sor, @"<[^>]+>", "").Trim();
            string nev = tisztitott.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrEmpty(nev))
                jelenlevok.Add(nev);
        }

        // Hiányzások frissítése
        for (int i = 0; i < nevek.Count; i++)
        {
            if (i >= hianyzasok.Count)
                hianyzasok.Add(0.0); // Biztonság kedvéért, ha nem egyezne a két lista hossza

            if (jelenlevok.Contains(nevek[i]))
                hianyzasok[i] += 1;
            else
                hianyzasok[i] = Math.Max(0.0, hianyzasok[i] - 0.25);
        }

        // Csak a pozitív hiányzásokat rendezve írjuk ki
        var rendezett = nevek
            .Select((nev, idx) => (nev, hianyzas: hianyzasok[idx]))
            .Where(x => x.hianyzas > 0)
            .OrderByDescending(x => x.hianyzas)
            .ToList();

        using var writer = new StreamWriter(EredmenyFile);

        bool separatorWritten = false;
        foreach (var (nev, hianyzas) in rendezett)
        {
            if (!separatorWritten && hianyzas < 5)
            {
                writer.WriteLine("------------------------------");
                separatorWritten = true;
            }

            string jelzes = hianyzas >= 10 ? " !!!" :
                            hianyzas >= 5 ? " !" : "";

            writer.WriteLine($"{nev} - {hianyzas}{jelzes}");
        }

        // Hiányzások mentése
        File.WriteAllLines(HianyzasokFile,
            hianyzasok.Select(h => h.ToString(CultureInfo.InvariantCulture)));

        Console.WriteLine("✅ Successfully updated 'hianyzasok.txt' and 'eredmeny.txt'!");
    }
}