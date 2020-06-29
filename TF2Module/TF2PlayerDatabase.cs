using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TF2Module
{
    class TF2PlayerDatabase
    {
        public bool cached = false;
        public List<ulong> ids = new List<ulong>();
        public List<ulong> defindex = new List<ulong>();
        public DateTime timestamp;
    }
}
