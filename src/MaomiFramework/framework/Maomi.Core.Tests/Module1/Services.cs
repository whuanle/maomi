using Maomi.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maomi.Core.Tests.Module1
{
    public interface IA { }
    public interface IB { }
    public interface IC { }


    public class ParentService { }
    [InjectOn]
    public class MyService : ParentService, IA, IB, IC { }
}
