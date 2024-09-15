using Maomi.Module;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maomi.Core.Tests.Module1
{
	public class AModule : IModule
    {
        private readonly IConfiguration _configuration;
        private readonly RecordInfo _recordInfo;
        private readonly IHostService _hostService;

        public AModule(IConfiguration configuration, RecordInfo recordInfo, IHostService hostService)
        {
            _configuration = configuration;
            _recordInfo = recordInfo;
            _hostService = hostService;
        }
        public void ConfigureServices(ServiceContext context)
        {
            _recordInfo.Add(typeof(AModule).Name);
        }
    }

    [InjectModule<AModule>]
    public class BModule : IModule
    {
        private readonly IConfiguration _configuration;
        private readonly RecordInfo _recordInfo;
        public BModule(IConfiguration configuration, RecordInfo recordInfo)
        {
            _configuration = configuration;
            _recordInfo = recordInfo;
        }

        public void ConfigureServices(ServiceContext context)
        {
            _recordInfo.Add(typeof(BModule).Name);
        }
    }

    [InjectModule<AModule>]
    public class CModule : IModule
    {
        private readonly IConfiguration _configuration;
        private readonly RecordInfo _recordInfo;
        public CModule(IConfiguration configuration, RecordInfo recordInfo)
        {
            _configuration = configuration;
            _recordInfo = recordInfo;
        }

        public void ConfigureServices(ServiceContext context)
        {
            _recordInfo.Add(typeof(CModule).Name);
        }
    }

    [InjectModule<BModule>]
    [InjectModule<CModule>]
    public class DModule : IModule
    {
        private readonly IConfiguration _configuration;
        private readonly RecordInfo _recordInfo;
        public DModule(IConfiguration configuration, RecordInfo recordInfo)
        {
            _configuration = configuration;
            _recordInfo = recordInfo;
        }
        public void ConfigureServices(ServiceContext context)
        {
            _recordInfo.Add(typeof(DModule).Name);
        }
    }
}
