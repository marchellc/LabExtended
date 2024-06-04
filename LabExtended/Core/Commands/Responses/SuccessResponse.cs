namespace LabExtended.Core.Commands.Responses
{
    public struct SuccessResponse : ICommandResponse
    {
        public string Text { get; }
        public bool IsSuccess { get; }

        public SuccessResponse(string text)
        {
            Text = text;
            IsSuccess = true;
        }
    }
}