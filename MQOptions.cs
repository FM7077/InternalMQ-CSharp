using System;
using System.Collections.Generic;
using System.Text;

namespace Das.Infrastructures
{
    public class MQOptions
    {

        /// <summary>
        /// 队列最大长度
        /// </summary>
        public int MQMaxLength { get; set; }

        /// <summary>
        /// 每条队列每次并行处理的消息量
        /// </summary>
        public int MQMaxConcurrency { get; set; }

        /// <summary>
        /// 最大队列数
        /// </summary>
        public int MQMaxQueue { get; set; }

        /// <summary>
        /// 每条队列每次批量处理的总等待时间, 单位: 秒
        /// </summary>
        public double MQWaitTime { get; set; }
    }
}
