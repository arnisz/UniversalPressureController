# UniversalPressureController

WPF-Anwendung zur Steuerung und Überwachung eines Mensor-CPC-Druckcontrollers über GPIB.

## Version 1.1.0

- Umstellung von .NET 6 auf .NET 10 (LTS).
- NationalInstruments.Visa 25.3.0.11 und Newtonsoft.Json 13.0.3.
- Bereinigung der Nullable-Warnungen; bestehende SCPI-/GPIB-Kommandos bleiben unverändert.

## Windows-App starten

1. Das Paket `UniversalPressureController-v1.1.0-win-x64.zip` aus den [GitHub-Releases](https://github.com/arnisz/UniversalPressureController/releases) herunterladen.
2. Den gesamten Inhalt in einen beschreibbaren Ordner entpacken und `UniversalPressureController.exe` starten. Die EXE nicht einzeln aus dem Ordner verschieben.
3. Die GPIB-Adresse des Geräts in der Anwendung einstellen.

Das Paket enthält die .NET-10-Laufzeit; eine separate .NET-Installation ist nicht erforderlich. Es benötigt ein von .NET 10 unterstütztes Windows-x64-System (Windows 10/11) und für die Gerätekommunikation eine passende NI-VISA-x64-Installation sowie die Treiber des GPIB-Adapters. Diese nativen Treiber sind nicht im Paket enthalten.

Beim ersten Start wird `config.json` im Anwendungsordner angelegt. Diese Datei enthält die lokale Konfiguration und wird nicht im Repository veröffentlicht.

Das Release 1.1.0 ist **nicht digital signiert**. Windows kann deshalb eine Sicherheitswarnung anzeigen. Das ältere signierte Release bleibt unverändert verfügbar.

## Aus dem Quellcode bauen

Voraussetzung: .NET 10 SDK auf Windows.

```powershell
dotnet build UniversalPressureController.slnx -c Release
dotnet publish UniversalPressureController.csproj -c Release -r win-x64 --self-contained true -o artifacts\win-x64
```

Die bisherigen Debug-/Release-Builds und der Fensterstart wurden geprüft. Die Kommunikation mit realer Mensor-/GPIB-Hardware muss am Gerät geprüft werden.

## Lizenz

Siehe [LICENSE](LICENSE).