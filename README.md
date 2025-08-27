# 🌀 Fusion Mods Launcher

> Ein moderner WPF‑Launcher für **FiveM** mit Ein‑Klick‑Connect, optionalem **Pure Mode**, **Auto‑TeamSpeak**, Steam‑Check und **Dev Mode**.  
> Ziel: **Einmal klicken – direkt auf den Server.**

<p align="center">
  <img alt="Platform" src="https://img.shields.io/badge/platform-Windows-blue">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-8.0-purple">
  <img alt="WPF" src="https://img.shields.io/badge/UI-WPF-6aa84f">
  <img alt="License" src="https://img.shields.io/badge/license-choose--one-lightgrey">
</p>

---

## ✨ Features

- 🔒 **Sicherer Start:** Warnt, wenn **FiveM** bereits läuft – auf Wunsch wird der Prozess geschlossen.
- 🎮 **Ein‑Klick Join:** Verbindet automatisch mit deinem `cfx.re` **Servercode** (z. B. `r9g6xx`).
- 🧼 **Pure Mode (optional):** Startet FiveM mit `-pure_1` (über temporäre .lnk), danach Auto‑Join.
- 🟢 **Steam Check:** Start bricht ab, wenn **Steam** nicht offen ist (mit Hinweis).
- 🎧 **Auto‑TeamSpeak (optional):** Startet `ts3server://<ip>` automatisch.
- ⏱️ **Waiting Time:** Konfigurierbare Wartezeit, bis der Auto‑Join ausgeführt & der Shortcut wieder gelöscht wird.
- 🛠️ **Dev Mode:** Per Kommandozeile aktivierbar – ideal fürs Debugging & Tests.
- 🪟 **Schönes UI:** Dunkles, moderner Look (WPF), eigenes App‑Icon (`fusionmods.ico`).

---

## 📦 Projektüberblick

- **Name:** `Fusion Mods Launcher` (`FusionMods`)
- **Technologie:** .NET 8 (WPF, Windows)
- **Entry Point:** `MainWindow` → `Backend.StartLoader()`
- **Wichtige Klassen/Dateien:**
  - `src/Backend.cs` – Startlogik (Steam‑Check, FiveM schließen/starten, TeamSpeak, Pure Mode, etc.)
  - `MainWindow.xaml` / `MainWindow.xaml.cs` – UI, Settings‑Panel, Buttons
  - `App.xaml.cs` – Auswertung von CLI‑Argumenten (u. a. `--devmode`)
  - `Properties/Settings.*` – Benutzer‑Settings
  - `assets/background.jpg` – Hintergrundbild

> **Hinweis:** Das Projekt nutzt `IWshRuntimeLibrary` (Windows Script Host), um im **Pure Mode** eine temporäre **.lnk** anzulegen und darüber `-pure_1` zu starten.

---

## 🚀 Quickstart (User)

1. **Release laden** (oder selbst bauen, siehe unten).
2. **Starten:** `FusionMods.exe`
3. ⚙️ **Einstellungen öffnen** (Zahnrad) und setzen:
   - **ServerIP (Servercode)**: dein `cfx.re` Join‑Code (Standard: `r9g6xx`)
   - **TeamSpeakIP** (optional): z. B. `ts.meinserver.de`
   - **AutoTeamSpeak** (optional): aktivieren, wenn TS automatisch starten soll
   - **PureMode** (optional): aktivieren für `-pure_1`
   - **WaitingTime**: Zeit in **ms** (z. B. `6000`) bis zum Auto‑Join
4. **PLAY NOW** → Launcher erledigt den Rest:
   - Warnung/Schließen, falls **FiveM** bereits läuft
   - Check, ob **Steam** läuft
   - Start von FiveM (+ optional `-pure_1`)
   - Join via `fivem://connect/cfx.re/join/<ServerIP>`
   - Optional: **TeamSpeak** wird geöffnet

---

## 🧭 Bedienung im Detail

