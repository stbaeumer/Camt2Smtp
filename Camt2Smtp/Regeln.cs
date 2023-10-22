// Published under the terms of GPLv3 Stefan Bäumer 2023.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace camt2smtp
{
    public class Regeln : List<Regel>
    {
        public string Pfad { get; private set; }

        public Regeln(string pfad)
        {
            try
            {
                var datei = (from f in new DirectoryInfo(pfad).GetFiles()
                             where f.Name.Contains("regeln.")
                             orderby f.LastWriteTime descending
                             select f).First();

                if (datei == null)
                {
                    string kopfzeile = "\"Name\";\"Kundenreferenz\";\"Mandatsreferenz\";\"IndikatorVerwendungszweck\";\"IndikatorIban\";\"IndikatorBeguenstigter\";\"Betrag\"" + Environment.NewLine;

                    File.WriteAllText(pfad + datei, kopfzeile);
                }

                using (StreamReader streamReader = new StreamReader(pfad + "\\" + datei))
                {
                    var überschrift = streamReader.ReadLine();

                    while (true)
                    {
                        Regel regel = new Regel();
                        string line = streamReader.ReadLine();

                        try
                        {
                            if (line != null)
                            {
                                string pattern = @"""\s*;\s*""";
                                string[] x = System.Text.RegularExpressions.Regex.Split(line.Substring(1, line.Length - 2), pattern);

                                try
                                {
                                    regel.Kategorien = x[0];
                                    regel.getSortierkriterium();
                                    regel.Kundenreferenz = x[1];
                                    regel.Mandatsreferenz = x[2];
                                    regel.Verwendungszweck = x[3];
                                    regel.Iban = x[4];
                                    regel.BeguenstigterZahlungspflichtiger = x[5];
                                    regel.Betrag = x[6] == "" ? 0 : Convert.ToDecimal(x[6]);
                                    regel.Buchungstext = x[7];
                                    this.Add(regel);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Fehler beim Einlesen der Regeln aus " + datei + " in Zeile " + line + ".\n" + regel.Verwendungszweck + ex.Message);
                                    Console.ReadLine();
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                // Datei nach Beguenstigter sortieren
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Regeln()
        {
        }

        internal void Filter(Buchung buchung)
        {
            foreach (Regel regel in this.ToList())
            {
                foreach (var eigenschaft in new List<string>
                {
                    "BeguenstigterZahlungspflichtiger",
                    "Verwendungszweck",
                    "Mandatsreferenz",
                    "Kundenreferenz",
                    "Iban",
                    "Buchungstext"
                })
                {
                    string eigenschaftswert = buchung.GetType().GetProperty(eigenschaft).GetValue(buchung, null).ToString();
                    string reigenschaftswert = regel.GetType().GetProperty(eigenschaft).GetValue(regel, null).ToString();

                    foreach (var re in reigenschaftswert.Split(','))
                    {
                        // Wenn in der Regel ein Eigenschaftswert gesetzt ist, muss die Buchung darauf matchen.

                        if (re != "" && (eigenschaftswert == "" || eigenschaftswert != "" && !eigenschaftswert.ToLower().Contains(re.ToLower())))
                        {
                            this.Remove(regel);
                        }
                    }
                }
            }
        }
    }
}