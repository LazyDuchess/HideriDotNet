using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    public abstract class BotModule
    {
        public abstract string GetName();

        public abstract string GetDescription();

        public abstract void Initialize(Program botCore);

        public abstract void CleanUp();
    }
}
