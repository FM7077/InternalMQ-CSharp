namespace Das.Infrastructures.MessageQueue
{
    /// <summary>
    /// 标识队列信息
    /// </summary>
    public interface IMessage
    {
        string MessageUid { get; }
    }
}
