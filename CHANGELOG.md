# Changelog

## 1.1.0.0

Changed
- Entries whose `db_path` is blank, or resolves to blank, are now skipped without a prompt. 1.0.0.0 asked KeePass to open an empty path, which showed "The specified file could not be found."

Removed
- The debug menu item **Shared Database Options** (main menu), which showed entry titles and UUIDs from a group named `Shared`.
- An unused plugin configuration setting and unused internal helpers.

Internal
- The trigger action UUID (`VOK1miBoR/299B1UelKmTQ==`) and the action name are unchanged, so existing triggers keep working.
- The project is now an SDK-style project: build with `dotnet build -c Release`, no Visual Studio needed. It compiles against the installed `KeePass.exe`, which can be overridden with `-p:KeePassExe=...`.
- Assembly metadata, copyright and license now name Greater Kentucky Credit Union, Inc.
- Added `test-fixtures/` and a manual test checklist (see README).

## 1.0.0.0

- First release: the **Open Shared Databases** trigger action, with the `db_path` and `auto_open` entry fields.
