using Common.Extensions;
using Common.IO.Collections;
using Common.Pooling.Pools;

using LabExtended.API;

using LabExtended.Hints.Elements;
using LabExtended.Hints.Interfaces;

using LabExtended.Ticking;
using LabExtended.Modules;

using Hints;

using HintMessage = LabExtended.API.Messages.HintMessage;

namespace LabExtended.Hints
{
    public class HintModule : Module
    {
        internal static bool ShowDebug;

        private static readonly TextHint _emptyHint = new TextHint(string.Empty, new HintParameter[] { new StringHintParameter(string.Empty) });

        private static readonly LockedDictionary<Type, Func<ExPlayer, bool>> _globalElements = new LockedDictionary<Type, Func<ExPlayer, bool>>();

        private readonly LockedList<IHintElement> _activeElements = new LockedList<IHintElement>();
        private readonly Queue<HintMessage> _temporaryQueue = new Queue<HintMessage>();

        private TemporaryElement _temporaryElement;
        private StringHintParameter _textParameter;
        private TextHint _textHint;

        private int _fullLeftPosition;
        private bool _clearedAfterEmpty;

        public ExPlayer Player { get; internal set; }

        public override TickOptions TickSettings { get; } = TickOptions.GetStatic(550f);

        public override void Start()
        {
            base.Start();

            Player = (ExPlayer)Parent;

            _fullLeftPosition = (int)(Math.Round(45.3448f * Player.AspectRatio - 51.527f));
            _textParameter = new StringHintParameter(string.Empty);
            _textHint = new TextHint(string.Empty, new HintParameter[] { _textParameter }, null, 10f);

            _temporaryElement = AddElement<TemporaryElement>();
            _temporaryElement.IsActive = false;

            foreach (var globalType in _globalElements)
            {
                if (globalType.Value != null && !globalType.Value.Call(Player))
                    continue;

                if (_activeElements.Any(element => element.GetType() == globalType.Key))
                    continue;

                var activeElement = globalType.Key.Construct<IHintElement>();

                activeElement.IsActive = true;
                activeElement.Player = Player;
                activeElement.OnEnabled();

                _activeElements.Add(activeElement);
            }
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

        public T AddElement<T>(T element) where T : IHintElement
        {
            if (TryGetElement<T>(out var activeElement))
                return activeElement;

            if (element.IsActive)
            {
                element.IsActive = false;
                element.OnDisabled();
            }

            element.IsActive = true;
            element.Player = Player;
            element.OnEnabled();

            _activeElements.Add(element);
            return element;
        }

        public bool RemoveElement<T>() where T : IHintElement
        {
            if (!TryGetElement<T>(out var element))
                return false;

            element.IsActive = false;
            element.OnDisabled();

            element.Player = null;
            return _activeElements.Remove(element);
        }

        public bool RemoveElement<T>(T element) where T : IHintElement
        {
            if (!_activeElements.Remove(element))
                return false;

            element.IsActive = false;
            element.OnDisabled();

            element.Player = null;
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

            _temporaryElement.CheckDuration();

            if (!_temporaryElement.IsActive && _temporaryQueue.TryDequeue(out var nextMessage))
                _temporaryElement.SetHint(nextMessage);

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
                ServerConsole.AddLog($"<noparse>\n{builtElements}</noparse>", ConsoleColor.Red);

            _textParameter.Value = builtElements;
            _textHint.Text = builtElements;

            _clearedAfterEmpty = false;

            Player.Connection.Send(new global::Hints.HintMessage(_textHint));
        }

        public static bool AddGlobalElement<T>(Func<ExPlayer, bool> validator = null) where T : IHintElement
        {
            if (_globalElements.ContainsKey(typeof(T)))
                return false;

            _globalElements[typeof(T)] = validator;
            return true;
        }

        public static bool RemoveGlobalElement<T>() where T : IHintElement
            => _globalElements.Remove(typeof(T));

        internal string InternalBuildString()
        {
            var builder = StringBuilderPool.Shared.Rent();

            builder.Append("\n<line-height=1285%>\n<line-height=0>\n");

            foreach (var element in _activeElements)
            {
                if (!element.IsActive)
                    continue;

                builder.Append($"<voffset={element.VerticalOffset}em>");

                string preTag = null;
                string postTag = null;

                switch (element.Alignment)
                {
                    case HintAlign.FullLeft:
                        preTag = $"<align=left><pos=-{_fullLeftPosition}%>";
                        postTag = $"</pos></align>";
                        break;

                    case HintAlign.Left:
                        preTag = $"<align=left>";
                        postTag = $"</align>";
                        break;

                    case HintAlign.Right:
                        preTag = $"<align=right>";
                        postTag = $"</align>";
                        break;
                }

                if (preTag != null)
                    builder.Append(preTag);

                element.Builder.Clear();
                element.Write();

                if (element.Builder.Length > 0)
                {
                    var writtenString = element.Builder.ToString();
                    var writtenLines = writtenString.SplitLines();

                    for (int i = 0; i < writtenLines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(writtenLines[i]))
                            continue;

                        if (i == 0)
                        {
                            builder.Append($"\n{writtenLines[i].Remove("\n")}\n");
                            continue;
                        }

                        builder.Append($"\n<voffset={element.VerticalOffset - (i + 0.5)}em>{writtenLines[i].Remove("\n")}</voffset>\n");
                    }
                }

                if (postTag != null)
                    builder.Append(postTag);

                builder.Append("\n");
            }

            builder.Append($"<voffset=0><line-height=2100%>\n");
            return StringBuilderPool.Shared.ToStringReturn(builder);
        }
    }
}