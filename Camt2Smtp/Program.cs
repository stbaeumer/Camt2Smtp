// Published under the terms of GPLv3 Stefan Bäumer 2023.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace camt2smtp
{
    class Program
    {
        public static string Benutzer = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToUpper().Split('\\')[1];
        public static string Pfad = "";
        public static string SmtpPassword = "";
        public static string SmtpUser = "";
        public static string SmtpServer = "";
        public static string SmtpPort = "";
        public static string Camt = "";
        public static SmtpClient SmtpClient;
        public static Buchungen ProtokollierteBuchungen;
        public static Buchungen SollBuchungen;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("camt2smtp");
                Console.WriteLine("Version 20231125" );
                Console.WriteLine("Published under the terms of GPLv3 Stefan Bäumer 2023.");
                Console.WriteLine("======================================================");
                Console.WriteLine("");
                
                PrüfeParameter(args);

                SmtpClient = BaueSmtpClient(SmtpUser, SmtpPassword, SmtpServer, SmtpPort);

                PrüfePfad();

                List<string> camtDateien = PrüfeDateien(Benutzer, Pfad, Camt);

                ProtokollierteBuchungen = new Buchungen(Pfad + @"\protokoll.csv");

                var regelpfad = Pfad + "\\" + (from f in new DirectoryInfo(Pfad).GetFiles()
                                               where f.Name.Contains("regeln.")
                                               orderby f.LastWriteTime descending
                                               select f).First().ToString();

                var regeln = new Regeln(regelpfad);

                SollBuchungen = new Buchungen(camtDateien, regeln);

                Sicherung(Pfad, SmtpClient, SmtpUser);

                var offeneKontobewegungen = "";

                foreach (var buchung in SollBuchungen)
                {
                    // Nur wenn eine Buchung noch nicht durchgeführt wurde... 

                    if (!(from protokollierteBuchung in ProtokollierteBuchungen
                          where protokollierteBuchung.Verwendungszweck == buchung.Verwendungszweck
                          where protokollierteBuchung.Buchungstag.Date == buchung.Buchungstag.Date
                          where protokollierteBuchung.BeguenstigterZahlungspflichtiger == buchung.BeguenstigterZahlungspflichtiger
                          select protokollierteBuchung).Any())
                    {
                        offeneKontobewegungen += buchung.GetRegel(regelpfad, Benutzer, Pfad, ProtokollierteBuchungen, SollBuchungen, SmtpClient, SmtpUser);
                    }
                }

                if (offeneKontobewegungen.Length > 0)
                {                    
                    SendeMail(Benutzer, offeneKontobewegungen, SmtpClient, SmtpUser);
                }

                //Protokoll2Erstellen(Pfad + @"\protokoll.csv");

                Console.WriteLine("Das Programm schließt in 10 Sekunden.");
                Thread.Sleep(20000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                SendeMail(Benutzer, ex.Message, SmtpClient, SmtpUser);
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private static void Protokoll2Erstellen(string pfad)
        {
            // Create the IEnumerable data source  
            string[] lines = System.IO.File.ReadAllLines(pfad + @"\protokoll.csv");

            // Create the query. Put field 2 first, then  
            // reverse and combine fields 0 and 1 from the old field  
            IEnumerable<string> query =
                from line in lines
                let x = line.Split(',')
                orderby x[2]
                select x[2] + ", " + (x[1] + " " + x[0]);

            // Execute the query and write out the new file. Note that WriteAllLines  
            // takes a string[], so ToArray is called on the query.  
            System.IO.File.WriteAllLines(Pfad + @"\protokoll_sortiert.csv", query.ToArray());

            Console.WriteLine("Protokoll_sortiert geschrieben.");            
        }

        private static SmtpClient BaueSmtpClient(string smtpUser, string smtpPassword, string smtpServer, string smtpPort)
        {
            try
            {
                SmtpClient = new SmtpClient
                {
                    Port = Convert.ToInt32(SmtpPort),
                    Host = SmtpServer,
                    EnableSsl = true,
                    Timeout = 10000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(SmtpUser, SmtpPassword)
                };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                return SmtpClient;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void PrüfeParameter(string[] args)
        {
            if (args.Length == 1 && HelpRequired(args[0]))
            {
                DisplayHelp("", "", "", "", "", "");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Ihre Parameter:");
                for (int i = 0; i < args.Length; i++)
                {
                    var parameter = new List<string>() { "-pfad", "-p", "-u", "-s", "-port", "c" };

                    if (args[i] == "-pfad" && args.Length > i && args[i + 1] != "")
                    {
                        if (!parameter.Contains(args[i + 1]))
                        {
                            Pfad = args[i + 1];
                            Console.WriteLine(" Pfad: " + Pfad);
                        }
                    }
                    if (args[i] == "-p" && args.Length > i && args[i + 1] != "")
                    {
                        if (!parameter.Contains(args[i + 1]))
                        {
                            SmtpPassword = args[i + 1];
                            Console.WriteLine(" SmtpPasswort: *****");
                        }

                    }
                    if (args[i] == "-u" && args.Length > i && args[i + 1] != "")
                    {
                        if (!parameter.Contains(args[i + 1]))
                        {
                            SmtpUser = args[i + 1];
                            Console.WriteLine(" SmtpUser: " + SmtpUser);
                        }
                    }
                    if (args[i] == "-s" && args.Length > i && args[i + 1] != "")
                    {
                        if (!parameter.Contains(args[i + 1]))
                        {
                            SmtpServer = args[i + 1];
                            Console.WriteLine(" SmtpServer: " + SmtpServer);
                        }
                    }
                    if (args[i] == "-port" && args.Length > i && args[i + 1] != "")
                    {
                        if (!parameter.Contains(args[i + 1]))
                        {
                            SmtpPort = args[i + 1];
                            Console.WriteLine(" SmtpPort: " + SmtpPort);
                        }
                    }
                    if (args[i] == "-c" && args.Length > i && args[i + 1] != "")
                    {
                        if (!parameter.Contains(args[i + 1]))
                        {
                            Camt = args[i + 1];
                            Console.WriteLine(" Namensbestandteil, an dem CAMT-Dateien erkannt werden: " + Camt);
                        }
                    }
                }
                if (SmtpPassword == "" || SmtpUser == "" || SmtpServer == "" || Pfad == "" || SmtpPort == "" || Camt == "")
                {
                    DisplayHelp(SmtpPassword, SmtpUser, SmtpServer, SmtpPort, Pfad, Camt);
                }
            }
        }

        private static void DisplayHelp(string smtpPassword, string smtpServer, string smtpUser, string pfad, string smtpPort, string camt)
        {
            Console.WriteLine("");
            Console.WriteLine("Hilfe:");
            Console.WriteLine("camt2smtp liest die Datei *CSV-CAMT V2* aus und mailt jede einzelne Buchung an die angegebene IMAP-Adresse.");
            Console.WriteLine("Siehe auch https://github.com/stbaeumer/camt2smtp");

            if (smtpServer == "")
            {
                Console.WriteLine("Geben Sie den Parameter -s SMTPSERVER an.");
            }
            if (smtpPort == "")
            {
                Console.WriteLine("Geben Sie den Parameter -port SMTPPORT an.");
            }
            if (smtpUser == "")
            {
                Console.WriteLine("Geben Sie den Parameter -u SMTPUSER an.");
            }
            if (smtpPassword == "")
            {
                Console.WriteLine("Geben Sie den Parameter -p PASSWORT an.");
            }

            if (pfad == "")
            {
                Console.WriteLine("Geben Sie den Parameter -pfad PFAD an.");
            }
            if (camt == "")
            {
                Console.WriteLine("Woran werden CAMT-Dateien (außer an der .CSV-Endung) erkannt? Geben Sie eine Namensbestandtteil ein..");
                Console.WriteLine("Geben Sie den Parameter -c NAMENSBESTANDTEIL an.");
            }
            Console.ReadKey();
        }

        private static bool HelpRequired(string param)
        {
            return param == "-h" || param == "--help" || param == "/?";
        }

        private static void Sicherung(string targetPath, SmtpClient client, string smtpUser)
        {
            try
            {
                Console.WriteLine("");

                // Monatliche Sicherung

                if (DateTime.Now.Day == 1)
                {
                    ProtokollSichern(targetPath, client, smtpUser);
                }
                Console.WriteLine("----------------------------------");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void SendeMail(string benutzer, string regelszeile, SmtpClient client, string smtpUser)
        {
            try
            {
                string betreff = "camt2smtp-Meldung";
                string body = "Der Datei " + Pfad + @"\regeln.csv wurde(n) folgende Zeile(n) hinzugefügt. Bitte ersetzen Sie den Kommentar # durch eine oder mehrere kommagetrennte Kategorien. Wählen Sie die Eigenschaften so, dass Mails an diesen Empfänger direkt erkannt werden.</br></br>" + regelszeile;

                MailMessage mm = new MailMessage(smtpUser, smtpUser, betreff, body)
                {
                    BodyEncoding = UTF8Encoding.UTF8,
                    SubjectEncoding = UTF8Encoding.UTF8,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,
                    IsBodyHtml = true
                };

                try
                {
                    client.Send(mm);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void ProtokollSichern(string pfad, SmtpClient client, string smtpUser)
        {
            try
            {
                string betreff = "camt2smtp Sicherung";
                string body = "camt2smtp Sicherung";

                MailMessage mailMessage = new MailMessage(smtpUser, smtpUser, betreff, body)
                {
                    BodyEncoding = UTF8Encoding.UTF8,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,
                    IsBodyHtml = true
                };

                DirectoryInfo d = new DirectoryInfo(pfad);

                foreach (var datei in d.GetFiles("*.csv"))
                {
                    mailMessage.Attachments.Add(new Attachment(pfad + datei.Name));
                }

                client.Send(mailMessage);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static List<string> PrüfeDateien(string benutzer, string pfad, string namensbestandteil)
        {
            try
            {
                var quellDateien = (from f in Directory.GetFiles(pfad, "*.csv", SearchOption.AllDirectories) where f.Contains(namensbestandteil) select f).ToList();

                Console.WriteLine("CAMT-Dateien: " + quellDateien.Count);

                return quellDateien.OrderBy(q => q).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void PrüfePfad()
        {
            if (!Directory.Exists(Pfad))
            {
                throw new Exception("Der Pfad " + Pfad + " existiert nicht.");
            }
            else
            {
                Pfad = Path.GetFullPath(Pfad);
            }
        }
    }
}