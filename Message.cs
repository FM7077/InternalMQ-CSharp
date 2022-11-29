using System;
using System.Collections.Generic;
using System.Text;

namespace Das.Infrastructures.MessageQueue
{
    public class Message : IMessage
    {
        public string MessageUid { get; set; } = Guid.NewGuid().ToString();
    }
}
