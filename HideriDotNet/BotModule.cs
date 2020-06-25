using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    public abstract class BotModule
    {
        public string directory;
        public ModuleData data;
        public virtual void Initialize(Program botCore)
        {

        }

        public virtual void CleanUp()
        {

        }

        public virtual void Unload()
        {

        }
    }
}
