﻿using Hints;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Enums;
using LabExtended.API.Pooling;
using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Core.Ticking;

using LabExtended.Extensions;
using LabExtended.Utilities;

using NorthwoodLib.Pools;

using System.Text;

using UnityEngine;

using HintMessage = LabExtended.API.Messages.HintMessage;

namespace LabExtended.API.Hints
{
    public static class HintController
    {
        #region Temporary Hint Settings
        public const HintAlign TemporaryHintAlign = HintAlign.Center;
        public const float TemporaryHintVerticalOffset = -5f;
        public const int TemporaryHintPixelSpacing = 3;
        public const bool TemporaryHintAutoWrap = true;
        #endregion

        private static volatile LockedHashSet<HintElement> _elements;
        private static volatile LockedDictionary<ExPlayer, HintCache> _cache;

        private static volatile TextHint _hintBuffer;
        private static volatile StringHintParameter _hintBufferParam;

        private static volatile TextHint _emptyHint;
        private static volatile StringHintParameter _emptyHintParam;

        private static volatile StringBuilder _builder;

        private static int _idClock;
        private static long _tickNum;
        private static float _curTime;

        public static IReadOnlyList<HintElement> Elements => _elements;

        public static int ElementCount => _elements.Count;
        public static long TickNumber => _tickNum;
        public static float TickTime => _curTime;

        public static global::Hints.HintMessage CurMessage => new global::Hints.HintMessage(_hintBuffer);
        public static global::Hints.HintMessage EmptyMessage { get; }

        static HintController()
        {
            _elements = new LockedHashSet<HintElement>(byte.MaxValue);
            _cache = new LockedDictionary<ExPlayer, HintCache>(byte.MaxValue);

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
            => GetOrAddCache(player).IsPaused = true;

        public static void ResumeHints(ExPlayer player)
            => GetOrAddCache(player).IsPaused = false;

        public static void Show(ExPlayer player, string content, ushort duration, bool isPriority = false)
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException(nameof(content));

            var cache = GetOrAddCache(player);

            if (cache.Queue.Count < 1)
            {
                cache.Queue.Add(new HintMessage(content, duration));
                return;
            }
            else
            {
                if (isPriority)
                {
                    var curHint = cache.Queue[0];

                    cache.RemoveCurrent();
                    cache.Queue.Insert(0, new HintMessage(content, duration));
                    cache.Queue.Add(curHint);
                }
                else
                {
                    cache.Queue.Add(new HintMessage(content, duration));
                }
            }
        }

        public static void ClearElements()
        {
            _elements.ForEach(x =>
            {
                x.IsActive = false;

                x.Whitelist.Dispose();
                x.Data.Clear();

                x.Id = 0;
                x._tickNum = 0;
                x._prevCompiled = null;

                x.OnDisabled();
            });

            _elements.Clear();
        }

        public static bool RemoveElement<T>() where T : HintElement
        {
            if (!TryGet<T>(out var element))
                return false;

            return RemoveElement(element);
        }

        public static bool RemoveElement(string customId)
        {
            if (!TryGet(customId, out var element))
                return false;

            return RemoveElement(element);
        }

        public static bool RemoveElement(int elementId)
        {
            if (!TryGet(elementId, out var element))
                return false;

            return RemoveElement(element);
        }

        public static bool RemoveElement(HintElement element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            element.IsActive = false;

            if (!_elements.Remove(element))
                return false;

            element.Whitelist.Dispose();
            element.Data.Clear();

            element.Id = 0;

            element._prevCompiled = null;
            element._tickNum = 0;

            element.OnDisabled();
            return true;
        }

        public static bool AddElement<T>() where T : HintElement
        {
            var instance = typeof(T).Construct<T>();

            if (instance is null)
                throw new Exception($"Failed to construct type {typeof(T).FullName}");

            return AddElement(instance);
        }

        public static bool AddElement(HintElement element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            element.Whitelist.Clear();
            element.Data.Clear();

            element.Id = _idClock++;

            element._prevCompiled = null;
            element._tickNum = 0;

            if (_elements.Add(element))
            {
                element.IsActive = true;
                element.OnEnabled();
                return true;
            }

            return false;
        }
        
        public static IEnumerable<HintElement> GetElements(Predicate<HintElement> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return _elements.Where(x => predicate(x));
        }

        public static IEnumerable<T> GetElements<T>(Predicate<T> predicate = null) where T : HintElement
        {
            if (predicate is null)
                return _elements.Where<T>();
            else
                return _elements.Where<T>(x => predicate(x));
        }

        public static T GetElement<T>(int id) where T : HintElement
            => GetElement<T>(x => x.Id == id);

        public static T GetElement<T>(string customId) where T : HintElement
            => GetElement<T>(x => x.CompareId(customId));

        public static HintElement GetElement(Predicate<HintElement> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return _elements.TryGetFirst(x => predicate(x), out var element) ? element : null;
        }

