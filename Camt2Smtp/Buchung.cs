// Published under the terms of GPLv3 Stefan Bäumer 2023.

using camt2smtp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public string Vertragsname { get; internal set; }
        public string Zeitstempel { get; internal set; }

        public Buchung(string auftragskonto)
        {
            VergangeneBuchungen = new List<Buchung>();
        }
        public Buchung()
        {
        }

        internal void SendeMail(string benutzer, string protokolldatei, Buchungen protokolldateiBuchungen, Regeln regeln, Buchungen alleKontobewegungen, SmtpClient smtpClient, string smtpUser)
        {
            string b = (Regel != null && Regel.Kategorien != null && Regel.Kategorien != "" ? " [" + Regel.Kategorien.Split(',')[0] + "] " : "") + " " + Buchungstag.Year + "-" + Buchungstag.Month.ToString("00") + "-" + Buchungstag.Day.ToString("00") + " | " + ((BeguenstigterZahlungspflichtiger == null || BeguenstigterZahlungspflichtiger == "" ? "" : BeguenstigterZahlungspflichtiger + " | ") + Verwendungszweck);
            string betreff = b.Substring(0, Math.Min(b.Length, 110)) + " | " + string.Format("{0:#.00}", Betrag) + " €";
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
            buchung.Betrag = Betrag;
            buchung.Währung = Währung;
            buchung.Info = Info;
            buchung.Vertragsname = Regel.Kategorien;
            protokolldateiBuchungen.Add(buchung);
            body += RenderDieseBuchung();
            body += BuchungenZuDiesenRegeln2List(regeln, protokolldateiBuchungen, buchung);

            Console.WriteLine("Betreff: " + betreff.Replace(" €", " EUR"));

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

                // Wenn der Versand erfolgreich war, wird der Datensatz gespeichert

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
                    Betrag + "\";\"" +
                    Währung + "\";\"" +
                    Info + "\";\"" +
                    (Regel == null ? " --- " : Regel.Kategorien) + "\"" + Environment.NewLine;

                File.AppendAllText(protokolldatei, protokollzeile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private string BuchungenZuDiesenRegeln2List(Regeln regeln, Buchungen protokolldateiBuchungen, Buchung buchung)
        {
            var z = @"<table border='1'>";

            foreach (var ver in Regel.Kategorien.Split(','))
            {
                z += "<tr><td><b>" + ver + ":</b></td></tr>";

                for (int year = DateTime.Now.AddYears(-2).Year; year <= DateTime.Now.Year; year++)
                {
                    var summe = (from p in protokolldateiBuchungen
                                 where p.Buchungstag.Year == year
                                 where p.Vertragsname != null
                                 where p.Vertragsname != ""
                                 where p.Vertragsname.Split(',').Contains(ver)
                                 select p.Betrag).Sum();

                    var anzahl = (from p in protokolldateiBuchungen
                                  where p.Buchungstag.Year == year
                                  where p.Vertragsname != null
                                  where p.Vertragsname != ""
                                  where p.Vertragsname.Split(',').Contains(ver)
                                  select p.Betrag).Count();

                    z += "<tr><td>" + year + "</td><td>" + anzahl + "x</td><td>" + string.Format("{0:0.00}", summe) + " EUR</td></tr>";

                    if (DateTime.Now.Year == year)
                    {
                        foreach (var pro in (from p in protokolldateiBuchungen
                                             where p.Buchungstag.Year == year
                                             where p.Vertragsname != null
                                             where p.Vertragsname != ""
                                             where p.Vertragsname.Split(',').Contains(ver)
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
        private Regeln FilterInfragekommendeRegeln(string eigenschaft, List<Regel> infragekommendeRegeln)
        {
            var regeln = new Regeln();

            try
            {
                string eigenschaftswert = this.GetType().GetProperty(eigenschaft).GetValue(this, null).ToString();

                if (eigenschaftswert != "")
                {
                    foreach (var iK in infragekommendeRegeln)
                    {
                        string veigenschaftswert = iK.GetType().GetProperty(eigenschaft).GetValue(iK, null).ToString();

                        if (veigenschaftswert != "")
                        {
                            if (veigenschaftswert.Split(',').All(s => eigenschaftswert.ToLower().Contains(s.ToLower())))
                            {
                                regeln.Add(iK);
                            }
                        }
                    }

                    // Wenn kein Volltreffer für diese Eigenschaft erzielt werden konnte, werden auch die leeren 
                    // Eigenschaftswerte zugelassen.

                    if (regeln.Count == 0)
                    {
                        foreach (var item in infragekommendeRegeln)
                        {
                            string veigenschaft = item.GetType().GetProperty(eigenschaft).GetValue(item, null).ToString();

                            if (veigenschaft == "")
                            {
                                regeln.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return regeln;
        }

        internal string GetRegel(string benutzer, Regeln regeln, string pfad, Buchungen protokollierteBuchungen, Buchungen csvKontobewegungen, SmtpClient smtpClient, string smtpUser)
        {
            try
            {
                Console.Write((this.BeguenstigterZahlungspflichtiger.Substring(0, Math.Min(20, this.BeguenstigterZahlungspflichtiger.Length)) + "|" + this.Verwendungszweck.Substring(0, Math.Min(50, this.Verwendungszweck.Length)) + " ...").PadRight(90, ' '));
                var infragekommendeRegeln = new Regeln();
                infragekommendeRegeln.AddRange(regeln);

                infragekommendeRegeln.Filter(this);

                if (infragekommendeRegeln.Count == 1 && infragekommendeRegeln[0].Kategorien != "?????")
                {
                    this.Regel = infragekommendeRegeln[0];
                    Console.WriteLine(Regel.Kategorien);
                    SendeMail(benutzer, pfad + @"\protokoll.csv", protokollierteBuchungen, regeln, csvKontobewegungen, smtpClient, smtpUser);
                    return "";
                }

                if (infragekommendeRegeln.Count >= 2)
                {
                    // Bei mehreren infragekommenden Regeln muss auf Splitbuchung geprüft werden.
                    // Es liegt eine Splitbuchung vor, wenn nur der Betrag sich unterscheidet.

                    if (
                        infragekommendeRegeln.Select(x => x.BeguenstigterZahlungspflichtiger).Distinct().Count() == 1 &&
                        infragekommendeRegeln.Select(x => x.Buchungstext).Distinct().Count() == 1 &&
                        infragekommendeRegeln.Select(x => x.Iban).Distinct().Count() == 1 &&
                        infragekommendeRegeln.Select(x => x.Kundenreferenz).Distinct().Count() == 1 &&
                        infragekommendeRegeln.Select(x => x.Mandatsreferenz).Distinct().Count() == 1 &&
                        infragekommendeRegeln.Select(x => x.Verwendungszweck).Distinct().Count() == 1 &&
                        infragekommendeRegeln.Select(x => x.Betrag).Distinct().Count() == infragekommendeRegeln.Count
                        )
                    {
                        foreach (var iK in infragekommendeRegeln)
                        {
                            Regel = iK;
                            Console.WriteLine(Regel.Kategorien + " (Splitbuchnung)");
                            this.Betrag = Regel.Betrag;
                            SendeMail(benutzer, pfad + @"\protokoll.csv", protokollierteBuchungen, regeln, csvKontobewegungen, smtpClient, smtpUser);
                        }
                        return "";
                    }
                    else
                    {
                        Console.WriteLine("Keine eindeutige Zuordnung: Es kommen " + infragekommendeRegeln.Count + " Regeln infrage.");
                    }
                }
                else
                {
                    Console.WriteLine("keine Zuordnung");
                }

                var zeile = "\"?????\";\"" + Kundenreferenz + "\";\"" + Mandatsreferenz + "\";\"" + Verwendungszweck + "\";\"" + Iban + "\";\"" + BeguenstigterZahlungspflichtiger + "\";\"" + Math.Abs(Betrag) + "\";\"" + Buchungstext + "\"" + Environment.NewLine;

                if (!(from r in regeln
                      where r.Kategorien == "?????"
                      where r.Kundenreferenz == Kundenreferenz
                      where r.Mandatsreferenz == Mandatsreferenz
                      where r.Verwendungszweck == Verwendungszweck
                      where r.Iban == Iban
                      where r.BeguenstigterZahlungspflichtiger == BeguenstigterZahlungspflichtiger
                      select r).Any())
                {
                    File.AppendAllText(pfad + "\\regeln.csv", zeile);
                }
                return zeile + "\"<br>";
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}