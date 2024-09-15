using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    public interface ITest
    {
        string GGG { get; }
    }
    public class Test : ITest
    {
        public string GGG { get { return DateTime.Now.ToLongDateString(); } }
    }
}
