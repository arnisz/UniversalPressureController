# Architektur der Anwendung

## 1. Übersicht

Die Anwendung dient zur Steuerung und Überwachung eines Druck Controllers (zunächst Mensor CPC) über die GPIB-Schnittstelle. Sie ist in mehrere klar getrennte Schichten unterteilt, um Wartbarkeit und Erweiterbarkeit zu gewährleisten.

---

## 2. Schichten der Architektur

### a) Präsentationsschicht (UI)

- **Zweck:** Interaktion mit dem Benutzer
- **Technologien:** WPF, WinForms oder Weboberfläche (je nach Implementierung)
- **Hauptaufgaben:**
    - Anzeigen von Messwerten und Statusinformationen
    - Steuerelemente für den Benutzer (z\. B\. Start/Stop, Sollwert setzen)
    - Fehler- und Statusmeldungen anzeigen

### b) Anwendungsschicht (Application Layer)

- **Zweck:** Orchestrierung der Geschäftslogik und Kommunikation zwischen UI und Services
- **Hauptaufgaben:**
    - Verarbeiten von Benutzeraktionen
    - Aufrufen der Methoden im GPIB Communication Service
    - Fehlerbehandlung und Weiterleitung von Statusinformationen an die UI

### c) Service-Schicht

- **Zweck:** Implementierung der Geschäftslogik und Kommunikation mit der Hardware
- **Komponenten:**
    - **GPIB Communication Service:** Direkte Kommunikation mit dem Mensor CPC über GPIB
    - **Logging-Service:** Protokolliert Aktionen und Fehler
    - **Konfigurationsservice:** Verarbeitet und speichert Konfigurationsdaten (z\. B\. GPIB-Adresse)

### d) Datenzugriffsschicht (Data Access Layer)

- **Zweck:** Zugriff auf persistente Daten
- **Hauptaufgaben:**
    - Speichern und Laden von Konfigurationsdaten
    - Optional: Speichern von Messwerten oder Logs in einer Datenbank

### e) Hardware-Abstraktionsschicht

- **Zweck:** Abstraktion der Kommunikation mit der GPIB-Hardware
- **Hauptaufgaben:**
    - Bereitstellen einer API für die GPIB-Kommunikation
    - Übersetzen von Befehlen und Antworten zwischen Anwendung und Hardware

---

## 3. Komponenten und Kommunikation

- **UI ↔ Application Layer:** Benutzeraktionen werden an die Anwendungsschicht weitergeleitet, Ergebnisse und Statusmeldungen an die UI zurückgegeben.
- **Application Layer ↔ Service Layer:** Die Anwendungsschicht ruft Methoden des GPIB Communication Service auf, um Befehle auszuführen oder Daten abzurufen.
- **Service Layer ↔ Hardware-Abstraktionsschicht:** Der GPIB Communication Service nutzt die Hardware-Abstraktionsschicht zur Kommunikation mit dem Gerät.
- **Service Layer ↔ Data Access Layer:** Konfigurations- und Log-Daten werden über die Datenzugriffsschicht gespeichert und geladen.

---

## 4. Technologien

- **Programmiersprache:** C\#
- **Frameworks:** .NET (z\. B\. .NET 6 oder höher)
- **Protokoll:** GPIB (General Purpose Interface Bus)

---

## 5. Erweiterbarkeit

- Neue Geräte oder Schnittstellen können durch zusätzliche Services oder Anpassung der Hardware-Abstraktionsschicht integriert werden.
- Die UI kann durch moderne Frameworks (z\. B\. Blazor oder MAUI) ersetzt oder erweitert werden.