# KeePassSharedDatabasePlugin

## Overview
KeePassSharedDatabasePlugin adds a custom Trigger Action that allows users to open shared databases by searching for entries within their personal KeePass database. The plugin uses master passwords stored in specific entries and evaluates a custom property, `db_path`, to locate the shared database.

## Features
- Custom Trigger Action for opening shared databases.
- Requires a tag and a custom `db_path` property.
- Evaluates template strings in the `db_path` property.

## Installation
1. Download the plugin DLL from the [Releases](#) page.
2. Place the DLL in KeePass's `Plugins` directory.
3. Restart KeePass.

## Usage
1. Add the desired tag to a group in your personal database.
2. Ensure each entry in the group contains the `db_path` custom string property, pointing to the shared database.
3. Configure the Trigger Action to use this tag to automatically search and open databases.

## License
This project is licensed under the MIT License.
