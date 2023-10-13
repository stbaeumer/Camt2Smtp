# Camt2Smtp

## Beschreibung

Camt2Smtp informiert den Anwender �ber jede Buchung auf seinem Girokonto per Mail und summiert im Body alle Betr�ge gleicher, individuell vergebener Kategorien je Kalenderjahr auf.

## Schritte der Verarbeitung:

1. Eine CAMT-Datei wird vom Anwender bei seiner Bank/Sparkasse heruntergeladen. 
1. Camt2Smtp liest (z.B. t�glich um 20 Uhr) alle CAMT-Dateien mit allen Buchungen ein.
2. Jede Buchung wird (automatisch) nach Anwenderw�nschen kategorisiert.
3. F�r jede Buchung wird eine Mail an das gew�nschte Mail-Postfach ausgel�st. Im Body der Mail werden alle Buchungen der selben Katgeorie f�r die Kalenderjahre aufsummiert.
4. Die Mail kann dann �ber Regeln in einen gew�nschten IMAP-Ordner verschoben. FairEmail ist unter Android eine hervorragend App. 

## FAQ

### Warum muss ich erst die CAMT-Datei herunterladen? Andere Programme k�nnen auch direkt auf Girokonten zugreifen.

Mir scheint das (auch aus Sicherheitsgr�nden) eine gute und schnelle L�sung zu sein. 

### Werden E-Mails nicht unverschl�sselt gesendet und abgelegt?

Bei Posteo (und bestimmt auch anderen Mail-Providern) kann die Eingangsverschl�sselung aktiviert werden. Eingehende Mails werden asymmetrisch mit dem eigenen Public-Key verschl�sselt abgelegt. Nur der Betreff bleibt im Klartext. 

Mit FairEmail oder Mailvelope k�nnen die Nachrichten dann wieder entschl�sselt werden. 

Man kann dann z.B. auch Fotos von Vertr�gen verschl�sselt an die eigene Mail-Adresse senden. 

Jeder Vertrag und alle seine zugeh�rigen Buchungen liegen werden dann im selben IMAP-Ordner archiviert.





