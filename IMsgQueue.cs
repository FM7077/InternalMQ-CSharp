using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Das.Infrastructures.MessageQueue
{
    /// <summary>
    /// 内置消息队列
    /// </summary>
    public interface IMsgQueue
    {
        /// <summary>
        /// 订阅
        /// </summary>
        /// <typeparam name="TMessage">要订阅的事件类型</typeparam>
        /// <param name="msgListener">谁要订阅</param>
        void Subscribe<TMessage>(IMsgListener<TMessage> msgListener) where TMessage : class, IMessage;

        /// <summary>
        /// 发布消息（可等待所以的 Handler 处理完成）
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        Task Publish<TMessage>(TMessage e) where TMessage : class, IMessage;

        /// <summary>
        /// 开始监听并处理队列
        /// </summary>
        void Run();
    }
}
