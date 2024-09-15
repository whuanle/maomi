using Microsoft.AspNetCore.Mvc;

namespace Demo9.Swagger.Controllers
{
    /// <summary>
    /// øÿ÷∆∆˜A
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AController : ControllerBase
    {
        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test1")]
        public string Get1() => "true";

        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test2")]
        public string Get2() => "true";
    }

    /// <summary>
    /// øÿ÷∆∆˜B
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class BController : ControllerBase
    {
        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test1")]
        public string Get1() => "true";

        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test2")]
        public string Get2() => "true";
    }

    /// <summary>
    /// øÿ÷∆∆˜C
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CController : ControllerBase
    {
        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test1")]
        public string Get1() => "true";

        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test2")]
        public string Get2() => "true";
    }

    /// <summary>
    /// øÿ÷∆∆˜D
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DController : ControllerBase
    {
        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        public string Get() => "true";
    }

    /// <summary>
    /// øÿ÷∆∆˜E
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "øÿ÷∆∆˜E")]
    public class EController : ControllerBase
    {
        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        public string Get() => "true";
    }

    /// <summary>
    /// øÿ÷∆∆˜F
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "øÿ÷∆∆˜F")]
    public class FController : ControllerBase
    {
        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        public string Get() => "true";
    }

    /// <summary>
    /// øÿ÷∆∆˜G
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class GController : ControllerBase
    {
        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        public string Get() => "true";
    }

    /// <summary>
    /// øÿ÷∆∆˜H
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "øÿ÷∆∆˜H")]
    public class HController : ControllerBase
    {
        /// <summary>
        /// ≤‚ ‘
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        public string Get() => "true";
    }
}