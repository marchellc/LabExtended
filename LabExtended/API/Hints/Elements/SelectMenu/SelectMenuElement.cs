using Common.Extensions;
using Common.Pooling.Pools;

using LabExtended.API.Hints.Elements.SelectMenu.Listeners.Keybind;

using LabExtended.API.Input;
using LabExtended.API.Input.Enums;

using LabExtended.Core.Hooking;
using LabExtended.Events.SelectMenu;

using System.Text;

using UnityEngine;

namespace LabExtended.API.Hints.Elements.SelectMenu
{
    public class SelectMenuElement : HintElement
    {
        private static bool _keybindsRegistered;

        private readonly List<SelectMenuOption> _options;
        private readonly List<SelectMenuOption> _selected;

        private StringBuilder _builder;

        public SelectMenuElement(HintAlign align, float verticalOffset = 0f)
        {
            Alignment = align;
            VerticalOffset = verticalOffset;

            _options = new List<SelectMenuOption>();
            _selected = new List<SelectMenuOption>();

            if (!_keybindsRegistered)
            {
                InputHandler.RegisterListener<MenuUpKeybindListener>(InputType.Keybind, KeyCode.UpArrow);
                InputHandler.RegisterListener<MenuDownKeybindListener>(InputType.Keybind, KeyCode.DownArrow);
                InputHandler.RegisterListener<MenuSelectKeybindListener>(InputType.Keybind, KeyCode.KeypadEnter);

                _keybindsRegistered = true;
            }
        }

        public int MaxSelections { get; set; } = -1;

        public SelectMenuFlags Flags { get; set; } = SelectMenuFlags.None;
        public SelectMenuOption CurrentOption { get; set; }

        public IReadOnlyList<SelectMenuOption> SelectedOptions => _selected;
        public IReadOnlyList<SelectMenuOption> AddedOptions => _options;

        public bool CanSelect => MaxSelections < 1 || _selected.Count < MaxSelections;

        public override void OnEnabled()
        {
            base.OnEnabled();
            _builder = StringBuilderPool.Shared.Rent();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            StringBuilderPool.Shared.Return(_builder);
            _builder = null;
        }

        public SelectMenuElement CreateOption(SelectMenuOption selectMenuOption, byte? customPosition = null)
        {
            if (selectMenuOption is null)
                throw new ArgumentNullException(nameof(selectMenuOption));

            if (_options.Any(opt => opt.Id == selectMenuOption.Id || opt.Label == selectMenuOption.Label))
                throw new InvalidOperationException($"There's already an option with the same ID or label ({selectMenuOption.Id} / {selectMenuOption.Label}).");

            var position = (byte)0;

            if (customPosition.HasValue)
                position = customPosition.Value;
            else
                position = GetNextPosition();

            if (TryGetOption(position.ToString(), out var curOption))
            {
                curOption.OnDisabled();
                HookRunner.RunEvent(new SelectMenuRemovedOptionArgs(this, curOption));
                _options.Remove(curOption);
            }

            _options.Add(selectMenuOption);

            selectMenuOption.Position = position;
            selectMenuOption.OnEnabled();

            CurrentOption ??= selectMenuOption;

            ReorderOptions();
            return this;
        }

        public SelectMenuElement RemoveOption(string idOrLabel)
        {
            if (!TryGetOption(idOrLabel, out var option))
                return this;

            return RemoveOption(option);
        }

        public SelectMenuElement RemoveOption(SelectMenuOption selectMenuOption)
        {
            if (selectMenuOption is null)
                throw new ArgumentNullException(nameof(selectMenuOption));

            if (!_options.Contains(selectMenuOption))
                return this;

            selectMenuOption.OnDisabled();

            if (CurrentOption != null && CurrentOption == selectMenuOption)
                CurrentOption = _options.FirstOrDefault();

            HookRunner.RunEvent(new SelectMenuRemovedOptionArgs(this, selectMenuOption));

            _options.Remove(selectMenuOption);

            ReorderOptions();
            return this;
        }

