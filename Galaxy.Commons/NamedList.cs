using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy.Commons
{
    public class NamedList<T> : List<T> where T : class, INamedElement
    {
        public T? this[string name] =>
            this.FirstOrDefault(x => x.Name == name);
    }
}
