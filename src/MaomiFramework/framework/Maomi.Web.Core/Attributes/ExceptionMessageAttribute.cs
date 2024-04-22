namespace Maomi.Attributes
{
    /// <summary>
    /// 错误信息.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ExceptionMessageAttribute : Attribute
    {
        /// <summary>
        /// 错误信息.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ExceptionMessageAttribute(string message)
        {
            Message = message;
        }
    }
}
