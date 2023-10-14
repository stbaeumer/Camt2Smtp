# Camt2Smtp

## Beschreibung

Camt2Smtp informiert den Anwender über jede Buchung auf seinem Girokonto per Mail und summiert im Body alle Beträge gleicher, individuell vergebener Kategorien je Kalenderjahr auf.

## Schritte der Verarbeitung:

1. Eine CAMT-Datei wird vom Anwender bei seiner Bank/Sparkasse heruntergeladen. 
1. Camt2Smtp liest (z.B. täglich um 20 Uhr) alle CAMT-Dateien mit allen Buchungen ein.
2. Jede Buchung wird (automatisch) nach Anwenderwünschen kategorisiert.
3. Für jede Buchung wird eine Mail an das gewünschte Mail-Postfach ausgelöst. Im Body der Mail werden alle Buchungen der selben Katgeorie für die Kalenderjahre aufsummiert.
4. Die Mail kann dann über Regeln in einen gewünschten IMAP-Ordner verschoben. FairEmail ist unter Android eine hervorragend App. 

## FAQ

### Für wen ist das Programm?

Wer folgende Fragen bejahen kann, ist Zielgruppe: 

* Ich will über Buchungen per Mail informiert werden
* Ich will Buchungen nach eigenen Wünschen kategorisieren und über Kategorien aufsummieren lassen
* Ich habe einen IMAP-Account, auf dem ich unendlich viele Ordner erstellen kann.
* Meine Mails liegen verschlüsselt in meinem IMAP-Account
* Ich archiviere wichtige Unterlagen in meinem IMAP-Account.
 
#### Beispielhafter Anwendungsfall:

Für jedes Fahrrad erstelle ich einen IMAP-Ordner. In dem Ordner liegt als digitale Kopie die Rechnung, der Fahrradpass, Fotos von Anbauteilen usw. 
Über eine Regel in FairEmail werden alle Mails mit der Kategorie "Riese&Müller" in diesen Ordner einsortiert und  Hinzukommen alle Kontobewegungen vom Kauf über Reparaturen und Ersatzteilkäufe.  

### Warum muss ich erst die CAMT-Datei herunterladen? Andere Programme können auch direkt auf Girokonten zugreifen.

Mir scheint das (auch aus Sicherheitsgründen) eine gute und schnelle Lösung zu sein. 

### Werden E-Mails nicht unverschlüsselt gesendet und abgelegt?

Bei Posteo (und bestimmt auch anderen Mail-Providern) kann die Eingangsverschlüsselung aktiviert werden. Eingehende Mails werden asymmetrisch mit dem eigenen Public-Key verschlüsselt abgelegt. Nur der Betreff bleibt im Klartext. 

Mit FairEmail oder Mailvelope können die Nachrichten dann wieder entschlüsselt werden. 

Man kann dann z.B. auch Fotos von Verträgen verschlüsselt an die eigene Mail-Adresse senden. 

Jeder Vertrag und alle seine zugehörigen Buchungen liegen werden dann im selben IMAP-Ordner archiviert.





