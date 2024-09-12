using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeePass.Ecas;
using KeePass.Plugins;
using KeePass.Util;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePassSharedDatabasePlugin
{

    public sealed class OpenUrlActionProvider : EcasActionProvider
    {
        private IPluginHost pluginHost;
        private KeePassSharedDatabaseConfig _config;

        public OpenUrlActionProvider(KeePassSharedDatabaseConfig config, IPluginHost host)
        {
            pluginHost = host;
            _config = config;

            EcasEnumItem item = new EcasEnumItem(0, this._config.OpenUrlActionTypeUuid.ToString());

            EcasEnum values = new EcasEnum(new EcasEnumItem[] { item });

            EcasParameter vParam = new EcasParameter(
                strName: "Group Tag",
                t: EcasValueType.String,
                eEnumValues: values
            );

            EcasActionType openUrl = new EcasActionType(
                uuidType: this._config.OpenUrlActionTypeUuid,
                strName: "Open Shared Databases",
                pwIcon: PwIcon.World,
                vParams: new EcasParameter[] { vParam },
                f: this.OnOpenUrlActionExecute
            );

            this.m_actions.Add(openUrl);
        }

        private static byte[] ConvertHexToBytes(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];

            for (var i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        private PwEntry FindEntryByHexUuid(string hexUuid)
        {
            try
            {
                PwUuid entryUuid = new PwUuid(ConvertHexToBytes(hexUuid));
                return pluginHost.Database.RootGroup.FindEntry(entryUuid, true);
            }
            catch (Exception)
            {
                return null;
            }
        }
        private PwGroup FindGroupByHexUuid(string hexUuid)
        {
            try
            {
                PwUuid groupUuid = new PwUuid(ConvertHexToBytes(hexUuid));
                return pluginHost.Database.RootGroup.FindGroup(groupUuid, true);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private PwGroup FindGroupByTag(string tag)
        {
            foreach (var group in pluginHost.Database.RootGroup.Groups)
            {
                if (group.Tags.Contains(tag))
                {
                    return group;
                }
            }

            return null;
        }

        private void OpenDatabase(string path, string password)
        {

            IOConnectionInfo conn = new IOConnectionInfo();
            conn.Path = path;

            CompositeKey cmpKey = new CompositeKey();
            cmpKey.AddUserKey(new KcpPassword(password));

            pluginHost.MainWindow.OpenDatabase(
                ioConnection: conn,
                cmpKey: cmpKey,
                bOpenLocal: false
            );
        }

        private static string ConstructDatabasePath(PwEntry entry)
        {
            string pathTemplate = entry.Strings.ReadSafe("db_path");

            SprContext sprCtx = new SprContext();
            sprCtx.Entry = entry;

            return SprEngine.Compile(pathTemplate, sprCtx);
        }

        private static bool ShouldOpen(PwEntry entry)
        {
            return entry.Strings.ReadSafe("auto_open").ToLower().Replace(" ", "") != "false";
        }

        private void OpenSharedDatabases(PwObjectList<PwEntry> entries)
        {
                foreach (PwEntry entry in  entries)
                {
                    if (!ShouldOpen(entry))
                    {
                        continue ;
                    }

                    string path = ConstructDatabasePath(entry);
                    
                    OpenDatabase(path, entry.Strings.ReadSafe("Password"));
                }
        }

        public void OnOpenUrlActionExecute(EcasAction a, EcasContext ctx) {
            foreach (string tag in a.Parameters)
            {
                PwGroup sharedGroup = FindGroupByTag(tag);
                if (sharedGroup == null)
                {
                    continue;
                }

                OpenSharedDatabases(sharedGroup.Entries);
            }
        }
    }
}
