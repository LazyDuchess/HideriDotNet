using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TF2Module.SchemaItems
{
    public class TF2SchemaItems
    {
        public TF2SchemaItemsResult result;
    }

    public class TF2SchemaItemsResult
    {
        public int status;
        public List<TF2SchemaItem> items;
    }

    public class TF2SchemaItem
    {
        public ulong id;
        public ulong defindex;
        public string item_name;
        public string item_description;
        public string image_url_large;
    }
}
