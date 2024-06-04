namespace LabExtended.Core.Commands.Responses
{
    public struct ContinuedResponse : ICommandResponse
    {
        public string Text { get; }
        public bool IsSuccess { get; }
        public float Timeout { get; }

        public Action<ContinuedCommandContext> Callback { get; }

        public ContinuedResponse(string text, bool success, float timeout, Action<ContinuedCommandContext> callback)
        {
            Text = text;
            Timeout = timeout;
            Callback = callback;
            IsSuccess = success;
        }
    }
}