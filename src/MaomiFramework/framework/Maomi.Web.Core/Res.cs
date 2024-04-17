using System.Net;

namespace Maomi.Web.Core
{
    /// <summary>
    /// 响应模型类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Res<T>
    {
        /// <summary>
        /// 当前请求是否有错误
        /// </summary>
        public virtual bool IsSuccess => Code == 200;

        /// <summary>
        /// 业务代码
        /// </summary>
        public virtual int Code { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public virtual string Msg { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public virtual T? Data { get; set; }
    }

    /// <summary>
    /// 响应模型类
    /// </summary>
    public partial class Res : Res<object?>
    {
        /// <summary>
        /// 创建 <see cref="Res"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Res<T> Create<T>(int code, string message, T data)
        {
            return new Res<T>
            {
                Code = code,
                Msg = message,
                Data = data
            };
        }

        /// <summary>
        /// 创建 <see cref="Res"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Res Create<T>(int code, string message)
        {
            return new Res
            {
                Code = code,
                Msg = message
            };
        }

        /// <summary>
        /// 创建 <see cref="Res"/>
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Res Create(int code, string message)
        {
            return new Res
            {
                Code = code,
                Msg = message
            };
        }


        /// <summary>
        /// 创建 <see cref="Res"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Res<T> Create<T>(HttpStatusCode code, string message, T data) => Create((int)code, message, data);
    }

    /// <summary>
    /// 分页结果模型类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageRes<T>
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public virtual int PageNo { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public virtual int PageSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual T? List { get; set; }
    }

    /// <summary>
    /// 分页结果模型类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class PageListRes<T> : Res<PageRes<IEnumerable<T>>>
    {
    }

    /// <summary>
    /// 分页结果模型类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class PageArrayRes<T> : Res<PageRes<T[]>>
    {
    }
}
