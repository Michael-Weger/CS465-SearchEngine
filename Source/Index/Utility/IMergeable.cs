using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CS465_SearchEngine.Source.Index.Utility
{
    public interface IMergeable
    {
        public void MergeInto(IMergeable toMergeInto);
    }
}
