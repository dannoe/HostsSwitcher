# Hosts Switcher

A Windows system tray utility that lets developers quickly switch between hosts file profiles — local, QA, production, or any other environment — by replacing `C:\Windows\System32\drivers\etc\hosts` with a predefined profile file.

Web developers working across multiple environments often need to toggle DNS overrides frequently. Hosts Switcher makes this instant: pick a profile and it is applied immediately. It also supports **automatic switching** based on your network gateway, so the right profile activates the moment you connect to a different network.

> **Note:** Web browsers maintain their own DNS cache. After switching profiles you may need to close and reopen your browser, or use a browser extension such as DNS Flusher (Firefox) to pick up the change immediately.

![Hosts Switcher Screenshot](/hosts-switcher.png)

## Requirements

- **Windows** (x64)
- **Administrator privileges** — required to write to `C:\Windows\System32\drivers\etc\hosts`
- No separate .NET runtime installation needed when using the self-contained release binary

## Installation

1. Download the latest `HostsSwitcher-win-x64-vX.Y.Z.zip` from the [Releases](../../releases) page.
2. Extract the ZIP to any folder (e.g. `C:\Tools\HostsSwitcher\`).
3. Run `HostsSwitcher.exe`. Windows will prompt for administrator elevation.
4. The app minimises to the system tray — look for the network icon.

### Building from source

```
dotnet publish HostsSwitcher.csproj --configuration Release --self-contained true --runtime win-x64
```

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

## Features

### Profile management

All files in `C:\Windows\System32\drivers\etc\` are listed as profiles (built-in system files are excluded). From the main window you can:

| Action | How |
|---|---|
| **Activate** a profile | Select it and click *Use as hosts*, or double-click it |
| **Copy** a profile to a new name | Select it and click *Copy* |
| **Delete** a profile | Select it and click *Delete* |
| **Edit** a profile | Select it and click *View/Edit* (opens Notepad) |
| **Open the folder** | Click *Open Folder* |

### Quick switch from the tray

Right-click the tray icon and use the **Quick Switch** submenu to activate any profile without opening the main window.

### Automatic network-based switching

The app watches for network changes (address or availability) and automatically activates the profile whose rules match your current network gateway. A 2-second debounce prevents rapid repeated evaluations.

See **[Configuring auto-switch rules](#configuring-auto-switch-rules)** below.

### Autostart with Windows

Toggle **Autostart** from the toolbar or tray context menu. When enabled, a Windows Task Scheduler entry is created that launches the app at logon in the background (minimised to tray) with the highest privilege level so it can write hosts without a UAC prompt each time.

### Activity log

All actions are shown in the main window log. Auto-switch events are prefixed with `[Auto]`:

```
[Auto] Switched to 'hosts-home'
[Auto] Already using 'hosts-office'
[Auto] No hosts profile matches the current network
```

## Configuring auto-switch rules

Add special comment directives to your profile files to tell the app which network they belong to:

```
# HostsSwitcher: gateway=192.168.1.1
```

**Format:** `# HostsSwitcher: <matchType>=<matchValue>`

**Supported match types:**

| Match type | Description |
|---|---|
| `gateway` | Default gateway IP address of the active network adapter |

Multiple directives in one file act as **OR** — the profile activates if *any* rule matches.

### Example: home network profile (`hosts-home`)

```
# HostsSwitcher: gateway=192.168.1.1
# HostsSwitcher: gateway=192.168.0.1

127.0.0.1       localhost
::1             localhost
127.0.0.1       dev.local
127.0.0.1       api.dev.local
```

### Example: office network profile (`hosts-office`)

```
# HostsSwitcher: gateway=10.0.0.1
# HostsSwitcher: gateway=172.16.0.1

127.0.0.1       localhost
::1             localhost
10.1.2.100      dev.office.local
10.1.2.101      api.office.local
```

> The active `hosts` file itself should **not** contain these directives — it is overwritten automatically.

### Auto-switch behaviour

| Situation | Result |
|---|---|
| One match found | Profile is activated; logged as `[Auto] Switched to '...'` |
| Profile already active | No file write; logged as `[Auto] Already using '...'` |
| No match found | Hosts file unchanged; tray notification warning shown |
| Multiple matches | First match (alphabetically) is used; tray notification lists all candidates |

### Profile file locations

Place your profile files alongside `hosts` in:

```
C:\Windows\System32\drivers\etc\
```

Examples:
- `hosts-home`
- `hosts-office`
- `hosts-coffee-shop`

Sample profile files (`EXAMPLE-hosts-home.txt` and `EXAMPLE-hosts-office.txt`) are included in the release ZIP.
