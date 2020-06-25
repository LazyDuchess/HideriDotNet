using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    //Module class, main entrypoint for plugins/modules
    public abstract class BotModule
    {
        //Directory this module is located in, for loading of assets, configs, etc.
        public string directory;
        //Module data from data.json
        public ModuleData data;

        //Execute on module load
        public virtual void Initialize(Program botCore)
        {

        }

        //Execute when bot stops
        public virtual void CleanUp()
        {

        }

        //Executed when the module gets unloaded
        public virtual void Unload()
        {

        }
    }
}
