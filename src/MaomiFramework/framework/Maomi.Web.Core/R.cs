using System.Net;

namespace Maomi.Web.Core
{
    public class R<T>
    {
        public virtual bool IsSuccess => Code == 200;
        public virtual int Code { get; set; }
        public virtual string Msg { get; set; }
        public virtual T? Data { get; set; }
    }

    public partial class R : R<object?> { }
    public class PageData<T>
    {
        public virtual int PageNo { get; set; }
        public virtual int PageSize { get; set; }
        public virtual T? List { get; set; }
    }

    public partial class PageDataList<T> : PageData<IEnumerable<T>?>
    {
    }
    public partial class PageDataArray<T> : PageData<T[]?>
    {
    }

    public partial class R
    {
        public static R<T> Create<T>(int code, string message, T data)
        {
            return new R<T>
            {
                Code = code,
                Msg = message,
                Data = data
            };
        }
        public static R<T> Create<T>(HttpStatusCode code, string message, T data) => Create((int)code, message, data);
    }
}
