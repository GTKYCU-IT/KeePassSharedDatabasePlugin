# KeePassSharedDatabasePlugin

A KeePass 2.x plugin that adds one trigger action, **Open Shared Databases**. When a trigger runs it, the plugin finds a group in your personal database tagged with a chosen tag, then opens the shared database described by each entry in that group, using the master password stored in the entry.

Typical use: one trigger opens a user's personal database, and all of the shared databases that user has access to open with it, with no extra password prompts.

> **Internal deployment docs (GTKYCU staff):** the Confluence page *KeePass Shared Database Plugin* covers how the plugin is deployed, how to release it, and troubleshooting.

## How it works

1. A KeePass trigger, usually on the event **Opened database file**, runs the **Open Shared Databases** action with a **Group Tag** parameter.
2. The plugin looks through the **top-level groups** of the database that fired the trigger. It picks the **first** group whose tags contain the Group Tag. The comparison is exact and case-sensitive. Nested groups are never searched.
3. For each entry in that group (not subgroups), in order:
   - It skips the entry if `auto_open` is `false`. Case and spaces are ignored.
   - It compiles `db_path` with the KeePass placeholder engine, in the context of that entry, and skips the entry if the result is blank.
   - It opens the resulting path with the entry's **Password** field as the master password.
4. KeePass's own error dialog appears if a database can't be opened, for example because of a wrong password or a missing file. The plugin then moves on to the next entry.

### Entry fields

| Field | Required | Meaning |
|---|---|---|
| `db_path` (custom string field) | yes | Path or URL of the shared database. KeePass placeholders are allowed, for example `{APPDIR}` or `%USERNAME%`. |
| Password (standard field) | yes | The shared database's master password. |
| `auto_open` (custom string field) | no | Set to `false` to keep the entry without opening its database. Any other value, or no field, means the database opens. |

**Limitations:**
- Shared databases must be protected by a **master password only**. Key files, Windows user account keys and key providers such as YubiKey are not supported.
- Only one group is used per tag: the first top-level match.

## Configuring the trigger

The plugin does nothing until a trigger uses its action. The action's type UUID is `VOK1miBoR/299B1UelKmTQ==`. It is fixed, and saved triggers reference the action by it.

The example below opens shared databases from the group tagged `SHARED_GROUP` whenever a database whose path ends with `%USERNAME%.kdbx` is opened. It then switches back to the tab of the database that fired the trigger.

### For a single machine: paste into the Triggers dialog

Copy this XML. In KeePass, go to **Tools → Triggers → Tools → Paste Triggers from Clipboard**.

```xml
<?xml version="1.0" encoding="utf-8"?>
<TriggerCollection xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <Triggers>
        <Trigger>
            <Guid>UtMRD9sIA0yRI9r5k1rrvA==</Guid>
            <Name>Auto-Open Shared Databases</Name>
            <Events>
                <Event>
                    <TypeGuid>5f8TBoW4QYm5BvaeKztApw==</TypeGuid>
                    <Parameters>
                        <Parameter>3</Parameter>
                        <Parameter>%USERNAME%.kdbx</Parameter>
                    </Parameters>
                </Event>
            </Events>
            <Conditions />
            <Actions>
                <Action>
                    <TypeGuid>VOK1miBoR/299B1UelKmTQ==</TypeGuid>
                    <Parameters>
                        <Parameter>SHARED_GROUP</Parameter>
                    </Parameters>
                </Action>
                <Action>
                    <TypeGuid>P7gzLdYWToeZBWTbFkzWJg==</TypeGuid>
                    <Parameters>
                        <Parameter />
                        <Parameter>1</Parameter>
                    </Parameters>
                </Action>
            </Actions>
        </Trigger>
    </Triggers>
</TriggerCollection>
```

| XML | Meaning |
|---|---|
| `5f8TBoW4QYm5BvaeKztApw==` | Event: *Opened database file*. Parameter `3` means "ends with", followed by the file filter. |
| `VOK1miBoR/299B1UelKmTQ==` | Action: *Open Shared Databases* (this plugin). The parameter is the Group Tag. |
| `P7gzLdYWToeZBWTbFkzWJg==` | Action: *Activate database (select tab)*. An empty File/URL with filter `1` (Triggering) switches back to the personal database. |

### For managed machines: the enforced configuration file

In KeePass 2.54 and later, triggers are saved in `KeePass.config.enforced.xml` in the KeePass program folder. That's also the file administrators deploy. Put the same `<Trigger>` inside `/Configuration/Application/TriggerSystem/Triggers`:

```xml
<Configuration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Application>
    <TriggerSystem>
      <Triggers>
        <!-- other triggers stay here unchanged -->
        <Trigger>
          <Guid>UtMRD9sIA0yRI9r5k1rrvA==</Guid>
          <!-- ...same content as above... -->
        </Trigger>
      </Triggers>
    </TriggerSystem>
  </Application>
</Configuration>
```

By default, KeePass uses the enforced file's trigger list **in place of** the user's own triggers. Triggers are matched by their `<Guid>`. When you edit the file:
- change only the `<Trigger>` whose action `TypeGuid` is `VOK1miBoR/299B1UelKmTQ==`;
- keep that trigger's existing `<Guid>`;
- leave every other trigger alone.

See KeePass's [Enforced Configuration](https://keepass.info/help/kb/config_enf.html) page for details.

## Installing

