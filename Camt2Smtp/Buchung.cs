// Published under the terms of GPLv3 Stefan Bäumer 2023.

using camt2smtp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace camt2smtp
{
    public class Buchung
    {
        public string Auftragskonto { get; internal set; }
        public DateTime Buchungstag { get; internal set; }
        public DateTime Valutadatum { get; internal set; }
        public string Buchungstext { get; internal set; }
        public string Verwendungszweck { get; internal set; }
        public string GlaeubigerID { get; internal set; }
        public string Mandatsreferenz { get; internal set; }
        public string Kundenreferenz { get; internal set; }
        public string Sammlerreferenz { get; internal set; }
        public string LastschriftUrsprungsbetrag { get; internal set; }
        public string AuslagenersatzRuecklastschrift { get; internal set; }
        public string BeguenstigterZahlungspflichtiger { get; internal set; }
        public string Iban { get; internal set; }
        public string Bic { get; internal set; }
        public decimal Betrag { get; internal set; }
        public string Währung { get; internal set; }
        public string Info { get; internal set; }
        public List<Buchung> VergangeneBuchungen { get; set; }
        public Regel Regel { get; set; }
        public string Kürzel { get; private set; }
        public string VertragJaNein { get; private set; }
        public string Kategorien { get; internal set; }
        public string Zeitstempel { get; internal set; }
        public string Zeile { get; internal set; }
        public Regeln Regeln { get; internal set; }
        public List<string> Kategorienliste { get; internal set; }

        public Buchung(string auftragskonto)
        {
            VergangeneBuchungen = new List<Buchung>();
        }
        public Buchung()
        {
        }

        internal void SendeMail(string benutzer, string protokolldatei, Buchungen protokolldateiBuchungen, SmtpClient smtpClient, string smtpUser)
        {
            string b = " [" + Regeln[0].KategorienListe[0] + (Regeln[0].KategorienListe.Count() > 1 ? "," + Regeln[0].KategorienListe[1] : "") + (Regeln[0].KategorienListe.Count() > 2 ? "," + Regeln[0].KategorienListe[2] : "") + "] " + Buchungstag.Year + "-" + Buchungstag.ToString("MMM", CultureInfo.InvariantCulture) + "-" + Buchungstag.Day.ToString("00") + " | " + ((BeguenstigterZahlungspflichtiger == null || BeguenstigterZahlungspflichtiger == "" ? "" : BeguenstigterZahlungspflichtiger + " | " + Verwendungszweck));
            string betreff = b.Substring(0, Math.Min(b.Length, 120
                )) + " | " + string.Format("{0:#.00}", Regeln[0].Betrag != 0 ? Regeln[0].Betrag : Betrag) + " €";
            string body = this.Auftragskonto;

            var buchung = new Buchung();
            buchung.Auftragskonto = Auftragskonto;
            buchung.Buchungstag = Buchungstag;
            buchung.Valutadatum = Valutadatum;
            buchung.Buchungstext = Buchungstext;
            buchung.Verwendungszweck = Verwendungszweck;
            buchung.GlaeubigerID = GlaeubigerID;
            buchung.Mandatsreferenz = Mandatsreferenz;
            buchung.Kundenreferenz = Kundenreferenz;
            buchung.Sammlerreferenz = Sammlerreferenz;
            buchung.LastschriftUrsprungsbetrag = LastschriftUrsprungsbetrag;
            buchung.AuslagenersatzRuecklastschrift = AuslagenersatzRuecklastschrift;
            buchung.BeguenstigterZahlungspflichtiger = BeguenstigterZahlungspflichtiger;
            buchung.Iban = Iban;
            buchung.Bic = Bic;
            buchung.Betrag = Regeln[0].Betrag != 0 ? Regeln[0].Betrag : Betrag;
            buchung.Währung = Währung;
            buchung.Info = Info;
            buchung.Kategorien = String.Join(",", Regeln[0].KategorienListe.ToArray());
            buchung.Regel = Regeln[0];
            protokolldateiBuchungen.Add(buchung);
            body += RenderDieseBuchung();
            body += BuchungenZuDiesenRegeln2List(protokolldateiBuchungen);

            Console.Write("Betreff: " + betreff.Replace(" €", " EUR").Substring(0,20) + "...");

            MailMessage mm = new MailMessage(smtpUser, smtpUser, betreff, body)
            {
                BodyEncoding = UTF8Encoding.UTF8,
                SubjectEncoding = UTF8Encoding.UTF8,
                DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,
                IsBodyHtml = true
            };

            try
            {
                smtpClient.Send(mm);
                Console.WriteLine("gesendet.");

                // Wenn der Versand erfolgreich war, wird der Datensatz protokolliert
                
                
                var regeln = String.Join(",", Regeln[0].KategorienListe.ToArray());

                // Bei Splitbuchungen wird der Betrag aus der Regel genommen.
                var betrag = (regeln.Contains("Splitbuchung") ? Regeln[0].Betrag : Betrag);

                string protokollzeile = "\"" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + "\";\"" +
                    Auftragskonto + "\";\"" +
                    Buchungstag.ToShortDateString() + "\";\"" +
                    Valutadatum.ToShortDateString() + "\";\"" +
                    Buchungstext + "\";\"" +
                    Verwendungszweck + "\";\"" +
                    GlaeubigerID + "\";\"" +
                    Mandatsreferenz + "\";\"" +
                    Kundenreferenz + "\";\"" +
                    Sammlerreferenz + "\";\"" +
                    LastschriftUrsprungsbetrag + "\";\"" +
                    AuslagenersatzRuecklastschrift + "\";\"" +
                    BeguenstigterZahlungspflichtiger + "\";\"" +
                    Iban + "\";\"" +
                    Bic + "\";\"" +
                    betrag + "\";\"" +
                    Währung + "\";\"" +
                    Info + "\";\"" +
                    (Regeln[0] == null ? " --- " : regeln) + "\"" + Environment.NewLine;

                File.AppendAllText(protokolldatei, protokollzeile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private string BuchungenZuDiesenRegeln2List(Buchungen protokolldateiBuchungen)
        {
            var z = @"<table border='1'>";

            foreach (var ver in Regeln[0].KategorienListe)
            {
                z += "<tr><td><b>" + ver + ":</b></td></tr>";

                for (int year = DateTime.Now.AddYears(-2).Year; year <= DateTime.Now.Year; year++)
                {
                    var summe = (from p in protokolldateiBuchungen
                                 where p.Buchungstag.Year == year
                                 where p.Kategorien != null
                                 where p.Kategorien != ""
                                 where p.Kategorien.ToLower().Split(',').Contains(ver.ToLower())
                                 select p.Betrag).Sum();

                    var anzahl = (from p in protokolldateiBuchungen
                                  where p.Buchungstag.Year == year
                                  where p.Kategorien != null
                                  where p.Kategorien != ""
                                  where p.Kategorien.ToLower().Split(',').Contains(ver.ToLower())
                                  select p.Betrag).Count();

                    z += "<tr><td>" + year + "</td><td>" + anzahl + "x</td><td>" + string.Format("{0:0.00}", summe) + " EUR</td></tr>";

                    if (DateTime.Now.Year == year)
                    {
                        foreach (var pro in (from p in protokolldateiBuchungen
                                             where p.Buchungstag.Year == year
                                             where p.Kategorien != null
                                             where p.Kategorien != ""
                                             where p.Kategorien.ToLower().Split(',').Contains(ver.ToLower())
                                             select p).ToList())
                        {
                            z += "<tr><td></td><td>" + pro.Buchungstag.ToShortDateString() + "</td><td>" + string.Format("{0:0.00}", pro.Betrag) + " EUR</td></tr>";
                        }
                    }
                }
            }
            return z + "</table>";
        }

        internal string RenderDieseBuchung()
        {
            try
            {
                return "<table><tr><td>Verwendungszweck:</td><td>" + Verwendungszweck + "</td></tr>" +
                   "<tr><td>" + BeguenstigterZahlungspflichtiger + "</td></tr>" +
                   "<tr><td>" + Iban + "</td></tr>" +
                   "<tr><td>Kundenreferenz: " + Kundenreferenz + "</td></tr>" +
                   "<tr><td>Mandatsreferenz :" + Mandatsreferenz + "</td></tr>" +
                   "<tr><td>" + string.Format("{0:#.00}", Betrag) + " EUR</td></tr>" +
                   "<tr><td>Buchungstag:</td><td>" + Buchungstag.ToShortDateString() + "</td></tr>" +
                   "<tr><td>Valuta:</td><td>" + Valutadatum.ToShortDateString() + "</td></tr>" +
                   "<tr><td>Buchungstext:</td><td>" + Buchungstext + "</td></tr>" +
                   "<tr><td>Kategorie:</td><td>" + (Regel == null ? " --- " : Regel.Kategorien) + "</td></tr></table>";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal string GetRegel(string regeldatei, string benutzer, string pfad, Buchungen protokollierteBuchungen, Buchungen csvKontobewegungen, SmtpClient smtpClient, string smtpUser)
        {
            try
            {
                Console.Write((this.BeguenstigterZahlungspflichtiger.Substring(0, Math.Min(20, this.BeguenstigterZahlungspflichtiger.Length)) + "|" + this.Verwendungszweck.Substring(0, Math.Min(25, this.Verwendungszweck.Length)) + " ...").PadRight(29, ' '));

                // Wenn es in allen Regeln genau einen Volltreffer gibt, werden alle anderen Regeln gelöscht.

                if ((from regel in this.Regeln
                     where regel.KriterienListe.All(w => this.Zeile.ToLower().Contains(w.ToLower()))
                     where regel.Betrag == this.Betrag
                     select regel).Count() == 1)
                {
                    var r = (from regel in this.Regeln
                             where regel.KriterienListe.All(w => this.Zeile.ToLower().Contains(w.ToLower()))
                             where regel.Betrag == this.Betrag
                             select regel).FirstOrDefault();

                    this.Regeln.Clear();
                    this.Regeln.Add(r);
                    
                    SendeMail(benutzer, pfad + @"\protokoll.csv", protokollierteBuchungen, smtpClient, smtpUser);
                    return "";
                }

                var regeln = (from regel in this.Regeln
                              where regel.KriterienListe.All(w => Zeile.ToLower().Contains(w.ToLower()))
                              select regel).ToList();

                // Wenn bis auf den Betrag alle Kriterien passen und mehr als eine Buchung
                // infrage kommt,dann könnte eine Splitbuchung vorliegen

                // zuerst wird geprüft, ob alle zusammen passen
                if (regeln.Sum(x => x.Betrag) == Betrag)
                {
                    foreach (var regel in regeln)
                    {
                        regel.KategorienListe.Add("Splitbuchung-" + Math.Abs(Betrag));
                        this.Regeln.Clear();
                        this.Regeln.Add(regel);
                        SendeMail(benutzer, pfad + @"\protokoll.csv", protokollierteBuchungen, smtpClient, smtpUser);
                    }
                    return "";
                }


                // dann wird geprüft, ob nur zwei verschiedene Elemente matchen.
                for (int i = 0; i < regeln.Count - 1; i++)
                {
                    for (int j = i + 1; j < regeln.Count; j++)
                    {
                        if (regeln[i].Betrag + regeln[j].Betrag == Betrag)
                        {
                            foreach (var regel in new Regeln() { regeln[i], regeln[j] })
                            {
                                regel.KategorienListe.Add("Splitbuchung-" + Math.Abs(Betrag));
                                this.Regeln.Clear();
                                this.Regeln.Add(regel);
                                SendeMail(benutzer, pfad + @"\protokoll.csv", protokollierteBuchungen, smtpClient, smtpUser);
                            }
                            return "";
                        }
                    }
                }

                if (regeln.Count() == 1)
                {
                    // Wenn nur ein Treffer erzielt wurde und nur die Kriterien, aber nicht der
                    // Betrag stimmt

                    Regeln.Clear();                                        
                    Regeln.Add(regeln[0]);
                    SendeMail(benutzer, pfad + @"\protokoll.csv", protokollierteBuchungen, smtpClient, smtpUser);
                    return "";
                }
                Console.WriteLine();
                File.AppendAllText(regeldatei, Environment.NewLine + "#" + "|" + this.Zeile + "|");
                return "#" + "|"+ this.Zeile + "|</br>";
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}