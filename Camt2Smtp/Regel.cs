﻿// Published under the terms of GPLv3 Stefan Bäumer 2023.

using System;
using System.Collections.Generic;
using System.IO;

namespace camt2smtp
{
    public class Regel
    {
        public string Kategorien { get; internal set; }
        public string Verwendungszweck { get; internal set; }
        public string Iban { get; internal set; }
        public string BeguenstigterZahlungspflichtiger { get; internal set; }
        public decimal Betrag { get; internal set; }
        public string Mandatsreferenz { get; internal set; }
        public string Kundenreferenz { get; internal set; }
        public string Buchungstext { get; internal set; }
        public string Sortierkriterium { get; internal set; }
        public List<string> KategorienListe { get; internal set; }
        public string[] KriterienListe { get; internal set; }

        public Regel(string kundenreferenz, string mandantsreferenz, string kategorien, string indikatorVerwendungszweck, string indikatorIban, string indikatorBeguenstigter, string pfad, decimal betrag)
        {
            Verwendungszweck = indikatorVerwendungszweck;
            Iban = indikatorIban;
            BeguenstigterZahlungspflichtiger = indikatorBeguenstigter;
            Kategorien = kategorien;
            Kundenreferenz = kundenreferenz;
            Mandatsreferenz = mandantsreferenz;
            Betrag = betrag;

            string regelzeile = "\"" + Kategorien + "\";\"" + Kundenreferenz + "\";\"" + Mandatsreferenz + "\";\"" + Verwendungszweck + "\";\"" + Iban + "\";\"" + BeguenstigterZahlungspflichtiger + "\";\"" + Betrag + "\"" + Environment.NewLine;

            File.AppendAllText(pfad, regelzeile);
        }

        public Regel()
        {
        }

        public Regel(Regel item)
        {
            BeguenstigterZahlungspflichtiger = item.BeguenstigterZahlungspflichtiger;
            Mandatsreferenz = item.Mandatsreferenz;
            Kundenreferenz = item.Kundenreferenz;
            Kategorien = item.Kategorien;
            Verwendungszweck = item.Verwendungszweck;
            Iban = item.Iban;
            Betrag = item.Betrag;
        }

        internal void getSortierkriterium()
        {
            // Es wird das zweite Element in den Kategorien zum Sortieren verwendet

            var x = Kategorien.Split(',');

            if (x.Length == 1)
            {
                this.Sortierkriterium = x[0];
            }
            if (x.Length > 1)
            {
                this.Sortierkriterium = x[1];
            }
        }
    }
}