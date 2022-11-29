using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;
using System.Threading;

namespace Das.Infrastructures.MessageQueue
{
    public class MsgQueue : IMsgQueue
    {
        private readonly ILogger _logger;
        /// <summary>
        /// 监听者队列
        /// </summary>
        private readonly ConcurrentDictionary<Type, List<Object>> _listeners;
        /// <summary>
        /// 消息队列
        /// </summary>
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<IMessage>> _queues;

        /// <summary>
        /// 队列最大长度
        /// </summary>
        private readonly int _maxLength = 200;
        /// <summary>
        /// 队列每次消耗并行数
        /// </summary>
        private readonly int _maxConcurrency = 3;
        /// <summary>
        /// 最多队列数
        /// </summary>
        private readonly int _maxQueue = 5;
        /// <summary>
        /// 队列等待时间
        /// </summary>
        private readonly double _queueWaitTime = 5;

        public MsgQueue(ILogger<MsgQueue> logger
            , IOptions<MQOptions> options)
        {
            _logger = logger;
            _listeners = new ConcurrentDictionary<Type, List<Object>>();
            _queues = new ConcurrentDictionary<Type, ConcurrentQueue<IMessage>>();
            _maxLength = options.Value.MQMaxLength;
            _maxConcurrency = options.Value.MQMaxConcurrency;
            _maxQueue = options.Value.MQMaxQueue;
            _queueWaitTime = options.Value.MQWaitTime;
        }

        Task IMsgQueue.Publish<TMessage>(TMessage e)
        {
            return Task.Run(() =>
            {
                var type = typeof(TMessage);
                ConcurrentQueue<IMessage> msgs;
                if (!_queues.TryGetValue(type, out msgs))
                {
                    _logger.LogWarning($"不存在队列[{type}]");
                    return;
                }
                if (msgs.Count <= _maxLength)
                {
                    msgs.Enqueue(e);
                    return;
                }
                _logger.LogWarning($"队列已满, 丢数据[{e.MessageUid}]");
            });
        }

        void IMsgQueue.Subscribe<TMessage>(IMsgListener<TMessage> listener)
        {
            var eType = typeof(TMessage);
            if (null == listener) throw new Exception("订阅者为空");


            List<object> listeners;

            lock (_listeners)
            {
                if (_listeners.Count >= _maxQueue)
                {
                    _logger.LogError($"消息队列达到最大限值[{_maxQueue}], 消息类型[{eType}]将不被处理");
                    return;
                }
                if (!_listeners.TryGetValue(eType, out listeners))
                {
                    listeners = new List<object>();
                    _listeners.TryAdd(eType, listeners);
                }
            }
            lock(_queues)
            {
                if (!_queues.TryGetValue(eType, out ConcurrentQueue<IMessage> msgs))
                {
                    msgs = new ConcurrentQueue<IMessage>();
                    _queues.TryAdd(eType, msgs);
                }
            }

            lock(listeners)
            {
                listeners.Add(listener);
            }
        }

        public void Run()
        {
            _logger.LogInformation($"开始监听队列, 共 [{_queues.Count()}] 条");
            foreach (var queue in _queues)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        if(queue.Value.Count > 0)
                        {
                            var type = queue.Key;
                            try
                            {
                                if (!_listeners.TryGetValue(type, out var listeners))
                                {
                                    _logger.LogWarning($"找不到消息[{type}]的监听者");
                                    break;
                                }

                                var msgCount = queue.Value.Count > _maxConcurrency ? _maxConcurrency : queue.Value.Count;
                                List<Task> queueTasks = new List<Task>();
                                CancellationTokenSource cts = new CancellationTokenSource();
                                CancellationToken ct = cts.Token;
                                for (int i = 0; i < msgCount; i++)
                                {
                                    if (queue.Value.TryDequeue(out var msg))
                                    {
                                        dynamic msgToProcess = Convert.ChangeType(msg, msg.GetType());
                                        var realPublish = new DealPublishModel(_logger, listeners, ct);
                                        var listenerTasks = realPublish.ProcessQueues(msgToProcess);
                                        queueTasks.AddRange(listenerTasks);
                                    }
                                }
                                if(!Task.WhenAll(queueTasks).Wait(TimeSpan.FromSeconds(_queueWaitTime)))
                                {
                                    _logger.LogWarning($"部分任务超时, 取消执行");
                                    cts.Cancel();
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"处理消息队列[{type}]出错: [{ex}]");
                                continue;
                            }
                        }
                        else
                        {
                            Thread.Sleep((int)_queueWaitTime * 5 * 1000);
                        }
                    }
                });
            }
        }
    }
}
