using Common.Extensions;
using Common.IO.Collections;
using Common.Pooling.Pools;

using LabExtended.API;

using LabExtended.Hints.Interfaces;
using LabExtended.Hints.Enums;

using LabExtended.Modules;

using Hints;

using HintMessage = LabExtended.API.Messages.HintMessage;

namespace LabExtended.Hints
{
    public class HintModule : Module
    {
        internal static bool ShowDebug;

        private static readonly TextHint _emptyHint = new TextHint(string.Empty, new HintParameter[] { new StringHintParameter(string.Empty) });

        private readonly LockedList<IHintElement> _activeElements = new LockedList<IHintElement>();
        private readonly Queue<HintMessage> _temporaryQueue = new Queue<HintMessage>();

        private TemporaryElement _temporaryElement;
        private StringHintParameter _textParameter;
        private TextHint _textHint;

        private int _fullLeftPosition;
        private bool _clearedAfterEmpty;

        public ExPlayer Player { get; internal set; }

        public override ModuleTickSettings? TickSettings { get; } = new ModuleTickSettings(600f);

        public override void Start()
        {
            base.Start();

            Player = (ExPlayer)Parent;

            _fullLeftPosition = (int)(Math.Round(45.3448f * Player.AspectRatio - 51.527f));
            _textParameter = new StringHintParameter(string.Empty);
            _textHint = new TextHint(string.Empty, new HintParameter[] { _textParameter }, null, 10f);

            _temporaryElement = AddElement<TemporaryElement>();
            _temporaryElement.IsActive = false;
        }

        public override void Stop()
        {
            base.Stop();

            ClearElements();

            _temporaryQueue.Clear();
            _fullLeftPosition = 0;

            Player = null;

            _textHint = null;
            _textParameter = null;
            _temporaryElement = null;
        }

        public void Show(string content, ushort duration, bool isPriority = false)
        {
            if (string.IsNullOrWhiteSpace(content) || duration <= 0)
                return;

            if (isPriority)
            {
                _temporaryElement.Reset();
                _temporaryElement.SetHint(new HintMessage(content, duration));

                return;
            }

            _temporaryQueue.Enqueue(new HintMessage(content, duration));
        }

        public T AddElement<T>() where T : IHintElement
        {
            if (TryGetElement<T>(out var activeElement))
                return activeElement;

            activeElement = typeof(T).Construct<T>();

            activeElement.IsActive = true;
            activeElement.Player = Player;
            activeElement.OnEnabled();

            _activeElements.Add(activeElement);
            return activeElement;
        }

        public bool RemoveElement<T>() where T : IHintElement
        {
            if (!TryGetElement<T>(out var element))
                return false;

            element.IsActive = false;
            element.OnDisabled();
            element.Player = null;

            _activeElements.Remove(element);
            return true;
        }

        public T GetElement<T>() where T : IHintElement
            => _activeElements.TryGetFirst<T>(out var element) ? element : throw new Exception($"Element of type {typeof(T).FullName} was not found.");

        public bool TryGetElement<T>(out T element)
            => _activeElements.TryGetFirst<T>(out element);

        public void ClearElements()
        {
            foreach (var activeElement in _activeElements)
            {
                activeElement.IsActive = false;
                activeElement.OnDisabled();
                activeElement.Player = null;
            }

            _activeElements.Clear();
        }

        public override void Tick()
        {
            base.Tick();

            if (_activeElements.Count(element => element.IsActive) < 1)
            {
                if (!_clearedAfterEmpty)
                {
                    Player.Connection.Send(new global::Hints.HintMessage(_emptyHint));
                    _clearedAfterEmpty = true;
                }

                return;
            }

            var builtElements = InternalBuildString();

            if (string.IsNullOrWhiteSpace(builtElements))
                return;

            if (ShowDebug)
                ServerConsole.AddLog($"\n{builtElements}", ConsoleColor.Red);

            _textParameter.Value = builtElements;
            _textHint.Text = builtElements;
            _clearedAfterEmpty = false;

            Player.Connection.Send(new global::Hints.HintMessage(_textHint));
        }

        internal string InternalBuildString()
        {
            var builder = StringBuilderPool.Shared.Rent();

            builder.Append("\n<line-height=1285%>\n<line-height=0>\n");

            _temporaryElement.CheckDuration();

            if (_temporaryElement._messages.Count < 1 && _temporaryQueue.TryDequeue(out var nextMessage))
                _temporaryElement.SetHint(nextMessage);

            foreach (var element in _activeElements)
            {
                if (!element.IsActive)
                    continue;

                builder.Append($"<voffset={element.VerticalOffset}em>");

                switch (element.Alignment)
                {
                    case HintAlign.FullLeft:
                        builder.Append($"<align=left><pos=-{_fullLeftPosition}%>");
                        element.WriteContent(builder);
                        builder.Append($"</pos></align>");
                        break;

                    case HintAlign.Left:
                        builder.Append($"<align=left>");
                        element.WriteContent(builder);
                        builder.Append($"</align>");
                        break;

                    case HintAlign.Right:
                        builder.Append($"<align=right>");
                        element.WriteContent(builder);
                        builder.Append($"</align>");
                        break;

                    default:
                        element.WriteContent(builder);
                        break;
                }

                builder.Append("\n");
            }

            builder.Append($"<voffset=0><line-height=2100%>\n");
            return StringBuilderPool.Shared.ToStringReturn(builder);
        }
    }
}