        public static T GetElement<T>(Predicate<T> predicate = null) where T : HintElement
        {
            if (predicate != null)
                return _elements.TryGetFirst<T>(x => predicate(x), out var element) ? element : null;
            else
                return _elements.TryGetFirst<T>(out var element) ? element : null;
        }

        public static bool TryGet<T>(out T element) where T : HintElement
            => _elements.TryGetFirst(out element);

        public static bool TryGet<T>(int id, out T element) where T : HintElement
            => TryGet(x => x.Id == id, out element);

        public static bool TryGet(int id, out HintElement element)
            => TryGet(x => x.Id == id, out element);

        public static bool TryGet<T>(string customId, out T element) where T : HintElement
            => TryGet(x => x.CompareId(customId), out element);

        public static bool TryGet(string customId, out HintElement element)
            => TryGet(x => x.CompareId(customId), out element);

        public static bool TryGet<T>(Predicate<T> predicate, out T element) where T : HintElement
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return _elements.TryGetFirst(x => predicate(x), out element);
        }

        public static bool TryGet(Predicate<HintElement> predicate, out HintElement element)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return _elements.TryGetFirst(x => predicate(x), out element);
        }

        private static void OnTick()
        {
            try
            {
                _curTime -= Time.deltaTime;

                if (_curTime > 0f)
                    return;

                _curTime = ApiLoader.ApiConfig.HintSection.UpdateInterval;

                if (_elements.Count < 1 || ExPlayer.Count < 1)
                    return;

                if ((_tickNum + 1) >= long.MaxValue)
                    _tickNum = 0;
                else
                    _tickNum++;

                foreach (var pair in _cache)
                {
                    var player = pair.Key;
                    var cache = pair.Value;

                    if (!player || cache.IsPaused)
                        continue;

                    _builder.Clear();
                    _builder.Append("~\n<line-height=1285%>\n<line-height=0>\n");

                    var anyAppended = false;

                    cache.RefreshRatio();

                    if (cache.CurrentMessage is null || cache.UpdateTime())
                        cache.NextMessage();

                    if (cache.CurrentMessage != null)
                    {
                        cache.ParseTemp();

                        if (cache.TempData.Count > 0)
                        {
                            AppendMessages(cache.TempData, TemporaryHintAlign, 0f, _builder);
                            anyAppended = true;
                        }
                    }

                    foreach (var element in _elements)
                    {
                        if (!element.IsActive)
                            continue;

                        if (element._tickNum != _tickNum)
                        {
                            element._tickNum = _tickNum;
                            element.TickElement();
                        }

                        if (!element.IsGlobal && !element.Whitelist.Contains(player))
                            continue;

                        var content = element.BuildContent(player);

                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            if (element.IsRaw)
                            {
                                _builder.Append(content);
                            }
                            else
                            {
                                if (element._prevCompiled is null || element._prevCompiled != content)
                                {
                                    element.Data.ForEach(x => ObjectPool<HintData>.Return(x));
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

                                if (element.Data.Count > 0)
                                {
                                    AppendMessages(element.Data, element.Align, cache.LeftOffset, _builder);
                                    anyAppended = true;
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (!anyAppended && !cache.WasClearedAfterEmpty)
                    {
                        player.Connection.Send(EmptyMessage);
                        cache.WasClearedAfterEmpty = true;

                        continue;
                    }

                    _builder.Append("<voffset=0><line-height=2100%>\n~");

                    _hintBufferParam.Value = _builder.ToString();
                    _hintBuffer.Text = _hintBufferParam.Value;
                    _hintBuffer.DurationScalar = ApiLoader.ApiConfig.HintSection.HintDuration;

                    player.Connection.Send(CurMessage);

                    cache.WasClearedAfterEmpty = false;
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Hint Controller", ex);
            }
        }

        private static HintCache GetOrAddCache(ExPlayer player)
        {
            if (_cache.TryGetValue(player, out var cache))
                return cache;

            ApiLog.Warn("Hint Controller", $"Player &3{player.Name} {player.UserId}&r did not have a HintCache.");
            return _cache[player] = new HintCache() { Player = player };
        }

        internal static void OnJoined(ExPlayer player)
        {
            _cache.Add(player, new HintCache() { Player = player });

            _elements.ForEach(x =>
            {
                if (!x.IsGlobal && !x.ValidateWhitelist(player))
                    return;

                x.Whitelist.Add(player);
                x.OnWhitelistAdded(player);
            });
        }

        internal static void OnLeft(ExPlayer player)
        {
            _elements.ForEach(x =>
            {
                if (!x.IsGlobal && !x.Whitelist.Contains(player))
                    return;

                x.Whitelist.Remove(player);
                x.OnWhitelistRemoved(player);
            });

            _cache.Remove(player);
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
        internal static void InternalLoad()
            => TickDistribution.UnityTick.CreateHandle(OnTick);
    }
}