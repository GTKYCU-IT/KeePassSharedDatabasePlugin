using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using KeePass.Plugins;
using KeePass.Forms;
using KeePass.Util;
using KeePassLib;
using KeePassLib.Collections;

namespace KeePassSharedDatabasePlugin
{
    public sealed class KeePassSharedDatabasePluginExt : Plugin
    {
        private IPluginHost m_host = null;
        private KeePassSharedDatabaseConfig m_config = null;

		public override bool Initialize(IPluginHost host)
		{
			if(host == null) return false;
			m_host = host;

            m_config = new KeePassSharedDatabaseConfig(m_host.CustomConfig);
            // MessageBox.Show(m_config.test);

            m_host.EcasPool.AddActionProvider(
                new OpenUrlActionProvider(config: m_config, host: m_host)
            );

			return true;
		}

        public override void Terminate()
        {
            base.Terminate();
        }

        public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
        {
            if (t == PluginMenuType.Main)
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem();
                tsmi.Text = "Shared Database Options";
                tsmi.Click += this.OnOptionsClicked;
                return tsmi;
            }

            return null;
        }



        private void OnOptionsClicked(object sender, EventArgs e)
        {
            foreach (var group in m_host.Database.RootGroup.Groups) { 
                if (group.Name == "Shared")
                {

                    foreach (var entry in group.Entries)
                    {
                        var bytes = entry.Uuid.ToHexString();
                        MessageBox.Show(entry.Strings.Get("Title").ReadString() + " " + bytes);
                    }
                }
            
            }

        }

    }
}