1. Download `KeePassSharedDatabasePlugin.dll` from [Releases](https://github.com/GTKYCU-IT/KeePassSharedDatabasePlugin/releases).
2. Copy it into the `Plugins` folder inside the KeePass program folder.
3. Restart KeePass. It should appear under **Tools → Plugins**.

KeePass keeps the DLL locked while it runs, so close KeePass before replacing the DLL.

## Building

You need:
- the [.NET SDK](https://dotnet.microsoft.com/download) (8 or later). Visual Studio is optional; the .NET Framework 4.8.1 reference assemblies come from NuGet;
- KeePass 2.x installed. The build compiles against `KeePass.exe`.

```powershell
dotnet build -c Release
# Output: bin\Release\net481\KeePassSharedDatabasePlugin.dll
```

The build looks for `C:\Program Files\KeePass Password Safe 2\KeePass.exe` by default. To use another copy:

```powershell
dotnet build -c Release -p:KeePassExe="D:\Tools\KeePass\KeePass.exe"
```

The build stops with a clear error if `KeePass.exe` isn't found.

### Code map

| File | What it does |
|---|---|
| `KeePassSharedDatabasePluginExt.cs` | Plugin entry point. KeePass finds it by naming convention (`<AssemblyName>Ext`) and calls `Initialize`, which registers the action provider. |
| `OpenSharedDatabasesActionProvider.cs` | Defines the trigger action (its UUID, name and Group Tag parameter) and all of the open logic described in [How it works](#how-it-works). |
| `KeePassSharedDatabasePlugin.csproj` | Build settings, the version number, and the assembly metadata KeePass shows. `Product` must stay `KeePass Plugin`, or KeePass won't load the DLL. |
| `test-fixtures/` | Dummy databases and a test configuration for the manual test checklist. |

## Releasing

1. On a branch, update `<Version>` in `KeePassSharedDatabasePlugin.csproj` and add an entry to `CHANGELOG.md`. Merge the change to `master` through a PR.
2. Build from the merged `master`, in a clean checkout (`git status` shows nothing to commit):
   ```powershell
   Remove-Item -Recurse -Force bin, obj -ErrorAction SilentlyContinue
   dotnet build -c Release
   git rev-parse HEAD    # full commit SHA, used in step 4
   ```
3. Run the [manual test checklist](#manual-test-checklist) against **that** DLL. Don't rebuild afterwards: the bytes you tested are the bytes you publish.
4. Publish, pinned to the commit you built. `--target` needs the full 40-character SHA. Without it, GitHub tags whatever `master` points to at that moment.
   ```powershell
   gh release create vX.Y.Z.W bin\Release\net481\KeePassSharedDatabasePlugin.dll --target <full SHA> --title vX.Y.Z.W --notes "<CHANGELOG entry>"
   ```
5. Download the release asset, and confirm both that its SHA-256 equals your local build and that `git rev-list -n 1 vX.Y.Z.W` equals the SHA you built. Deploy the downloaded file. GTKYCU staff: the Confluence runbook has a script that does this check, and the deployment steps.

## Manual test checklist

There are no automated tests. The plugin is a thin layer over KeePass's UI and trigger system. Run this checklist before every release; it takes about five minutes.

**Setup.** This uses a throwaway portable KeePass, so your installed KeePass and its configuration stay untouched.

1. Download the portable **KeePass 2.x ZIP** from [keepass.info](https://keepass.info/download.html) and unzip it to a scratch folder, e.g. `D:\temp\kp-test\`.
2. Copy the newly built `KeePassSharedDatabasePlugin.dll` into `D:\temp\kp-test\Plugins\`.
3. Copy the repo's `test-fixtures` folder to `D:\temp\kp-test\test-fixtures\`.
4. Copy `test-fixtures\KeePass.config.enforced.xml` to `D:\temp\kp-test\` (next to `KeePass.exe`). It holds a test trigger that fires on `personal-test.kdbx`. It also turns off the update prompt and single-instance mode, so the test copy doesn't hand the file over to a KeePass you already have open.
5. Run:
   ```powershell
   D:\temp\kp-test\KeePass.exe D:\temp\kp-test\test-fixtures\personal-test.kdbx -pw:personal
   ```

**Fixture passwords** (dummy values; do not reuse them anywhere):

| Database | Password |
|---|---|
| `personal-test.kdbx` | `personal` |
| `shared-a.kdbx` … `shared-d.kdbx` | `shared-a` … `shared-d` |

`personal-test.kdbx` contains a top-level group **Shared Databases** (tag `SHARED_GROUP`) with five entries. Their `db_path` values use `{APPDIR}\test-fixtures\…`. It also contains an **Untagged** group, and a tagged group nested under **Parent**.

**Expected results**, in this order:

| # | Check | Expected |
|---|---|---|
| 1 | Entry "3 Skipped: blank db_path" | No dialog. (Version 1.0.0.0 showed "The specified file could not be found.") |
| 2 | Entry "4 Wrong password: Shared C" | KeePass error dialog: `…\shared-c.kdbx` / "Failed to load the specified file!" / "The master key is invalid!" Click OK. |
| 3 | Entry "5 Missing file" | KeePass error dialog: `…\does-not-exist.kdbx` / "The specified file could not be found." Click OK. |
| 4 | Tabs after the dialogs | Exactly two tabs: `personal-test.kdbx` (selected) and `shared-a.kdbx`. There is no `shared-b` tab (`auto_open = False`) and no `shared-d` tab (the untagged and nested groups are ignored). |
| 5 | **Tools → Plugins** | *KeePassSharedDatabasePlugin* is listed with the new version. |
| 6 | **Tools → Triggers →** *Auto-Open Shared Databases (test)* **→ Edit → Actions** | The action *Open Shared Databases* has a **Group Tag** text field set to `SHARED_GROUP`. |

Close the test KeePass without saving, and delete the scratch folder when you're done.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

## License

MIT. See [LICENSE.txt](LICENSE.txt).
