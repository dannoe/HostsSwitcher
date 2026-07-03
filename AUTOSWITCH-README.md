# Hosts Switcher - Automatic Network-Based Profile Switching

## New Feature: Automatic Hosts Profile Switching

The application now automatically switches to the appropriate hosts profile based on your network connection.

### How It Works

1. **At Startup**: The app checks your current network and automatically loads the matching hosts profile
2. **Network Changes**: When you connect to a different network (WiFi/Ethernet change), the app automatically switches profiles
3. **Manual Override**: You can still manually switch profiles at any time using the existing UI

### Configuring Auto-Switch Rules

Add special comment directives to your hosts profile files to define which networks they should activate for:

```
# HostsSwitcher: gateway=192.168.1.1
# HostsSwitcher: gateway=192.168.0.1
```

**Format**: `# HostsSwitcher: <matchType>=<matchValue>`

**Current Match Types**:
- `gateway` - Matches the default gateway IP address

**Multiple Rules**: You can add multiple directives to one profile. If any rule matches, the profile activates.

### Example Profiles

**Home Network Profile** (`hosts-home`):
```
# HostsSwitcher: gateway=192.168.1.1
# HostsSwitcher: gateway=192.168.0.1

127.0.0.1       localhost
127.0.0.1       dev.local
127.0.0.1       api.dev.local
```

**Office Network Profile** (`hosts-office`):
```
# HostsSwitcher: gateway=10.0.0.1
# HostsSwitcher: gateway=172.16.0.1

127.0.0.1       localhost
10.1.2.100      dev.office.local
10.1.2.101      api.office.local
```

### Behavior

**Match Found**: The app automatically switches to the matching profile and logs the action.

**No Match**: The current hosts file is kept unchanged, and you'll see a Windows notification warning.

**Multiple Matches**: The first matching profile (alphabetically) is used, and you'll see a notification listing all candidates.

**Already Active**: If the matching profile is already in use, no file copy occurs (avoiding unnecessary writes).

### Activity Log

All auto-switch actions are logged in the app window with the `[Auto]` prefix:
```
[Auto] Switched to 'hosts-home'
[Auto] No hosts profile matches the current network
[Auto] Already using 'hosts-office'
```

### Future Extensions

The matching system is designed to be extensible. Future versions can support:
- `ssid` - WiFi network name
- `dnsSuffix` - DNS suffix (e.g., `corp.local`)
- `networkName` - Windows network profile name

### Technical Details

- **Debouncing**: Network change events are debounced (2-second delay) to avoid rapid repeated evaluations
- **Thread Safety**: Network events are marshaled to the UI thread for safe logging and notifications
- **Error Handling**: Malformed directives are silently ignored; the app continues working normally

### Files to Place Rules

The directives should be added to your profile files located in:
```
C:\Windows\System32\drivers\etc\
```

For example:
- `C:\Windows\System32\drivers\etc\hosts-home`
- `C:\Windows\System32\drivers\etc\hosts-office`
- `C:\Windows\System32\drivers\etc\hosts-coffee-shop`

**Note**: The active `hosts` file itself should NOT contain these directives (it's overwritten automatically).
