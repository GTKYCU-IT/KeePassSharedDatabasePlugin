using System;
using KeePass.Ecas;
using KeePass.Plugins;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Serialization;

namespace KeePassSharedDatabasePlugin
{
    /// <summary>
    /// Registers the "Open Shared Databases" trigger action. The action takes a group tag,
    /// finds the first top-level group carrying that tag in the active database, and opens
    /// the shared database described by each entry in that group.
    /// </summary>
    public sealed class OpenSharedDatabasesActionProvider : EcasActionProvider
    {
        /// <summary>
        /// Saved triggers (including the enforced configuration) reference this action by
        /// this UUID. Changing it breaks every deployed trigger.
        /// </summary>
        public static readonly PwUuid ActionTypeUuid =
            new PwUuid(Convert.FromBase64String("VOK1miBoR/299B1UelKmTQ=="));

        private const string DbPathField = "db_path";
        private const string AutoOpenField = "auto_open";

        private readonly IPluginHost host;

        public OpenSharedDatabasesActionProvider(IPluginHost host)
        {
            this.host = host;

            EcasParameter groupTag = new EcasParameter("Group Tag", EcasValueType.String, null);

            m_actions.Add(new EcasActionType(
                ActionTypeUuid,
                "Open Shared Databases",
                PwIcon.World,
                new EcasParameter[] { groupTag },
                OpenSharedDatabases));
        }

        private void OpenSharedDatabases(EcasAction action, EcasContext context)
        {
            // Capture the database that fired the trigger; opening shared databases
            // changes host.Database to the most recently opened one.
            PwDatabase personalDb = host.Database;

            foreach (string tag in action.Parameters)
            {
                PwGroup sharedGroup = FindTopLevelGroupByTag(personalDb, tag);
                if (sharedGroup == null)
                {
                    continue;
                }

                foreach (PwEntry entry in sharedGroup.Entries)
                {
                    if (!ShouldOpen(entry))
                    {
                        continue;
                    }

                    // A blank db_path would make KeePass try to open an empty path; skip it.
                    string path = ResolveDatabasePath(entry);
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    OpenDatabase(path, entry.Strings.ReadSafe(PwDefs.PasswordField));
                }
            }
        }

        /// <summary>Only direct children of the root group are searched; the first match wins.</summary>
        private static PwGroup FindTopLevelGroupByTag(PwDatabase db, string tag)
        {
            foreach (PwGroup group in db.RootGroup.Groups)
            {
                if (group.Tags.Contains(tag))
                {
                    return group;
                }
            }

            return null;
        }

        /// <summary>Entries open unless auto_open is "false" (case- and space-insensitive).</summary>
        private static bool ShouldOpen(PwEntry entry)
        {
            string autoOpen = entry.Strings.ReadSafe(AutoOpenField).Replace(" ", "");
            return !string.Equals(autoOpen, "false", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Compiles KeePass placeholders (e.g. {USERNAME}, %VAR%) in db_path against the entry.</summary>
        private static string ResolveDatabasePath(PwEntry entry)
        {
            string pathTemplate = entry.Strings.ReadSafe(DbPathField);

            SprContext ctx = new SprContext();
            ctx.Entry = entry;

            return SprEngine.Compile(pathTemplate, ctx);
        }

        private void OpenDatabase(string path, string password)
        {
            IOConnectionInfo connection = new IOConnectionInfo();
            connection.Path = path;

            CompositeKey key = new CompositeKey();
            key.AddUserKey(new KcpPassword(password));

            host.MainWindow.OpenDatabase(connection, key, false);
        }
    }
}
