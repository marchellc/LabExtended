namespace LabExtended.Core.Commands.Responses
{
    public struct ErrorResponse : ICommandResponse
    {
        public Exception Exception { get; }
        public string Text { get; }
        public bool IsSuccess { get; }

        public ErrorResponse(string text, Exception exception)
        {
            Exception = exception;
            Text = text;
            IsSuccess = false;
        }
    }
}
