using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Transactions;
//20250607
namespace LaptopBerles
{
    class Program
    {
        static void Main(string[] args)
        {
            List<RentalTransaction> rentalTransactionList = new List<RentalTransaction>(); 
            using(StreamReader sr = new StreamReader("laptoprentals2022.csv"))
            {
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] i = line.Split(";");
                    Berlo berlo = new Berlo
                    {
                        PersonalId = i[0],
                        Name = i[1],
                        DateOfBirth = DateOnly.Parse(i[2]),
                        PostalCode = Convert.ToInt32(i[3]),
                        City = i[4],
                        Address = i[5]
                    };
                    InvetoriedLaptop laptop = new InvetoriedLaptop
                    {
                       InvNumber = i[6],
                       Model = i[7],
                       County= i[8],
                       RAM = Convert.ToInt32(i[9]),
                       Color = i[10],
                       DailyFee = Convert.ToInt32(i[11]),   
                       Deposit  = Convert.ToInt32(i[12]),
                    };
                    RentalTransaction rentalTransaction = new RentalTransaction
                    {
                        KiBerelte = berlo,
                        LaptopRented = laptop,
                        StartDate = DateOnly.Parse(i[13]),
                        StopDate = DateOnly.Parse(i[14]),
                        UsedDeposit = i[15]== "1" ? true: false,
                        Uptime = Convert.ToDouble(i[16])
                    };
                    rentalTransactionList.Add(rentalTransaction);
                }
            }
            Console.WriteLine("3. feladat: Bérlések darabszáma: " + rentalTransactionList.Count);
            /////////////////////////////////////////////////////////
            Console.WriteLine("4.feladat: Szürke Acer bérlések");
            var szurke = rentalTransactionList.Where(k => k.LaptopRented.Model.Contains("Acer")
                         && k.LaptopRented.Color=="szürke").OrderByDescending(k=>k.LaptopRented.InvNumber);
            foreach(var berlet in szurke)
            {
                //LPT014772 Acer TravelMate 8118- SVG225RT Podmaniczky Fedóra
                Console.WriteLine($"\t{berlet.LaptopRented.InvNumber} {berlet.LaptopRented.Model} --- {berlet.KiBerelte.PersonalId} {berlet.KiBerelte.Name}");
            }
            ///////////////////////////////////////////////////////////
            Console.WriteLine("5. feladat: Vármegyék, ahol a legkevesebb laptopot bérelték");
            var megyeKeves = rentalTransactionList
                .GroupBy(k => k.LaptopRented.County)
                .Select(g => new { County = g.Key, Count = g.Count() })
                .OrderBy(x => x.Count)
                .Take(2);
            foreach(var megyeberletszam in megyeKeves)
            {
                Console.WriteLine($"\t{megyeberletszam.County}: {megyeberletszam.Count}");
            }
            ////////////////////////////////////////////////////////////
            Console.WriteLine("6. feladat:");
            Console.WriteLine("\tKeresett leltári szám (kilépéshez: 0)");
            while (true)
            {
                string pattern = @"^LPT\d{6}$";
                string input = Console.ReadLine();
                if (input == "0") break;
                if(!Regex.IsMatch(input, pattern))
                {
                    Console.WriteLine("Hibás formátum");
                    continue;
                }
                else
                {
                    var laptopFind = rentalTransactionList
                        .Where(k=>k.LaptopRented.InvNumber==input).FirstOrDefault();
                    if (laptopFind == null) 
                        Console.WriteLine("\tNincs ilyen leltári számú laptop a " +
                            "beolvasott adatok között!");
                    else
                    {
                        //LPT724143 Fujitsu Lifebook E5 sárga
                        Console.WriteLine($"\t{laptopFind.LaptopRented.InvNumber} " +
                                          $"{laptopFind.LaptopRented.Model} " +
                                          $"{laptopFind.LaptopRented.Color}");
                    }
                    break;
                }

            }
            ////////////////////////////////////////////////////////////
            int bevetel = 0;
            foreach(RentalTransaction berles in rentalTransactionList)
            {
                int laptopFee = berles.LaptopRented.DailyFee;
                int napok = berles.StopDate.DayNumber - berles.StartDate.DayNumber + 1;
                int depositToKeep = berles.UsedDeposit ? berles.LaptopRented.Deposit : 0;
                bevetel = bevetel + laptopFee*napok + depositToKeep;
            }
            //7. feladat: A cég összes bevétele: 45 293 000 Ft
            Console.WriteLine($"7. feladat: A cég összes bevétele: {bevetel:N0} Ft");
            //8. feladat:
            double AvgUpTime(string invNumber)
            {
                var rents = rentalTransactionList.Where(k => k.LaptopRented.InvNumber == invNumber);
                if (rents.Count() > 0)
                {
                    double uptime = 0;
                    foreach (var rent in rents)
                    {
                        uptime = uptime + rent.Uptime;
                    }
                    return Math.Round(uptime / rents.Count(), 2);
                }
                return 0;
            }
            //8. feladat: Az LPT233271 leltári számú laptop bérlésenkénti átlagos üzemideje: 82,87 óra
            Console.WriteLine("8. feladat: \tKeresett leltári számú gép átlagos üzemidőhöz (kilépéshez: 0)");
            while (true)
            {
                double uzemido = 0;
                string pattern = @"^LPT\d{6}$";
                string input = Console.ReadLine();
                if (input == "0") break;
                if (!Regex.IsMatch(input, pattern))
                {
                    Console.WriteLine("Hibás formátum");
                    continue;
                }
                else
                {
                    var laptopFind = rentalTransactionList
                        .Where(k => k.LaptopRented.InvNumber == input).FirstOrDefault();
                    if (laptopFind == null)
                        Console.WriteLine("\tNincs ilyen leltári számú laptop a " +
                            "beolvasott adatok között!");
                    else
                    {
                        uzemido = AvgUpTime(input);
                        Console.WriteLine($"8. feladat: Az {input} leltári számú laptop bérlésenkénti " +
                              $"átlagos üzemideje: {uzemido} óra");
                    }
                    break;
                }
            }
        }
    }
    public class Berlo
    {
        public string PersonalId { get; set; }
        public string Name { get; set; }    
        public DateOnly DateOfBirth { get; set; }
        public int PostalCode { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
    }
    public class InvetoriedLaptop 
    {
        public string InvNumber { get; set; }
        public string Model { get; set; }
        public string County  { get; set; }
        public int RAM { get; set; }
        public string Color { get; set; }
        public int DailyFee { get; set; }
        public int Deposit { get;set; }
    }
    public class RentalTransaction
    {
        public InvetoriedLaptop LaptopRented { get; set; }
        public Berlo KiBerelte { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly StopDate { get; set; }
        public bool UsedDeposit { get; set; }
        public double Uptime { get; set; }
    }
}
