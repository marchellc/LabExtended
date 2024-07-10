using Common.Extensions;

using LabExtended.API.Modules;
using LabExtended.API.Hints.Elements;

using LabExtended.Core;

using LabExtended.Ticking;

using Hints;

using HintMessage = LabExtended.API.Messages.HintMessage;
using LabExtended.API.Collections.Locked;

namespace LabExtended.API.Hints
{
    public class HintModule : GenericModule<ExPlayer>
    {
        internal static bool ShowDebug;

        private static readonly TextHint _emptyHint = new TextHint(string.Empty, new HintParameter[] { new StringHintParameter(string.Empty) });

        private static readonly LockedDictionary<Type, Func<ExPlayer, bool>> _globalElements = new LockedDictionary<Type, Func<ExPlayer, bool>>();

        private readonly LockedHashSet<HintElement> _activeElements = new LockedHashSet<HintElement>();
        private readonly Queue<HintMessage> _temporaryQueue = new Queue<HintMessage>();

        private TemporaryElement _temporaryElement;
        private TextHint _textHint;

        private StringHintParameter _textParameter;

        private bool _clearedAfterEmpty;

        private float _aspectRatio;

        private int _leftOffset;
        private int _idClock = 0;

        public override TickOptions TickOptions { get; } = TickOptions.GetStatic(500f);

        public override void OnStarted()
        {
            base.OnStarted();

            _textParameter = new StringHintParameter(string.Empty);
            _textHint = new TextHint(string.Empty, new HintParameter[] { _textParameter }, null, 10f);

            _temporaryElement = AddElement<TemporaryElement>();
            _temporaryElement.IsActive = false;

            InternalRefreshAspectRatio(true);

            foreach (var globalType in _globalElements)
            {
                if (globalType.Value != null && !globalType.Value.Call(CastParent))
                    continue;

                if (_activeElements.Any(element => element.GetType() == globalType.Key))
                    continue;

                var activeElement = globalType.Key.Construct<HintElement>();

                activeElement.IsActive = true;
                activeElement.Player = CastParent;

                activeElement.OnEnabled();

                _activeElements.Add(activeElement);
            }
        }

        public override void OnStopped()
        {
            base.OnStopped();

            ClearElements();

            _temporaryQueue.Clear();

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

        public T AddElement<T>() where T : HintElement
        {
            if (TryGetElement<T>(out var activeElement))
                return activeElement;

            activeElement = typeof(T).Construct<T>();

            activeElement.IsActive = true;
            activeElement.Id = _idClock++;
            activeElement.Player = CastParent;

            activeElement.OnEnabled();

            _activeElements.Add(activeElement);
            return activeElement;
        }

        public T AddElement<T>(T element) where T : HintElement
        {
            if (TryGetElement<T>(out var activeElement))
                return activeElement;

            if (element.IsActive)
            {
                element.IsActive = false;
                element.OnDisabled();
            }

            element.IsActive = true;
            element.Id = _idClock++;
            element.Player = CastParent;

            element.OnEnabled();

            _activeElements.Add(element);
            return element;
        }

        public bool RemoveElement<T>() where T : HintElement
        {
            if (!TryGetElement<T>(out var element))
                return false;

            element.IsActive = false;
            element.OnDisabled();

            element.Player = null;
            return _activeElements.Remove(element);
        }

        public bool RemoveElement<T>(T element) where T : HintElement
        {
            if (!_activeElements.Remove(element))
                return false;

            element.IsActive = false;
            element.OnDisabled();

            element.Player = null;
            return true;
        }

        public static bool AddGlobalElement<T>(Func<ExPlayer, bool> validator = null) where T : HintElement
        {
            if (_globalElements.ContainsKey(typeof(T)))
                return false;

            _globalElements[typeof(T)] = validator;
            return true;
        }

        public static bool RemoveGlobalElement<T>() where T : HintElement
            => _globalElements.Remove(typeof(T));

        public T GetElement<T>() where T : HintElement
            => _activeElements.TryGetFirst<T>(out var element) ? element : throw new Exception($"Element of type {typeof(T).FullName} was not found.");

        public bool TryGetElement<T>(out T element)
            => _activeElements.TryGetFirst(out element);

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

        public override void OnTick()
        {
            base.OnTick();

            _temporaryElement.CheckDuration();

            if (!_temporaryElement.IsActive && _temporaryQueue.TryDequeue(out var nextMessage))
                _temporaryElement.SetHint(nextMessage);

            if (_activeElements.Count(element => element.IsActive) < 1)
            {
                if (!_clearedAfterEmpty)
                {
                    CastParent.Connection.Send(new global::Hints.HintMessage(_emptyHint));
                    _clearedAfterEmpty = true;
                }

                return;
            }

            InternalRefreshAspectRatio(false);

            var builtElements = InternalBuildString();

            if (string.IsNullOrWhiteSpace(builtElements))
                return;

            if (ShowDebug)
                File.WriteAllText($"{ExLoader.Folder}/hint_debug.txt", builtElements);

            if (builtElements.Length >= ushort.MaxValue)
            {
                ExLoader.Warn("Hint API", $"The completed hint is too big! (current: {builtElements.Length}, max size: {ushort.MaxValue - 1})");
                return;
            }

            _textParameter.Value = builtElements;
            _textHint.Text = builtElements;

            _clearedAfterEmpty = false;

            CastParent.Connection.Send(new global::Hints.HintMessage(_textHint));
        }

        internal string InternalBuildString()
        {
            var result = "~\n<line-height=1285%>\n<line-height=0>\n";

            foreach (var element in _activeElements)
            {
                element.UpdateElement();

                if (!element.IsActive)
                    continue;

                var data = element.GetContent();

                if (data is not null && data.Length > 0)
                {
                    if (element.IsRawDisplay)
                    {
                        result += data;
                    }
                    else
                    {
                        if (element.SkipPreviousLine || element._prev is null || element._prev != data)
                        {
                            element._messages.Clear();
                            element._prev = data;

                            data = data.Replace("\r\n", "\n").Replace("\\n", "\n").Replace("<br>", "\n").TrimEnd();

                            HintUtils.TrimStartNewLines(ref data, out var count);

                            var offset = element.VerticalOffset;

                            if (offset == 0f)
                                offset = -count;

                            HintUtils.GetMessages(offset, element.MaxCharactersPerLine, data, element._messages);
                        }

                        foreach (var message in element._messages)
                        {
                            if (string.IsNullOrWhiteSpace(message.Content))
                                continue;

                            result += $"<voffset={message.VerticalOffset}em>";

                            if (element.Alignment is HintAlign.FullLeft)
                                result += $"<align=left><pos=-{_leftOffset}%>{message.Content}</pos></align>";
                            else if (element.Alignment is HintAlign.Left)
                                result += $"<align=left>{message.Content}</align>";
                            else if (element.Alignment is HintAlign.Right)
                                result += $"<align=right>{message.Content}</align>";
                            else
                                result += message.Content;
                        }
                    }

                    result += "\n";
                }
            }

            return result + "<voffset=0><line-height=2100%>\n~";
        }

        private void InternalRefreshAspectRatio(bool isInitial)
        {
            if (isInitial)
            {
                _aspectRatio = CastParent.AspectRatio;
                _leftOffset = (int)Math.Round(45.3448f * CastParent.AspectRatio - 51.527f);
            }
            else
            {
                if (_aspectRatio != CastParent.AspectRatio)
                {
                    _aspectRatio = CastParent.AspectRatio;
                    _leftOffset = (int)Math.Round(45.3448f * CastParent.AspectRatio - 51.527f);
                }
            }
        }
    }
}