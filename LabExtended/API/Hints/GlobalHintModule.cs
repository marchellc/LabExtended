using LabExtended.API.Collections.Locked;
using LabExtended.API.Enums;
using LabExtended.API.Modules;

using LabExtended.Core.Ticking;

using LabExtended.Extensions;
using LabExtended.Utilities;

namespace LabExtended.API.Hints
{
    public class GlobalHintModule : Module
    {
        public static GlobalHintModule Instance { get; private set; }

        private readonly LockedHashSet<HintElement> _activeElements = new LockedHashSet<HintElement>();
        private int _idClock = 0;

        public IReadOnlyList<HintElement> Elements => _activeElements;

        public override TickTimer TickTimer { get; } = TickTimer.GetStatic(500f, false, true);

        public override bool ValidateAdd(Module module)
            => base.ValidateAdd(module) && Instance is null;

        public override void OnStarted()
        {
            base.OnStarted();

            _idClock = 0;

            Instance = this;
        }

        public override void OnStopped()
        {
            base.OnStopped();

            Instance = null;

            _idClock = 0;

            ClearElements();
        }

        public T AddElement<T>(string customId = null) where T : HintElement
        {
            if (TryGetElement<T>(out var activeElement))
                return activeElement;

            activeElement = typeof(T).Construct<T>();

            activeElement.IsActive = true;
            activeElement.Id = _idClock++;

            if (!string.IsNullOrWhiteSpace(customId))
                activeElement.CustomId = customId;

            activeElement.OnEnabled();

            _activeElements.Add(activeElement);
            return activeElement;
        }

        public T AddElement<T>(T element, string customId = null) where T : HintElement
        {
            if (element.IsActive)
            {
                element.IsActive = false;
                element.OnDisabled();
            }

            element.IsActive = true;
            element.Id = _idClock++;

            if (!string.IsNullOrWhiteSpace(customId))
                element.CustomId = customId;

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

            return _activeElements.Remove(element);
        }

        public bool RemoveElement<T>(string customId) where T : HintElement
        {
            if (!TryGetElement<T>(customId, out var element))
                return false;

            element.IsActive = false;
            element.OnDisabled();

            return _activeElements.Remove(element);
        }

        public bool RemoveElement<T>(T element) where T : HintElement
        {
            if (!_activeElements.Remove(element))
                return false;

            element.IsActive = false;
            element.OnDisabled();

            return true;
        }

        public T GetElement<T>() where T : HintElement
            => _activeElements.TryGetFirst<T>(out var element) ? element : throw new Exception($"Element of type {typeof(T).FullName} was not found.");

        public T GetElement<T>(string customId) where T : HintElement
            => _activeElements.TryGetFirst<T>(item => item.CompareId(customId), out var element) ? element : throw new Exception($"Element of ID {customId} was not found.");

        public bool TryGetElement<T>(out T element) where T : HintElement
            => _activeElements.TryGetFirst(out element);

        public bool TryGetElement<T>(string customId, out T element) where T : HintElement
            => _activeElements.TryGetFirst(item => item.CompareId(customId), out element);

        public void ClearElements()
        {
            foreach (var activeElement in _activeElements)
            {
                activeElement.IsActive = false;
                activeElement.OnDisabled();
            }

            _activeElements.Clear();
        }

        public override void OnTick()
        {
            base.OnTick();

            foreach (var element in _activeElements)
                element.UpdateElement();

            foreach (var player in ExPlayer._players)
            {
                player.Hints._globalText = "";

                foreach (var element in _activeElements)
                {
                    if (!element.IsActive)
                        continue;

                    var data = element.GetContent(player);
                    var buffer = "";

                    if (data is not null && data.Length > 0)
                    {
                        if (element.IsRawDisplay)
                        {
                            buffer += data;
                        }
                        else
                        {
                            if (element._prev is null || element._prev != data)
                            {
                                element._messages.Clear();
                                element._prev = data;

                                data = data.Replace("\r\n", "\n").Replace("\\n", "\n").Replace("<br>", "\n").TrimEnd();

                                HintUtils.TrimStartNewLines(ref data, out var count);

                                var offset = element.VerticalOffset;

                                if (offset == 0f)
                                    offset = -count;

                                HintUtils.GetMessages(data, element._messages, offset, element.AutoLineWrap);
                            }

                            foreach (var message in element._messages)
                            {
                                if (string.IsNullOrWhiteSpace(message.Content))
                                    continue;

                                buffer += $"<voffset={message.VerticalOffset}em>";

                                if (element.Alignment is HintAlign.FullLeft)
                                    buffer += $"<align=left><pos=-{player.Hints.LeftOffset}%>{message.Content}</pos></align>";
                                else if (element.Alignment is HintAlign.Left)
                                    buffer += $"<align=left>{message.Content}</align>";
                                else if (element.Alignment is HintAlign.Right)
                                    buffer += $"<align=right>{message.Content}</align>";
                                else
                                    buffer += message.Content;

                                buffer += "\n";
                            }
                        }

                        player.Hints._globalText += buffer;
                    }
                }

                player.Hints.InternalTick();
            }
        }
    }
}