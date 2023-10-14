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

## Erläuterungen mit Bildern 

### Das Programm wird (z.B. täglich mit einem Task) gestartet
Die eingegebenen Parameter werden angezeigt. Das Programm speichert intern keine Daten, wie z.B. das Passwort. Alles wird als Parameter bei der Ausführung übergeben. 

![Konsole](Camt2Smtp/Bilder/Parameter.png?raw=true)

Die Konsolenausgabe zeigt: Es wurden bisher 12 CAMT-Dateien heruntergeladen. Alle werden eingelesen. Eine Buchung mit der Kategorie "Arzt,Gesundheit,ArztrechnungenMinusErstattungen" wird ausgeführt. Der Betreff der Mail wird angezeigt. In eckigen Klammern steht die erste Kategorie, anhand derer Fairemail die Zurodnung in einen IMAP-Ordner vornimmt.

![Konsole](Camt2Smtp/Bilder/console.png?raw=true)

### Fairemail wartet auf eingehende Mails.
Alle Ordner sind zugeklappt. 

![Fairemail](Camt2Smtp/Bilder/fairemail_zugeklappt.png?raw=true)

### Eine Abbuchung ist erfolgt.
Der Ordner klappt sich auf und zeigt, dass eine neue Abbuchung (also eine ungelesene Mail) vorliegt. Im selben Ordner liegen auch alle Emails rund um den Vertrag. Mit Fairemail kann sogar eine Kündigung zum gewünschten Termin angezeigt werden.

![fairemail](Camt2Smtp/Bilder/fairemail_neue_abbuchung.png?raw=true)

### Falls Camt2Smtp keine Regel zuordnen kann.

Wenn Camt2Smtp einer Buchung keine Regel zuordnen kann, wird eine Meldung ausgelöst, die dann ebenfalls per Mail verschickt wird.

![Meldung](Camt2Smtp/Bilder/meldung.png?raw=true)

Der Inhalt der Meldung wird markiert und kopiert, ... 

![Meldung](Camt2Smtp/Bilder/meldung_geoeffnet_markiert.png?raw=true)

... um anschließend in der Datei regeln.csv angefügt zu werden. Die Fragezeichen werden durch Kategorien ersetzt. Beim nächsten Start von Camt2smtp wird die Regel auf diese Buchung angewendet.

![Regeln](Camt2Smtp/Bilder/regeln.png?raw=true)

Beispiele für Regeln im Editor:

![Regeln](Camt2Smtp/Bilder/Regeln_beispiele.png?raw=true)

### Erfolgreiche Buchungen

Erfolgreich abgearbeitete Buchungen werden in der Datei protokoll.csv abgelegt.

![Protokoll](Camt2Smtp/Bilder/protokoll.png?raw=true)
