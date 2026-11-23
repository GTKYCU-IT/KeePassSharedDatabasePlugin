using KeePass.Plugins;

namespace KeePassSharedDatabasePlugin
{
    /// <summary>
    /// KeePass plugin entry point. KeePass locates it by convention: the class must be named
    /// "&lt;AssemblyName&gt;Ext" and live in the namespace "&lt;AssemblyName&gt;".
    /// </summary>
    public sealed class KeePassSharedDatabasePluginExt : Plugin
    {
        public override bool Initialize(IPluginHost host)
        {
            if (host == null)
            {
                return false;
            }

            host.EcasPool.AddActionProvider(new OpenSharedDatabasesActionProvider(host));
            return true;
        }
    }
}
