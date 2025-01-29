using Hints;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Hints.Elements;
using LabExtended.API.Enums;

using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Events;
using LabExtended.Extensions;

using LabExtended.Utilities;
using LabExtended.Utilities.Unity;

using NorthwoodLib.Pools;

using System.Text;
using LabApi.Loader.Features.Paths;
using LabExtended.Commands.Contexts;
using UnityEngine;
using UnityEngine.PlayerLoop;

using HintMessage = LabExtended.API.Messages.HintMessage;

namespace LabExtended.API.Hints
{
    public static class HintController
    {
        public struct HintUpdateLoop { }
        
        #region Temporary Hint Settings
        public const HintAlign TemporaryHintAlign = HintAlign.Center;
        public const float TemporaryHintVerticalOffset = -5f;
        public const int TemporaryHintPixelSpacing = 3;
        public const bool TemporaryHintAutoWrap = true;
        #endregion

        public const ushort MaxHintTextLength = ushort.MaxValue;

        private static volatile List<HintElement> _elements;

        private static volatile TextHint _hintBuffer;
        private static volatile StringHintParameter _hintBufferParam;

        private static volatile TextHint _emptyHint;
        private static volatile StringHintParameter _emptyHintParam;

        private static volatile StringBuilder _builder;

        private static int _idClock;
        private static long _tickNum;
        private static float _curTime;

        internal static bool isDebugEnabled;
        internal static bool isManual;

        public static IReadOnlyList<HintElement> Elements => _elements;

        public static int ElementCount => _elements.Count;
        
        public static long TickNumber => _tickNum;
        public static float TickTime => _curTime;

        public static global::Hints.HintMessage CurMessage => new global::Hints.HintMessage(_hintBuffer);
        public static global::Hints.HintMessage EmptyMessage { get; }

        static HintController()
        {
            _elements = new List<HintElement>();

            _hintBufferParam = new StringHintParameter(string.Empty);
            _hintBuffer = new TextHint(string.Empty, new HintParameter[] { _hintBufferParam });

            _emptyHintParam = new StringHintParameter(string.Empty);
            _emptyHint = new TextHint(string.Empty, new HintParameter[] { _emptyHintParam });

            _idClock = 0;
            _tickNum = 0;

            _builder = StringBuilderPool.Shared.Rent();

            EmptyMessage = new global::Hints.HintMessage(_emptyHint);
        }

        public static void PauseHints(ExPlayer player)
            => player.hintCache.IsPaused = true;

        public static void ResumeHints(ExPlayer player)
            => player.hintCache.IsPaused = false;
        
        public static void Show(ExPlayer player, string content, ushort duration, bool isPriority = false)
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException(nameof(content));
            
            if (player.hintCache.Queue.Count < 1)
            {
                player.hintCache.Queue.Add(new HintMessage(content, duration));
            }
            else
            {
                if (isPriority)
                {
                    var curHint = player.hintCache.Queue[0];

                    player.hintCache.RemoveCurrent();
                    
                    player.hintCache.Queue.Insert(0, new HintMessage(content, duration));
                    player.hintCache.Queue.Add(curHint);
                }
                else
                {
                    player.hintCache.Queue.Add(new HintMessage(content, duration));
                }
            }
        }

        public static void ClearElements(bool clearPersonal = false)
        {
            _elements.ForEach(x =>
            {
                x.IsActive = false;

                x.Id = 0;
                
                x._tickNum = 0;
                x._prevCompiled = null;

                x.OnDisabled();
            });

            _elements.Clear();

            if (clearPersonal)
            {
                foreach (var player in ExPlayer.Players)
                {
                    player.elements.ForEach(x =>
                    {
                        x.IsActive = false;

                        x.Id = 0;
                
                        x._tickNum = 0;
                        x._prevCompiled = null;

                        x.OnDisabled();
                    });
                    
                    player.elements.Clear();
                }
            }
        }

