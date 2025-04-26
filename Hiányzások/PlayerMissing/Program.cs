using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

class Program
{
    const int MaxSorok = 50;

    static void Main()
    {
        string[] nevek = new string[MaxSorok];
        double[] hianyzasok = new double[MaxSorok];
        List<(int index, double hianyzas)> rendezetthianyzasok = new();

        int index = 0;

        // Nevek beolvasása
        if (!File.Exists("nevek.txt"))
        {
            Console.Error.WriteLine("Can't open 'nevek.txt' file!");
            return;
        }

        var nevLines = File.ReadAllLines("nevek.txt");
        for (; index < Math.Min(nevLines.Length, MaxSorok); index++)
        {
            // Az egész nevet tároljuk el, nem csak az első szót
            nevek[index] = nevLines[index].Trim(); // Az egész név megtartása
        }

        // Hiányzások beolvasása
        if (!File.Exists("hianyzasok.txt"))
        {
            Console.Error.WriteLine("Can't open 'hianyzasok.txt' file!");
            return;
        }

        var hianyzasLines = File.ReadAllLines("hianyzasok.txt");
        for (int i = 0; i < Math.Min(hianyzasLines.Length, MaxSorok); i++)
        {
            hianyzasok[i] = double.Parse(hianyzasLines[i], CultureInfo.InvariantCulture);
        }

        // Napi jelenlevők beolvasása formázott sorokból
        if (!File.Exists("napi.txt"))
        {
            Console.Error.WriteLine("Can't open 'napi.txt' file!");
            return;
        }

        HashSet<string> jelenlevok = new();
        var napiSorok = File.ReadAllLines("napi.txt");

        // A HTML tagek eltávolítása és a név kinyerése
        foreach (var sor in napiSorok)
        {
            // HTML tagek eltávolítása
            string tisztitottSor = Regex.Replace(sor, @"<[^>]+>", "").Trim();

            // Az első szó (a név) kinyerése
            string nev = tisztitottSor.Split(' ')[0].Trim();

            // A nevet hozzáadjuk a jelenlevők listájához
            if (!string.IsNullOrWhiteSpace(nev))
            {
                jelenlevok.Add(nev);
            }
        }

        // Hiányzások frissítése
        for (int i = 0; i < index; i++)
        {
            // Az egész nevet tartjuk meg kisbetűk nélkül
            string nevTrimmed = nevek[i].Trim(); // Az ékezetekkel helyesen tárolt név
            if (jelenlevok.Contains(nevTrimmed))
            {
                hianyzasok[i] += 1;
            }
            else
            {
                hianyzasok[i] = Math.Max(0.0, hianyzasok[i] - 0.25);
            }
        }

        // Rendezés
        for (int i = 0; i < index; i++)
        {
            if (hianyzasok[i] > 0)
            {
                rendezetthianyzasok.Add((i, hianyzasok[i]));
            }
        }

        rendezetthianyzasok = rendezetthianyzasok
            .OrderByDescending(x => x.hianyzas)
            .ToList();

        // Eredmény fájlba
        using var eredmenyFile = new StreamWriter("eredmeny.txt");
        bool printedSeparator = false;
        foreach (var (idx, hianyzas) in rendezetthianyzasok)
        {
            if (hianyzas == 0)  // Ha nincs hiányzás, nem írjuk ki
                continue;

            if (hianyzas < 5 && !printedSeparator)
            {
                eredmenyFile.WriteLine("------------------------------");
                printedSeparator = true;
            }

            // A teljes nevet írjuk ki most
            if (hianyzas >= 10)
                eredmenyFile.WriteLine($"{nevek[idx]} - {hianyzas} !!!");
            else if (hianyzas >= 5)
                eredmenyFile.WriteLine($"{nevek[idx]} - {hianyzas} !");
            else
                eredmenyFile.WriteLine($"{nevek[idx]} - {hianyzas}");
        }

        // Hiányzások mentése
        File.WriteAllLines("hianyzasok.txt", hianyzasok
            .Take(index)
            .Select(h => h.ToString(CultureInfo.InvariantCulture)));

        Console.WriteLine("✅ Successfully updated 'hianyzasok.txt' and 'eredmeny.txt'!");
    }
}
