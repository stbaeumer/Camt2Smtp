// Published under the terms of GPLv3 Stefan Bäumer 2023.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace camt2smtp
{
    public class Buchungen : List<Buchung>
    {
        public Buchungen()
        {
        }

        public Buchungen(string protokolldatei)
        {
            try
            {
                var buchungenUnsortiert = new Buchungen();

                if (!File.Exists(protokolldatei))
                {
                    string clientHeader = "\"Zeitstempel\";\"Auftragskonto\";\"Buchungstag\";\"Valutadatum\";\"Buchungstext\";\"Verwendungszweck\";\"Glaeubiger ID\";\"Mandatsreferenz\";\"Kundenreferenz (End-to-End)\";\"Sammlerreferenz\";\"LastschriftUrsprungsbetrag\";\"Auslagenersatz Ruecklastschrift\";\"Beguenstigter/Zahlungspflichtiger\";\"Kontonummer/IBAN\";\"BIC (SWIFT-Code)\";\"Betrag\";\"Waehrung\";\"Info\";\"Vertrag\"" + Environment.NewLine;

                    File.WriteAllText(protokolldatei, clientHeader);
                }

                using (StreamReader reader = new StreamReader(protokolldatei, Encoding.UTF8, true))
                {
                    var überschrift = reader.ReadLine();
                    int i = 1;

                    while (true)
                    {
                        i++;
                        Buchung buchung = new Buchung();
                        string line = reader.ReadLine();

                        try
                        {
                            if (line != null)
                            {
                                string pattern = @"""\s*;\s*""";

                                string[] x = System.Text.RegularExpressions.Regex.Split(line.Substring(1, line.Length - 2), pattern);

                                try
                                {
                                    buchung = new Buchung();
                                    buchung.Zeitstempel = x[0];
                                    buchung.Auftragskonto = x[1];
                                    buchung.Buchungstag = DateTime.ParseExact(x[2].PadRight(10).Substring(0, 10), "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    buchung.Valutadatum = x[3] == "" ? new DateTime() : DateTime.ParseExact(x[2], "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    buchung.Buchungstext = x[4];
                                    buchung.Verwendungszweck = x[5];
                                    buchung.GlaeubigerID = x[6];
                                    buchung.Mandatsreferenz = x[7];
                                    buchung.Kundenreferenz = x[8];
                                    buchung.Sammlerreferenz = x[9];
                                    buchung.LastschriftUrsprungsbetrag = x[10];
                                    buchung.AuslagenersatzRuecklastschrift = x[11];
                                    buchung.BeguenstigterZahlungspflichtiger = x[12];
                                    buchung.Iban = x[13];
                                    buchung.Bic = x[14];
                                    buchung.Betrag = Convert.ToDecimal(x[15]);
                                    buchung.Währung = x[16];
                                    buchung.Info = x[17];
                                    buchung.Vertragsname = x[18];

                                    buchungenUnsortiert.Add(buchung);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Fehler in Zeile: " + i + " Fehlermeldung: " + ex.ToString());
                                    break;
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
                    this.AddRange(buchungenUnsortiert.OrderBy(x => x.Buchungstag));
                    Console.WriteLine("Protokollierte ausgeführte Buchungen: " + this.Count);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Buchungen(List<string> camtDateien, Regeln regeln)
        {
            try
            {
                var buchungenUnsortiert = new Buchungen();

                foreach (string camtDatei in camtDateien)
                {
                    int zähler = 0;

                    using (StreamReader reader = new StreamReader(camtDatei, Encoding.Default, true))
                    {
                        var überschrift = reader.ReadLine();
                        int i = 1;
                        zähler = 0;

                        while (true)
                        {
                            i++;

                            Buchung buchung = new Buchung();

                            string line = reader.ReadLine();

                            try
                            {
                                if (line != null)
                                {
                                    string pattern = @"""\s*;\s*""";
                                    string[] x = System.Text.RegularExpressions.Regex.Split(line.Substring(1, line.Length - 2), pattern);

                                    buchung = new Buchung();
                                    buchung.Auftragskonto = x[0];
                                    buchung.Buchungstag = DateTime.ParseExact(x[1], "dd.MM.yy", System.Globalization.CultureInfo.InvariantCulture);
                                    buchung.Valutadatum = x[2] == "" ? new DateTime() : DateTime.ParseExact(x[2], "dd.MM.yy", System.Globalization.CultureInfo.InvariantCulture);
                                    buchung.Buchungstext = x[3];
                                    buchung.Verwendungszweck = x[4];
                                    buchung.GlaeubigerID = x[5];
                                    buchung.Mandatsreferenz = x[6];
                                    buchung.Kundenreferenz = x[7];
                                    buchung.Sammlerreferenz = x[8];
                                    buchung.LastschriftUrsprungsbetrag = x[9];
                                    buchung.AuslagenersatzRuecklastschrift = x[10];
                                    buchung.BeguenstigterZahlungspflichtiger = x[11];
                                    buchung.Iban = x[12];
                                    buchung.Bic = x[13];
                                    buchung.Betrag = Convert.ToDecimal(x[14]);
                                    buchung.Währung = x[15];
                                    buchung.Info = x[16];
                                    buchung.VergangeneBuchungen = new List<Buchung>();

                                    // Buchungen könnten in verschiedenen Export-Dateien mehrfach vorkommen.
                                    // Also werden sie gefiltert.

                                    if (!(from t in this
                                          where t.Verwendungszweck == buchung.Verwendungszweck
                                          where t.Buchungstag == buchung.Buchungstag
                                          where t.Mandatsreferenz == buchung.Mandatsreferenz
                                          where t.Kundenreferenz == buchung.Kundenreferenz
                                          where t.BeguenstigterZahlungspflichtiger == buchung.BeguenstigterZahlungspflichtiger
                                          select t).Any())
                                    {
                                        if (buchung.Info == "Umsatz gebucht")
                                        {
                                            if (!(from b in buchungenUnsortiert
                                                  where b.Verwendungszweck == buchung.Verwendungszweck
                                                  where b.Buchungstag == buchung.Buchungstag
                                                  where b.Mandatsreferenz == buchung.Mandatsreferenz
                                                  where b.Kundenreferenz == buchung.Kundenreferenz
                                                  where b.BeguenstigterZahlungspflichtiger == buchung.BeguenstigterZahlungspflichtiger
                                                  select b).Any()
                                                 )
                                            {
                                                buchungenUnsortiert.Add(buchung);
                                                zähler++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Soll-Buchungen " + camtDatei + ": " + zähler);
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                AddRange(buchungenUnsortiert.OrderBy(x => x.Buchungstag));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}