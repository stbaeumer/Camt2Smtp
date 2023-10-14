# Camt2Smtp

Camt2Smtp informiert mich über jede Buchung auf meinem Girokonto per Mail und summiert im Mail-Body alle Beträge gleicher, individuell vergebener Kategorien je Kalenderjahr auf.

In Kombination mit FairEmail und Posteo werden alle eigehenden Mails regelbasiert in meiner IMAP-Orderstuktur verschlüsselt abgelegt. 

Zusammen mit eingescannten Verträgen wird Posteo zum vollwertigen Ersatz eines Online-Laufwerks und auch eines Passwortmanagers.

#### Das Ganze an einem Beispiel:

Für jedes Fahrrad erstelle ich einen IMAP-Ordner. Die digitale Kopie die Rechnung, den Fahrradpass, Fotos von Anbauteilen, ein Foto von der Rahmennummer, die Zahlenkombination des Schlosses usw. schicke ich verschlüsselt per Mail an mich selbst. 
Über eine Regel in FairEmail werden alle Kontobewegungen mit der Kategorie "Riese&Müller" ebenfalls in diesen Ordner einsortiert. So sind alle Informationen zu dem Rad jederzeit und an jedem Ort vorliegend. Im Falle eines Verkaufs oder Diebstahls oder einer Kontrolle muss ich die gewünschte Mail einfach nur in FairEmail öffnen. 

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

### Warum muss ich erst die CAMT-Datei herunterladen? Andere Programme können auch direkt auf Girokonten zugreifen.

Mir scheint das (auch aus Sicherheitsgründen) eine gute und schnelle Lösung zu sein. 

### Werden E-Mails nicht unverschlüsselt gesendet und abgelegt?

Bei Posteo (und bestimmt auch anderen Mail-Providern) kann die Eingangsverschlüsselung aktiviert werden. Eingehende Mails werden asymmetrisch mit dem eigenen Public-Key verschlüsselt abgelegt. Nur der Betreff bleibt im Klartext. 

Mit FairEmail oder Mailvelope oder anderen Programmen können die Nachrichten dann wieder entschlüsselt werden. 

### Sind Splitbuchungen möglich?

Ja, wenn zwei Regeln sich ausschließlich im Betrag unterscheiden, dann liegt eine Splitbuchung vor.;