        public static bool RemoveElement<T>(ExPlayer target) where T : PersonalElement
        {
            if (!target)
                throw new ArgumentNullException(nameof(target));

            if (!TryGet<T>(target, out var element))
                return false;
            
            return RemoveElement(element, target);
        }
        
        public static bool RemoveElement<T>() where T : HintElement
        {
            if (!TryGet<T>(out var element))
                return false;

            return RemoveElement(element);
        }
        
        public static bool RemoveElement(string customId, ExPlayer target)
        {
            if (!TryGet(customId, target, out var element))
                return false;

            return RemoveElement(element);
        }

        public static bool RemoveElement(string customId)
        {
            if (!TryGet(customId, out var element))
                return false;

            return RemoveElement(element);
        }
        
        public static bool RemoveElement(int elementId, ExPlayer target)
        {
            if (!TryGet(elementId, target, out var element))
                return false;

            return RemoveElement(element);
        }

        public static bool RemoveElement(int elementId)
        {
            if (!TryGet(elementId, out var element))
                return false;

            return RemoveElement(element);
        }
        
        public static bool RemoveElement(PersonalElement element, ExPlayer target)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (!target.elements.Remove(element))
                return false;
            
            element.IsActive = false;

            element.Id = 0;

            element._prevCompiled = null;
            element._tickNum = 0;

