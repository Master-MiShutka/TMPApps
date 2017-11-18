using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.ARMTES.Model
{
    public class PageResult<T>
    {
        public List<T> Items { get; set; }
        public Uri NextPageLink { get; set; }
        public int Count { get; set; }
    }
}