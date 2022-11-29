using System.Threading.Tasks;
using System.Threading;

namespace Das.Infrastructures.MessageQueue
{
    /// <summary>
    /// 消息处理接口
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMsgListener<TMessage>
        where TMessage : IMessage
    {
        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ct"></param>
        Task DealMsg(TMessage e, CancellationToken cancellationToken);
    }
}