            element.OnDisabled();
            return true;
        }

        public static bool RemoveElement(HintElement element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            element.IsActive = false;

            if (!_elements.Remove(element))
                return false;

            element.Id = 0;

            element._prevCompiled = null;
            element._tickNum = 0;

            element.OnDisabled();
            return true;
        }
        
        public static bool AddElement<T>(ExPlayer target) where T : PersonalElement
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            
            var instance = typeof(T).Construct<T>();

            if (instance is null)
                throw new Exception($"Failed to construct type {typeof(T).FullName}");

            return AddElement(instance, target);
        }

        public static bool AddElement<T>() where T : HintElement
        {
            var instance = typeof(T).Construct<T>();

            if (instance is null)
                throw new Exception($"Failed to construct type {typeof(T).FullName}");

            return AddElement(instance);
        }
        
        public static bool AddElement(PersonalElement element, ExPlayer target)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            element.Id = _idClock++;
            element.Player = target;

            element._prevCompiled = null;
            element._tickNum = 0;

            target.elements.Add(element);
            
            element.OnEnabled();
            element.IsActive = true;

            ApiLog.Debug("Hint API", $"Added element &1{element.Id}&r (&6{element.GetType().Name}&r) to player &1{target.Name}&r (&6{target.UserId}&r)");
            return true;
        }

        public static bool AddElement(HintElement element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            element.Id = _idClock++;

            element._prevCompiled = null;
            element._tickNum = 0;

            _elements.Add(element);
            
            element.OnEnabled();
            element.IsActive = true;

            ApiLog.Debug("Hint API", $"Added element &1{element.Id}&r (&6{element.GetType().Name}&r)");
            return true;
        }

        public static IEnumerable<HintElement> GetElements(Predicate<HintElement> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return _elements.Where(x => predicate(x));
        }
        
        public static IEnumerable<PersonalElement> GetElements(ExPlayer target, Predicate<PersonalElement> predicate)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return target.elements.Where(x => predicate(x));
        }

        public static IEnumerable<T> GetElements<T>(Predicate<T> predicate = null) where T : HintElement
        {
            if (predicate is null)
                return _elements.Where<T>();
            else
                return _elements.Where<T>(x => predicate(x));
        }
        
        public static IEnumerable<T> GetElements<T>(ExPlayer target, Predicate<T> predicate = null) where T : PersonalElement
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            
            if (predicate is null)
                return target.elements.Where<T>();
            else
                return target.elements.Where<T>(x => predicate(x));
        }

        public static T GetElement<T>(int id) where T : HintElement
            => GetElement<T>(x => x.Id == id);
        
        public static T GetElement<T>(int id, ExPlayer target) where T : PersonalElement
            => GetElement<T>(target, x => x.Id == id);
        
        public static T GetElement<T>(string customId) where T : PersonalElement
            => GetElement<T>(x => x.CompareId(customId));

        public static T GetElement<T>(string customId, ExPlayer target) where T : PersonalElement
            => GetElement<T>(target, x => x.CompareId(customId));

        public static HintElement GetElement(Predicate<HintElement> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return _elements.TryGetFirst(x => predicate(x), out var element) ? element : null;
        }
        
        public static PersonalElement GetElement(ExPlayer target, Predicate<PersonalElement> predicate)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return target.elements.TryGetFirst(x => predicate(x), out var element) ? element : null;
        }

        public static T GetElement<T>(Predicate<T> predicate = null) where T : HintElement
        {
            if (predicate != null)
                return _elements.TryGetFirst<T>(x => predicate(x), out var element) ? element : null;
            else
                return _elements.TryGetFirst<T>(out var element) ? element : null;
        }
        
        public static T GetElement<T>(ExPlayer target, Predicate<T> predicate = null) where T : PersonalElement
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            
            if (predicate != null)
                return target.elements.TryGetFirst<T>(x => predicate(x), out var element) ? element : null;
            else
                return target.elements.TryGetFirst<T>(out var element) ? element : null;
        }

        public static bool TryGet<T>(out T element) where T : HintElement
            => _elements.TryGetFirst(out element);
        
        
        public static bool TryGet<T>(ExPlayer target, out T element) where T : PersonalElement
            => target.elements.TryGetFirst(out element);

        public static bool TryGet<T>(int id, out T element) where T : HintElement
            => TryGet(x => x.Id == id, out element);
        
        public static bool TryGet<T>(int id, ExPlayer target, out T element) where T : PersonalElement
            => TryGet(target, x => x.Id == id, out element);

        public static bool TryGet(int id, out HintElement element)
            => TryGet(x => x.Id == id, out element);
        
        public static bool TryGet(int id, ExPlayer target, out PersonalElement element)
            => TryGet(target, x => x.Id == id, out element);

        public static bool TryGet<T>(string customId, out T element) where T : HintElement
            => TryGet(x => x.CompareId(customId), out element);
        
        public static bool TryGet<T>(string customId, ExPlayer target, out T element) where T : PersonalElement
            => TryGet(target, x => x.CompareId(customId), out element);

        public static bool TryGet(string customId, out HintElement element)
            => TryGet(x => x.CompareId(customId), out element);
        
        public static bool TryGet(string customId, ExPlayer target, out PersonalElement element)
            => TryGet(target, x => x.CompareId(customId), out element);

        public static bool TryGet<T>(Predicate<T> predicate, out T element) where T : HintElement
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return _elements.TryGetFirst(x => predicate(x), out element);
        }
        
        public static bool TryGet<T>(ExPlayer target, Predicate<T> predicate, out T element) where T : PersonalElement
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            return target.elements.TryGetFirst(x => predicate(x), out element);
        }

        public static bool TryGet(Predicate<HintElement> predicate, out HintElement element)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return _elements.TryGetFirst(x => predicate(x), out element);
        }
        
        public static bool TryGet(ExPlayer target, Predicate<PersonalElement> predicate, out PersonalElement element)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            return target.elements.TryGetFirst(x => predicate(x), out element);
        }

        internal static void OnTick()
        {
            try
            {
                if (!isManual && ApiLoader.ApiConfig.HintSection.UpdateInterval > 0f)
                {
                    _curTime -= Time.deltaTime;

                    if (_curTime > 0f)
                        return;

                    _curTime = ApiLoader.ApiConfig.HintSection.UpdateInterval;
                }

                if ((_tickNum + 1) >= long.MaxValue)
                    _tickNum = 0;
                else
                    _tickNum++;

                if (!isManual && ExPlayer.Count < 1)
                {
                    if (isDebugEnabled)
                        ApiLog.Debug("Hint API", $"[Tick {_tickNum}] No players online");
                    
                    return;
                }

                if (isDebugEnabled)
                    ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Processing");
                
                foreach (var player in ExPlayer.Players)
                {
                    if (!player || player.hintCache is null)
                    {
                        if (isDebugEnabled)
                            ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Encountered null player or cache");
                        
                        continue;
                    }

                    if (player.hintCache.IsPaused)
                    {
                        if (isDebugEnabled)
                            ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Hint cache is paused");
                        
                        continue;
                    }

                    _builder.Clear();
                    _builder.Append("~\n<line-height=1285%>\n<line-height=0>\n");

                    var anyAppended = false;
                    var anyOverride = false;
                    
                    var overrideParse = false;
                    
                    if (isDebugEnabled)
                        ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Processing main message");

                    player.hintCache.RefreshRatio();

                    if (player.hintCache.CurrentMessage is null || player.hintCache.UpdateTime())
                        player.hintCache.NextMessage();

                    if (player.hintCache.CurrentMessage != null)
                    {
                        player.hintCache.ParseTemp();

                        if (player.hintCache.TempData.Count > 0)
                        {
                            AppendMessages(player.hintCache.TempData, TemporaryHintAlign, 0f, _builder);
                            anyAppended = true;
                        }
                    }
                    
                    if (isDebugEnabled)
                        ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Processed ({anyAppended})");

                    void ProcessElement(HintElement element)
                    {
                        if (isDebugEnabled)
                            ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Processing element {element.GetType().Name}");

                        if (!element.IsActive)
                        {
                            if (isDebugEnabled)
                                ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Element disabled");

                            return;
                        }
                        
                        element.Builder.Clear();

                        if (element._tickNum != _tickNum)
                        {
                            element._tickNum = _tickNum;
                            element.OnUpdate();
                        }

                        if (isDebugEnabled)
                            ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Calling OnDraw");

                        if (!element.OnDraw(player) || element.Builder.Length < 1)
                        {
                            if (isDebugEnabled)
                                ApiLog.Debug("Hint API", $"[Tick {_tickNum}] OnDraw failed or builder empty ({element.Builder.Length})");

                            return;
                        }

                        if ((_builder.Length + element.Builder.Length) >= MaxHintTextLength)
                        {
                            ApiLog.Warn("Hint API", $"Could not append text from element &1{element.GetType().Name}&r for player &3{player.Name}&r (&6{player.UserId}&r) " +
                                                    $"due to it exceeding maximum allowed limit (&1{_builder.Length + element.Builder.Length}&r / &2{MaxHintTextLength}&r)");

                            return;
                        }
                        
                        var content = element.Builder.ToString();

                        if (isDebugEnabled)
                        {
                            ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Built content");
                            File.WriteAllText(Path.Combine(PathManager.LabApi.FullName, $"{player.UserId}_{element.GetType().Name}_{_tickNum}_hint_debug.txt"), content);
                        }

                        if (element.OverridesOthers)
                        {
                            if (isDebugEnabled)
                                ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Element override active");
                            
                            anyOverride = true;
                            
                            _builder.Clear();

                            if (element.ShouldParse)
                                _builder.Append("~\n<line-height=1285%>\n<line-height=0>\n");
                            else
                                overrideParse = true;
                        }

                        if (!element.ShouldParse)
                        {
                            if (isDebugEnabled)
                                ApiLog.Debug("Hint API", $"[Tick {_tickNum}] ShouldParse false");
                            
                            _builder.Append(content);
                            anyAppended = true;
                        }
                        else
                        {
                            if (!element.ShouldCache || element._prevCompiled is null || element._prevCompiled != content)
                            {
                                if (isDebugEnabled)
                                    ApiLog.Debug("Hint API", $"[Tick {_tickNum}] ShouldCache is false or cache is not up to date");
                                
                                element.Data.ForEach(x => ObjectPool<HintData>.Shared.Return(x));
                                element.Data.Clear();

                                element._prevCompiled = content;

                                content = content
                                    .Replace("\r\n", "\n")
                                    .Replace("\\n", "\n")
                                    .Replace("<br>", "\n")
                                    .TrimEnd();

                                HintUtils.TrimStartNewLines(ref content, out var count);

                                var offset = element.GetVerticalOffset(player);

                                if (offset == 0f)
                                    offset = -count;

                                HintUtils.GetMessages(content, element.Data, offset, element.ShouldWrap, element.GetPixelSpacing(player));
                            }
                            
                            if (isDebugEnabled)
                                ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Data: {element.Data.Count}");

                            if (element.Data.Count > 0)
                            {
                                AppendMessages(element.Data, element.GetAlignment(player), player.hintCache.LeftOffset, _builder);
                                anyAppended = true;
                            }
                        }
                    }

                    foreach (var element in _elements)
                    {
                        if (anyOverride)
                            break;
                        
                        ProcessElement(element);
                    }

                    foreach (var personalElement in player.elements)
                    {
                        if (anyOverride)
                            break;
                        
                        ProcessElement(personalElement);
                    }
                    
                    if (isDebugEnabled)
                        ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Finishing builder {_builder.Length}");

                    if (!anyAppended && !player.hintCache.WasClearedAfterEmpty)
                    {
                        if (isDebugEnabled)
                            ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Empty, sending empty");
                        
                        player.Send(EmptyMessage);
                        player.hintCache.WasClearedAfterEmpty = true;

                        continue;
                    }
                    
                    if (!overrideParse)
                        _builder.Append("<voffset=0><line-height=2100%>\n~");

                    _hintBufferParam.Value = _builder.ToString();
                    
                    _hintBuffer.Text = _hintBufferParam.Value;
                    _hintBuffer.DurationScalar = ApiLoader.ApiConfig.HintSection.HintDuration;
                    
                    if (isDebugEnabled)
                        File.WriteAllText(Path.Combine(PathManager.LabApi.FullName, $"{player.UserId}_hint_debug_{_tickNum}.txt"), _hintBuffer.Text);

                    if (_hintBuffer.Text.Length >= MaxHintTextLength)
                    {
                        if (!player.hintCache.WasClearedAfterEmpty)
                        {
                            player.Send(EmptyMessage);
                            player.hintCache.WasClearedAfterEmpty = true;
                        }

                        ApiLog.Warn("Hint API", $"The compiled hint is too big! (&1{_hintBuffer.Text.Length}&r / &2{MaxHintTextLength}&r)");
                        continue;
                    }

                    player.Send(CurMessage);
                    player.hintCache.WasClearedAfterEmpty = false;
                    
                    if (isDebugEnabled)
                        ApiLog.Debug("Hint API", $"[Tick {_tickNum}] Sent current");
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Hint Controller", ex);
            }
        }

        private static void AppendMessages(IEnumerable<HintData> hints, HintAlign align, float leftOffset, StringBuilder builder)
        {
            foreach (var message in hints)
            {
                if (string.IsNullOrWhiteSpace(message.Content))
                    continue;

                builder.Append("<voffset=");
                builder.Append(message.VerticalOffset);
                builder.Append("em>");

                if (align is HintAlign.FullLeft)
                {
                    builder.Append("<align=left><pos=-");
                    builder.Append(leftOffset);
                    builder.Append("%>");
                    builder.Append(message.Content);
                    builder.Append("</pos></align>");
                }
                else if (align is HintAlign.Left)
                {
                    builder.Append("<align=left>");
                    builder.Append(message.Content);
                    builder.Append("</align>");
                }
                else if (align is HintAlign.Right)
                {
                    builder.Append("<align=right>");
                    builder.Append(message.Content);
                    builder.Append("</align>");
                }
                else
                {
                    builder.Append(message.Content);
                }

                builder.Append("</voffset>");
                builder.AppendLine();
            }
        }

        [LoaderInitialize(1)]
        private static void Init()
        {
            PlayerLoopHelper.ModifySystem(x => x.InjectAfter<TimeUpdate.WaitForLastPresentationAndUpdateTime>(OnTick, typeof(HintUpdateLoop)) ? x : null);
            isDebugEnabled = ApiLoader.ApiConfig.HintSection.WriteDebug;
        }
    }
}