        public SelectMenuElement SelectOptions(params string[] options)
        {
            if (_selected.Count < 1 || (Flags & SelectMenuFlags.CanChangeSelection) == SelectMenuFlags.CanChangeSelection)
            {
                foreach (var option in options)
                {
                    if (!TryGetOption(option, out var selectMenuOption)
                        && !(byte.TryParse(option, out var optionPos)
                        && !_options.TryGetFirst(opt => opt.Position == optionPos, out selectMenuOption))
                        || selectMenuOption is null)
                        continue;

                    if (_selected.Contains(selectMenuOption))
                        continue;

                    if (MaxSelections > 0 && _selected.Count >= MaxSelections)
                        break;

                    _selected.Add(selectMenuOption);
                    HookRunner.RunEvent(new SelectMenuSelectedOptionArgs(this, selectMenuOption));
                }
            }

            return this;
        }

        public SelectMenuElement UnselectOptions(params string[] options)
        {
            if (_selected.Count < 1 || (Flags & SelectMenuFlags.CanChangeSelection) == SelectMenuFlags.CanChangeSelection)
            {
                foreach (var option in options)
                {
                    if (!TryGetOption(option, out var selectMenuOption)
                        && !(byte.TryParse(option, out var optionPos)
                        && !_options.TryGetFirst(opt => opt.Position == optionPos, out selectMenuOption))
                        || selectMenuOption is null)
                        continue;

                    if (!_selected.Contains(selectMenuOption))
                        continue;

                    if (_selected.Remove(selectMenuOption))
                        HookRunner.RunEvent(new SelectMenuUnselectedOptionArgs(this, selectMenuOption));
                }
            }

            return this;
        }

        public SelectMenuElement ToggleOptions(params string[] options)
        {
            if (_selected.Count < 1 || (Flags & SelectMenuFlags.CanChangeSelection) == SelectMenuFlags.CanChangeSelection)
            {
                foreach (var option in options)
                {
                    if (!TryGetOption(option, out var selectMenuOption)
                        && !(byte.TryParse(option, out var optionPos)
                        && !_options.TryGetFirst(opt => opt.Position == optionPos, out selectMenuOption))
                        || selectMenuOption is null)
                        continue;

                    if (_selected.Contains(selectMenuOption))
                    {
                        _selected.Remove(selectMenuOption);
                        HookRunner.RunEvent(new SelectMenuUnselectedOptionArgs(this, selectMenuOption));
                    }
                    else
                    {
                        if (MaxSelections > 0 && _selected.Count >= MaxSelections)
                            continue;

                        _selected.Add(selectMenuOption);
                        HookRunner.RunEvent(new SelectMenuSelectedOptionArgs(this, selectMenuOption));
                    }
                }
            }

            return this;
        }

        public SelectMenuOption GetOption(string idOrLabel)
            => TryGetOption(idOrLabel, out var selectMenuOption) ? selectMenuOption : throw new Exception($"Unknown option: {idOrLabel}");

        public bool TryGetOption(string idOrLabel, out SelectMenuOption selectMenuOption)
            => _options.TryGetFirst(opt => opt != null && opt.Id == idOrLabel || opt.Label == idOrLabel || opt.Position.ToString() == idOrLabel, out selectMenuOption);

        public bool IsSelected(SelectMenuOption selectMenuOption)
            => selectMenuOption != null && _selected.Contains(selectMenuOption);

        public bool IsSelected(string idOrLabel)
            => !string.IsNullOrWhiteSpace(idOrLabel) && _selected.Any(opt => opt.Id == idOrLabel || opt.Label == idOrLabel || opt.Position.ToString() == idOrLabel);

        public bool IsSelected(byte position)
            => _selected.Any(opt => opt.Position == position);

        public void ClearOptions()
        {
            _selected.Clear();
            _options.Clear();
        }

        public void ReorderOptions()
        {
            var orderedOptions = _options.OrderBy(opt => opt.Position).ToList();

            _options.Clear();
            _options.AddRange(orderedOptions);

            CurrentOption ??= orderedOptions.FirstOrDefault();
        }

        public override void Write()
        {
            if (_options.Count < 1)
                return;

            _builder.Clear();

            for (int i = 0; i < _options.Count; i++)
            {
                var option = _options[i];

                option.TickOption();

                if (!option.IsEnabled)
                    continue;

                _builder.AppendLine($"<b>{(CanSelect && CurrentOption != null && CurrentOption == option ? "<color=#00ffec>→</color> " : "")}<color=#d4ff33>[{option.Position}]</color> | <color={(!IsSelected(option) ? "#ff0000" : "#4dff00")}>{option.Label}</color></b>");
            }

            if (_builder.Length < 1)
                return;

            Writer.Write(_builder.ToString());
        }

        private byte GetNextPosition()
            => (byte)_options.Count;
    }
}