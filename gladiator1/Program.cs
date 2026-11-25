using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Gladiator
{
    public string Namn, Typ; // Typ: "Tung", "Snabb" eller "Taktisk"
    public int MaxHP, HP, Styrka, Snabbhet;

    public Gladiator(string namn, string typ, int hp, int styrka, int snabbhet)
    {
        Namn = namn;
        Typ = typ;
        MaxHP = hp;
        HP = hp;
        Styrka = styrka;
        Snabbhet = snabbhet;
    }
}

class Program
{
    static Random rng = new();
    static int vinster = 0, mod = 10, drycker = 2;

    // Dictionary för attacker och deras bas-skada
    static Dictionary<string, int> attacker = new()
    {
        { "Snabbt hugg", 12 },
        { "Tung kross", 20 },
        { "Publikfrieri (+Mod)", 8 }
    };

    static async Task Main()
    {
        Console.WriteLine("=== GLADIATOR: FIRERED LITE ===");
        await HamtaCitat();

        Console.WriteLine("\nVälj din starter-gladiator:");
        Console.WriteLine("1. Maximus [Tung]    - Slår Snabb");
        Console.WriteLine("2. Aurelia [Snabb]   - Slår Taktisk");
        Console.WriteLine("3. Crixus  [Taktisk] - Slår Tung");
        Console.Write("Val: ");

        Gladiator spelare = Console.ReadLine() switch
        {
            "2" => new Gladiator("Aurelia", "Snabb", 90, 13, 18),
            "3" => new Gladiator("Crixus", "Taktisk", 100, 15, 13),
            _ => new Gladiator("Maximus", "Tung", 115, 14, 9)
        };

        while (true)
        {
            Console.WriteLine($"\n--- LÄGER (Vinster: {vinster} | Mod: {mod} | Drycker: {drycker}) ---");
            Console.WriteLine($"{spelare.Namn} [{spelare.Typ}] - HP: {spelare.HP}/{spelare.MaxHP}");
            Console.WriteLine("1. Gå ut på arenan   2. Utmana Mästaren   3. Vila (Full HP)   4. Avsluta");
            Console.Write("Val: ");
            string val = Console.ReadLine() ?? "";

            if (val == "1")
            {
                Gladiator[] fiender = {
                    new("Sky", "Snabb", 75, 12, 16),
                    new("Brutus", "Tung", 105, 14, 7),
                    new("Midas", "Taktisk", 85, 13, 12)
                };
                StartaStrid(spelare, fiender[rng.Next(fiender.Length)]);
            }
            else if (val == "2")
            {
                StartaStrid(spelare, new Gladiator("Mästaren Spartacus", "Taktisk", 140, 18, 14));
            }
            else if (val == "3")
            {
                spelare.HP = spelare.MaxHP;
                Console.WriteLine("Full hälsa återställd!");
            }
            else if (val == "4")
            {
                Console.WriteLine("Hej då!");
                break;
            }
        }
    }

    static void StartaStrid(Gladiator spelare, Gladiator fiende)
    {
        Console.WriteLine($"\n[!] {fiende.Namn} [{fiende.Typ}] kliver in på arenan!");

        while (spelare.HP > 0 && fiende.HP > 0)
        {
            Console.WriteLine($"\nDU: {spelare.HP}/{spelare.MaxHP} HP  |  FIENDE: {fiende.HP}/{fiende.MaxHP} HP");
            Console.WriteLine("1. Snabbt hugg   2. Tung kross   3. Publikfrieri   4. Läkedryck (+35 HP)   5. Ge upp");
            Console.Write("Drag: ");
            string drag = Console.ReadLine() ?? "1";

            if (drag == "5")
            {
                Console.WriteLine("Du gav upp!");
                mod = Math.Max(0, mod - 5);
                return;
            }

            if (drag == "4")
            {
                if (drycker > 0)
                {
                    drycker--;
                    spelare.HP = Math.Min(spelare.MaxHP, spelare.HP + 35);
                    Console.WriteLine("Du drack en läkedryck!");
                    // Fienden får anfalla efter att du druckit
                    Attackera(fiende, spelare, "Vildsint hugg", 13);
                }
                else
                {
                    Console.WriteLine("Slut på drycker!");
                }
            }
            else
            {
                string atkNamn = drag == "2" ? "Tung kross" : (drag == "3" ? "Publikfrieri (+Mod)" : "Snabbt hugg");
                if (drag == "3") mod += 5;

                if (spelare.Snabbhet >= fiende.Snabbhet)
                {
                    Attackera(spelare, fiende, atkNamn, attacker[atkNamn]);
                    if (fiende.HP > 0) Attackera(fiende, spelare, "Vildsint hugg", 13);
                }
                else
                {
                    Attackera(fiende, spelare, "Vildsint hugg", 13);
                    if (spelare.HP > 0) Attackera(spelare, fiende, atkNamn, attacker[atkNamn]);
                }
            }
        }

        if (fiende.HP <= 0)
        {
            Console.WriteLine($"\nSEGER mot {fiende.Namn}!");
            vinster++;
            if (rng.Next(2) == 0) { drycker++; Console.WriteLine("Du hittade en läkedryck!"); }
        }
        else
        {
            Console.WriteLine($"\nDu förlorade! {(mod >= 15 ? "Kejsaren räddar ditt liv för ditt mod!" : "Läkarna bär ut dig.")}");
            spelare.HP = spelare.MaxHP / 2;
        }
    }

    static void Attackera(Gladiator anfallare, Gladiator mal, string attackNamn, int basSkada)
    {
        if (rng.Next(100) < 15)
        {
            Console.WriteLine($"{anfallare.Namn} missade med {attackNamn}!");
            return;
        }

        double bonus = (anfallare.Typ == "Tung" && mal.Typ == "Snabb") ||
                       (anfallare.Typ == "Snabb" && mal.Typ == "Taktisk") ||
                       (anfallare.Typ == "Taktisk" && mal.Typ == "Tung") ? 1.5 : 1.0;

        int skada = (int)((basSkada + anfallare.Styrka * 0.4) * bonus);
        mal.HP = Math.Max(0, mal.HP - skada);

        Console.WriteLine($"{anfallare.Namn} använder {attackNamn} och gör {skada} skada! {(bonus > 1 ? "(Supereffektivt!)" : "")}");
    }

    static async Task HamtaCitat()
    {
        try
        {
            using HttpClient client = new();
            string json = await client.GetStringAsync("https://zenquotes.io/api/random");
            using JsonDocument doc = JsonDocument.Parse(json);
            Console.WriteLine($"Dagens citat: \"{doc.RootElement[0].GetProperty("q")}\"");
        }
        catch
        {
            Console.WriteLine("Dagens citat: \"Fortune favors the bold.\"");
        }
    }
}