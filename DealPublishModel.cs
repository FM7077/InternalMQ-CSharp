using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;

namespace Das.Infrastructures.MessageQueue
{
    public class DealPublishModel
    {
        private readonly ILogger _logger;
        private readonly List<Object> _listeners;
        private readonly CancellationToken _cancellationToken;

        public DealPublishModel(ILogger logger
            , List<Object> listeners
            , CancellationToken cancellationToken)
        {
            _logger = logger;
            _listeners = listeners;
            _cancellationToken = cancellationToken;
        }

        public List<Task> ProcessQueues<TMessage>(TMessage e) where TMessage : class, IMessage
        {
            List<Task> listenerTask = new List<Task>();
            foreach(var listener in _listeners)
            {
                var lst = listener as IMsgListener<TMessage>;
                var task = lst.DealMsg(e, _cancellationToken);
                listenerTask.Add(task);
            }
            return listenerTask;
        }
    }
}
