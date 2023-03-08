using Maomi.Module;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maomi.Core.Tests.Module1
{


    public class RecordInfo
    {
        private readonly List<string> _list = new List<string>();
        public IReadOnlyList<string> List => _list;
        public void Add(string name)
        {
            _list.Add(name);
        }
    }
}
