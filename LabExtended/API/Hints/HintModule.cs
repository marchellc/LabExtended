using Common.Extensions;
using Common.IO.Collections;
using Common.Pooling.Pools;

using LabExtended.API.Modules;
using LabExtended.API.Hints.Elements;

using LabExtended.Core;

using LabExtended.Ticking;

using Hints;

using HintMessage = LabExtended.API.Messages.HintMessage;

namespace LabExtended.API.Hints
{
    public class HintModule : GenericModule<ExPlayer>
    {
        internal static bool ShowDebug;

        private static readonly TextHint _emptyHint = new TextHint(string.Empty, new HintParameter[] { new StringHintParameter(string.Empty) });

        private static readonly LockedDictionary<Type, Func<ExPlayer, bool>> _globalElements = new LockedDictionary<Type, Func<ExPlayer, bool>>();

        private readonly LockedList<HintElement> _activeElements = new LockedList<HintElement>();
        private readonly Queue<HintMessage> _temporaryQueue = new Queue<HintMessage>();

        private TemporaryElement _temporaryElement;
        private StringHintParameter _textParameter;
        private TextHint _textHint;

        private bool _clearedAfterEmpty;

        private float _prevAspectRatio;
        private float _curAspectRatio;

        private int _prevFullLeft;
        private int _curFullLeft;

        private int _idClock = 0;

        public override TickOptions TickOptions { get; } = TickOptions.GetStatic(550f);

        public override void OnStarted()
        {
            base.OnStarted();

            _prevFullLeft = 0;
            _curFullLeft = 0;

            _prevAspectRatio = -1f;
            _curAspectRatio = -1f;

            RefreshAspectRatio(true);

            _textParameter = new StringHintParameter(string.Empty);
            _textHint = new TextHint(string.Empty, new HintParameter[] { _textParameter }, null, 10f);

            _temporaryElement = AddElement<TemporaryElement>();
            _temporaryElement.IsActive = false;

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

                activeElement._writer = new HintWriter(activeElement.VerticalOffset);

                _activeElements.Add(activeElement);
            }
        }

        public override void OnStopped()
        {
            base.OnStopped();

            ClearElements();

            _temporaryQueue.Clear();

            _prevFullLeft = 0;
            _curFullLeft = 0;

            _prevAspectRatio = -1f;
            _curAspectRatio = -1f;

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

            activeElement._writer = new HintWriter(activeElement.VerticalOffset);

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

            element._writer = new HintWriter(element.VerticalOffset);

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

            RefreshAspectRatio(false);

            var builtElements = InternalBuildString();

            if (string.IsNullOrWhiteSpace(builtElements))
                return;

            if (ShowDebug)
                File.WriteAllText($"{ExLoader.Folder}/hint_debug.txt", builtElements);

            _textParameter.Value = builtElements;
            _textHint.Text = builtElements;

            _clearedAfterEmpty = false;

            CastParent.Connection.Send(new global::Hints.HintMessage(_textHint));
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

        internal string InternalBuildString()
        {
            var builder = StringBuilderPool.Shared.Rent();

            builder.Append("~\n<line-height=1285%>\n<line-height=0>\n");

            foreach (var element in _activeElements)
            {
                if (!element.IsActive)
                    continue;

                if (element._prevAlign != element.Alignment)
                    element._prevAlign = element.Alignment;

                if (element._prevOffset != element.VerticalOffset)
                {
                    element._writer._vOffset = element.VerticalOffset;
                    element._prevOffset = element.VerticalOffset;
                }

                if (element.ClearWriter)
                    element._writer.Clear();

                element.Write();

                if (element._writer.Size > 0)
                {
                    foreach (var message in element._writer._messages)
                    {
                        if (string.IsNullOrWhiteSpace(message.Content))
                            continue;

                        if (ShowDebug)
                            ExLoader.Debug("Hint API - InternalBuildString()", $"[FOREACH] Appending message: Content={message.Content} (VerticalOffset={message.VerticalOffset}, Size={message.Size}, Id={message.Id})");

                        builder.Append($"<voffset={message.VerticalOffset}em>");

                        if (element.Alignment is HintAlign.FullLeft)
                            builder.Append($"<align=left><pos=-{_curFullLeft}%>{message.Content}</pos></align>");
                        else if (element.Alignment is HintAlign.Left)
                            builder.Append($"<align=left>{message.Content}</align>");
                        else if (element.Alignment is HintAlign.Right)
                            builder.Append($"<align=right>{message.Content}</align>");
                        else
                            builder.Append(message.Content);
                    }

                    builder.Append("\n");
                }
            }

            builder.Append("<voffset=0><line-height=2100%>\n~");
            return StringBuilderPool.Shared.ToStringReturn(builder);
        }

        private void RefreshAspectRatio(bool isInitial)
        {
            if (isInitial)
            {
                _curAspectRatio = _prevAspectRatio = CastParent.AspectRatio;
                _curFullLeft = _prevFullLeft = (int)Math.Round(45.3448f * CastParent.AspectRatio - 51.527f);
            }
            else
            {
                if (_curAspectRatio != CastParent.AspectRatio)
                {
                    _prevAspectRatio = _curAspectRatio;
                    _curAspectRatio = CastParent.AspectRatio;

                    _prevFullLeft = _curFullLeft;
                    _curFullLeft = (int)Math.Round(45.3448f * CastParent.AspectRatio - 51.527f);
                }
            }
        }
    }
}