### Startlogik
- Prüft laufende **FiveM**‑Prozesse → Bestätigungsdialog → **CloseMainWindow**/**Kill** (fallback)
- **Steam** muss laufen → sonst Abbruch mit Meldung
- **Pure Mode**: temporären Shortcut (`launch_fivem.lnk`) erzeugen → FiveM mit `-pure_1` starten → auf Main Window warten → `fivem://connect/...` öffnen → Shortcut löschen
- **Non‑Pure Mode**: direkt `fivem://connect/...` per `explorer.exe`

### Einstellungen (Properties)
| Setting         | Typ     | Default        | Beschreibung |
|-----------------|---------|----------------|--------------|
| `ServerIP`      | string  | `r9g6xx`       | Dein `cfx.re` Servercode (Join‑Code). |
| `TeamSpeakIP`   | string  | `Your TeamSpeakIp` | TeamSpeak Serveradresse für Auto‑Connect. |
| `AutoTeamSpeak` | bool    | `False`        | Bei Start TS automatisch öffnen. |
| `PureMode`      | bool    | `False`        | FiveM mit `-pure_1` starten. |
| `WaitingTime`   | string  | `100`          | Wartezeit in **ms** (empfohlen: `6000` oder höher). |
| `DevMode`       | bool    | `True`         | Entwicklungsmodus (kann via CLI überschrieben werden). |

> **Pro‑Tipp:** Speichere `WaitingTime` als **ganze Millisekunden** (z. B. `6000`).

---

## 🧑‍💻 Dev Mode & CLI

Der Launcher akzeptiert ein CLI‑Argument `--devmode` (ohne oder mit Wert).  
Ausgewertet wird es in `App.xaml.cs`.

### Beispiele
```bat
:: Einfach einschalten (implizit true)
FusionMods.exe --devmode

:: Explizit setzen
FusionMods.exe --devmode=true
FusionMods.exe --devmode=false
```

### Verhalten
- Das Argument überschreibt den in den Benutzereinstellungen gesetzten Wert `DevMode`.
- Ideal für Tests, Logging & Entwicklungsszenarien.

> **Hinweis:** Weitere CLI‑Parameter kannst du leicht hinzufügen, indem du die Auswertung in `App.xaml.cs` erweiterst.

---

## 🛠️ Build aus dem Source

### Voraussetzungen
- **Windows 10/11**
- **.NET 8 SDK** (https://dotnet.microsoft.com/)
- **Visual Studio 2022** oder `dotnet` CLI
- **(Optional für Pure Mode)** COM‑Referenz **Windows Script Host Object Model** (stellt `IWshRuntimeLibrary` bereit)

> Wenn du `IWshRuntimeLibrary` nicht als COM‑Referenz im Projekt hast, füge sie in Visual Studio hinzu:  
> **Projekt → Verweis hinzufügen… → COM → Windows Script Host Object Model**

### Build mit Visual Studio
1. Repository klonen/entpacken
2. `FusionMods.sln` (falls vorhanden) oder direkt `FusionMods.csproj` öffnen
3. **Konfiguration:** `Release | Any CPU` (oder `x64`)
4. **Build** → **Build**

### Build mit CLI
```bash
# Restore & Compile
dotnet restore
dotnet build -c Release

# Optional: Publish (Framework‑dependent)
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishTrimmed=false

# Optional: Self‑contained Publish
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false
```

> Das Icon **`fusionmods.ico`** ist bereits im Projekt eingebunden.

---

## 🧩 Troubleshooting

- **"FiveM is already opened! Closing?" kommt ständig**  
  → Stelle sicher, dass keine Background‑Instanz hängt. Sonst im Task‑Manager beenden.

- **"You must have Steam open to play"**  
  → Steam muss laufen. Starte Steam zuerst.

- **"FiveM.exe wurde nicht gefunden."**  
  → Standardpfad ist `%LOCALAPPDATA%\FiveM\FiveM.exe`. Prüfe Installation.

- **TeamSpeak startet nicht**  
  → Client muss installiert sein, und der URL‑Handler `ts3server://` registriert. Ggf. neu installieren.

- **Pure Mode hängt / kein Auto‑Join**  
  → `WaitingTime` erhöhen (z. B. `6000`–`10000`) und sicherstellen, dass das FiveM‑**MainWindow** erscheint.

- **Antivirus blockt Shortcut/Start**  
  → Pure Mode nutzt temporäre `.lnk`. Ggf. Projekt/Ordner whitelisten.

---

## 📚 Architektur‑Notizen

- **Prozess‑Handling:** Erst `CloseMainWindow()`, dann `Kill()` (Fallback), `WaitForExit()` mit Timeout empfohlen.
- **Window‑Handling:** `WaitForFiveMWindow()` pollt auf `MainWindowHandle`. Setze ein Timeout, um Endlosschleifen zu vermeiden.
- **Interop:** Für x64‑Sicherheit `GetWindowLongPtr/SetWindowLongPtr` statt `Get/SetWindowLong` nutzen, wenn du das Fenster „hiden“/stylen willst.

---

## 🗺️ Roadmap (Ideen)

- CLI‑Parameter für `--server=<code>`, `--pure`, `--ts=<ip>`, `--wait=<ms>`
- Logging (Serilog) + Log‑Fenster im UI
- Auto‑Update (z. B. via GitHub Releases)
- Optional: **HideFiveMWindow()** on launch (mit Timeout)
- Mehrsprachigkeit (de/en)

---

## 🤝 Contributing

1. Forken 🍴
2. Feature‑Branch: `git checkout -b feature/dein-feature`
3. Committen: `git commit -m "feat: beschreibe dein feature"`
4. Pushen & PR eröffnen

---

## 📜 Lizenz

> **Trage hier deine gewünschte Lizenz ein** (z. B. MIT, GPL‑3.0, Apache‑2.0).  
> Alternativ: Lege eine `LICENSE`‑Datei im Repo an und verlinke sie hier.

---

## 🙌 Credits

- **FiveM / cfx.re** – für die Plattform & Protokolle
- **TeamSpeak** – für `ts3server://` Integration
- **.NET / WPF** – für das App‑Framework

---

<p align="center">
  Made with ❤️ for the RP‑Community · <b>Have fun & drive safe!</b>
</p>
