namespace WS.Core.DefaultMsg
{
    public enum ErrorCode
    {
        /// <summary>
        /// 序列化错误
        /// </summary>
        SerializerError = 0,
        /// <summary>
        /// 反序列化错误
        /// </summary>
        DeSerializerError = 1,
        /// <summary>
        /// 方法执行错误
        /// </summary>
        FuncExcuteError = 2
    }
}
