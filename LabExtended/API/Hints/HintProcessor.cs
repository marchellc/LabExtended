using LabExtended.API.Hints.Interfaces;

using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Utilities;

namespace LabExtended.API.Hints;

/// <summary>
/// Processes hint overlays.
/// </summary>
public static class HintProcessor
{
    /// <summary>
    /// Processes hint elements for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ProcessPlayer(ExPlayer player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        var state = HintController.State;
        
        player.Hints.RefreshRatio();

        if (player.Hints.CurrentMessage is null || player.Hints.UpdateTime())
            player.Hints.NextMessage();

        if (player.Hints.CurrentMessage != null)
        {
            player.Hints.ParseTemp();

            state.Parameters.AddRange(player.Hints.CurrentMessage.Parameters);

            if (player.Hints.TempData.Count > 0)
            {
                HintUtils.AppendMessages(player.Hints.TempData, HintController.TemporaryHintAlign, state.Builder, player.Hints.LeftOffset);
                state.AnyAppended = true;
            }
        }

        if (!state.FrameRemoved)
        {
            for (var i = 0; i < state.Remove.Count; i++)
                state.Remove[i].RemoveHintElement();

            state.FrameRemoved = true;
        }

        for (var i = 0; i < HintController.Elements.Count; i++)
        {
            var element = HintController.Elements[i];

            if (!element.IsActive || element.Builder is null)
            {
                state.Remove.Add(element);
                continue;
            }

            if (state.AnyOverrideAll)
                break;
            
            ProcessElement(element, player);
        }

        foreach (var element in player.HintElements)
        {
            if (!element.IsActive || element.Builder is null)
            {
                player.removeNextFrame.Add(element);
                continue;
            }

            if (state.AnyOverrideAll)
                break;

            ProcessElement(element, player);
        }
        
        player.removeNextFrame.Clear();

        if (state.LowestInterval != -1f && state.LowestInterval < state.Interval)
            state.Interval = state.LowestInterval;
    }

    private static void ProcessElement(HintElement element, ExPlayer player)
    {
        var state = HintController.State;

        if (element.tickNum != state.FrameCounter)
        {
            if (element.ClearBuilderOnUpdate)
                element.Builder.Clear();
            
            element.tickNum = state.FrameCounter;
            element.OnUpdate();
        }

        if (element.ClearParameters)
            element.Parameters.Clear();

        if (!element.ClearBuilderOnUpdate)
            element.Builder.Clear();

        if (!element.OnDraw(player) || element.Builder.Length < 1)
            return;

        if ((state.Builder.Length + element.Builder.Length) >= HintController.MaxHintTextLength)
        {
            ApiLog.Warn("Hint API",
                $"Could not append text from element &1{element.GetType().Name}&r for player &3{player.Nickname}&r (&6{player.UserId}&r) " +
                $"due to it exceeding maximum allowed limit (&1{state.Builder.Length + element.Builder.Length}&r / &2{HintController.MaxHintTextLength}&r)");

            return;
        }

        var content = element.Builder.ToString();

        if (element.OverridesOthers)
        {
            state.AnyOverrideAll = true;
            state.Builder.Clear();

            if (element.ShouldParse)
                state.Builder.Append("~\n<line-height=1285%>\n<line-height=0>\n");
            else
                state.AnyOverrideParse = true;
        }

        if (!element.ShouldParse)
        {
            state.Builder.Append(content);
            state.AnyAppended = true;
        }
        else
        {
            if (!element.ShouldCache || element.prevCompiled is null || element.prevCompiled != content)
            {
                element.Data.ForEach(x => ObjectPool<HintData>.Shared.Return(x));
                element.Data.Clear();

                element.prevCompiled = content;

                content = content
                    .Replace("\r\n", "\n")
                    .Replace("\\n", "\n")
                    .Replace("<br>", "\n")
                    .TrimEnd();

                HintUtils.TrimStartNewLines(ref content, out var count);

                var offset = element.GetVerticalOffset(player);

                if (offset == 0f)
                    offset = -count;

                HintUtils.GetMessages(content, element.Data, offset, element.ShouldWrap,
                    element.GetPixelSpacing(player));
            }

            if (element.Data.Count > 0)
            {
                state.Parameters.AddRange(element.Parameters);

                HintUtils.AppendMessages(element.Data, element.GetAlignment(player), state.Builder, player.Hints.LeftOffset);
                
                state.AnyAppended = true;
            }
        }

        if (element is IHintRateModifier hintRateModifier)
        {
            var requestedInterval = hintRateModifier.GetDesiredDelay(state.Interval);

            if (requestedInterval >= 0f && (state.LowestInterval == -1f || requestedInterval < state.LowestInterval))
                state.LowestInterval = requestedInterval;
        }
    }
}