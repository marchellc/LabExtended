﻿using LabExtended.API;

using LabExtended.Commands.Arguments;
using LabExtended.Commands.Contexts;

namespace LabExtended.Commands.Interfaces
{
    public interface ICommandContext
    {
        ICommandResponse Response { get; }

        string RawInput { get; }
        string[] RawArgs { get; }

        ArgumentCollection Args { get; }
        CustomCommand Command { get; }
        ExPlayer Sender { get; }

        bool IsHost { get; }

        void Respond(object response, bool success = true);

        void RespondOk(object response);
        void RespondOk(IEnumerable<object> lines);

        void RespondFail(object response);
        void RespondFail(IEnumerable<object> lines);

        void RespondContinued(object response, Action<ContinuedContext> onContinued);
        void RespondContinued(IEnumerable<object> lines, Action<ContinuedContext> onContinued);
    }
}