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


## Configuration

The plugin does nothing on its own and requires a trigger be configured in order to open any shared databases.

### Example Trigger

The following example trigger will open shared databases in the group tagged with `SHARED_GROUP` upon opening a database whose name matches the current user's username:

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

## License
This project is licensed under the MIT License.
