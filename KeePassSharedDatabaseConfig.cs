using KeePass.App.Configuration;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeePassSharedDatabasePlugin
{

    public class KeePassSharedDatabaseConfig
    {
        const string BASE_64_STRING = "VOK1miBoR/299B1UelKmTQ==";

        private AceCustomConfig customConfig;
        private PwUuid actionType;

        public KeePassSharedDatabaseConfig(AceCustomConfig customConfig) { 
            this.customConfig = customConfig;
        }

        public PwUuid OpenUrlActionTypeUuid {
            get {
                if (actionType == null)
                {
                    actionType = new PwUuid(System.Convert.FromBase64String( BASE_64_STRING ));
                }

                return actionType;
            }
        }
        public string test {
            get {
                return this.customConfig.GetString(
                    "KeePassSharedDatabaseConfig_test",
                    ""
                );
            }

            set {
                this.customConfig.SetString(
                    "KeePassSharedDatabaseConfig_test",
                    value
                );
            }
        }
    }
}
