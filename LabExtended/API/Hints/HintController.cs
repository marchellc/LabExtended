﻿using System.Diagnostics;

using LabExtended.API.Hints.Elements.Personal;
using LabExtended.API.Enums;

using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Extensions;

using LabExtended.Utilities;
using LabExtended.Utilities.Unity;

using NorthwoodLib.Pools;

using System.Text;

using LabExtended.API.Hints.Interfaces;

using Mirror;

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

        public const ushort MaxHintTextLength = 65534;

        private static volatile List<HintElement> elements;
        private static volatile StringBuilder builder;
        
        private static NetworkWriter writer;
        private static NetworkWriter emptyWriter;

        private static Stopwatch watch;

        private static ArraySegment<byte> emptyData;

        private static int idClock;
        private static float updateInterval;
        private static long tickNum;

        internal static volatile bool sendNextFrame;

        public static IReadOnlyList<HintElement> Elements => elements;

        public static int ElementCount => elements.Count;

        public static bool SendNextFrame
        {
            get => sendNextFrame;
            set => sendNextFrame = value;
        }

        static HintController()
        {
            elements = new List<HintElement>();
            watch = new Stopwatch();
            
            writer = new NetworkWriter();
            
            emptyWriter = new NetworkWriter();
            emptyWriter.WriteHintData(0f, string.Empty);

            emptyData = emptyWriter.ToArraySegment();

            idClock = 0;

            builder = StringBuilderPool.Shared.Rent();
        }

        public static void PauseHints(this ExPlayer? player) => player.Hints.IsPaused = true;
        public static void ResumeHints(this ExPlayer? player) => player.Hints.IsPaused = false;
        public static void ToggleHints(this ExPlayer player) => player.Hints.IsPaused = !player.Hints.IsPaused;
        
        public static void ForceSendHints() => SendNextFrame = true;
        public static void ForceSendHints(this ExPlayer player) => SendNextFrame = true;

        /// <summary>
        /// Sends a new hint.
        /// </summary>
        /// <param name="content">Text of the hint.</param>
        /// <param name="duration">Duration of the hint (in seconds).</param>
        /// <param name="isPriority">Whether to show the hint immediately.</param>
        public static void ShowHint(this ExPlayer player, string content, ushort duration, bool isPriority = false)
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException(nameof(content));
            
            if (player.Hints.Queue.Count < 1)
            {
                player.Hints.Queue.Add(new HintMessage(content, duration));
            }
            else
            {
                if (isPriority)
                {
                    var curHint = player.Hints.Queue[0];

                    player.Hints.RemoveCurrent();
                    
                    player.Hints.Queue.Insert(0, new HintMessage(content, duration));
                    player.Hints.Queue.Add(curHint);
                }
                else
                {
                    player.Hints.Queue.Add(new HintMessage(content, duration));
                }
            }
        }

        public static void ClearHintElements(bool clearPersonal = false)
        {
            elements.ForEach(x =>
            {
                x.IsActive = false;

                x.Id = 0;
                
                x._tickNum = 0;
                x._prevCompiled = null;

                x.OnDisabled();
            });

            elements.Clear();

            if (clearPersonal)
            {
                foreach (var player in ExPlayer.Players)
                {
                    player.HintElements.ForEach(x =>
                    {
                        x.IsActive = false;

                        x.Id = 0;
                
                        x._tickNum = 0;
                        x._prevCompiled = null;

                        x.OnDisabled();
                    });
                    
                    player.HintElements.Clear();
                }
            }
        }

        public static bool RemoveHintElement<T>(this ExPlayer target) where T : PersonalHintElement
        {
            if (!target)
                throw new ArgumentNullException(nameof(target));

            if (!TryGetHintElement<T>(target, out var element))
                return false;
            
            return RemoveHintElement(target, element);
        }
        
        public static bool RemoveHintElement<T>() where T : HintElement
        {
            if (!TryGetHintElement<T>(out var element))
                return false;

            return RemoveHintElement(element);
        }
        
        public static bool RemoveHintElement(this ExPlayer target, string customId)
        {
            if (!TryGetHintElement(target, customId, out var element))
                return false;

            return RemoveHintElement(element);
        }

        public static bool RemoveHintElement(string customId)
        {
            if (!TryGetHintElement(customId, out var element))
                return false;

            return RemoveHintElement(element);
        }
        
        public static bool RemoveHintElement(this ExPlayer target, int elementId)
        {
            if (!TryGetHintElement(target, elementId, out var element))
                return false;

            return RemoveHintElement(element);
        }

        public static bool RemoveHintElement(int elementId)
        {
            if (!TryGetHintElement(elementId, out var element))
                return false;

            return RemoveHintElement(element);
        }
        
        public static bool RemoveHintElement(this ExPlayer target, PersonalHintElement hintElement)
        {
            if (hintElement is null)
                throw new ArgumentNullException(nameof(hintElement));
            
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (!target.HintElements.Remove(hintElement))
                return false;
            
            hintElement.IsActive = false;

            hintElement.Id = 0;

            hintElement._prevCompiled = null;
            hintElement._tickNum = 0;

            hintElement.OnDisabled();
            return true;
        }

        public static bool RemoveHintElement(this HintElement element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            element.IsActive = false;

            if (!elements.Remove(element))
                return false;

            element.Id = 0;

            element._prevCompiled = null;
            element._tickNum = 0;

            element.OnDisabled();
            return true;
        }
        
        public static bool AddHintElement<T>(this ExPlayer target) where T : PersonalHintElement
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            var instance = Activator.CreateInstance<T>();

            if (instance is null)
                throw new Exception($"Failed to construct type {typeof(T).FullName}");

            return AddHintElement(target, instance);
        }

        public static bool AddHintElement<T>() where T : HintElement
        {
            var instance = Activator.CreateInstance<T>();

            if (instance is null)
                throw new Exception($"Failed to construct type {typeof(T).FullName}");

            return AddHintElement(instance);
        }
        
        public static bool AddHintElement(this ExPlayer target, PersonalHintElement hintElement)
        {
            if (hintElement is null)
                throw new ArgumentNullException(nameof(hintElement));
            
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            hintElement.Id = idClock++;
            hintElement.Player = target;

            hintElement._prevCompiled = null;
            hintElement._tickNum = 0;

            target.HintElements.Add(hintElement);
            
            hintElement.OnEnabled();
            hintElement.IsActive = true;

            ApiLog.Debug("Hint API", $"Added personal element &1{hintElement.Id}&r (&6{hintElement.GetType().Name}&r) to player &1{target.Nickname}&r (&6{target.UserId}&r)");
            return true;
        }

        public static bool AddHintElement(this HintElement element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            if (element is PersonalHintElement)
                throw new Exception("This element requires a target player.");

            element.Id = idClock++;

            element._prevCompiled = null;
            element._tickNum = 0;

            elements.Add(element);
            
            element.OnEnabled();
            element.IsActive = true;

            ApiLog.Debug("Hint API", $"Added element &1{element.Id}&r (&6{element.GetType().Name}&r)");
            return true;
        }

        public static IEnumerable<HintElement> GetHintElements(Predicate<HintElement> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return elements.Where(x => predicate(x));
        }
        
        public static IEnumerable<PersonalHintElement> GetHintElements(this ExPlayer target, Predicate<PersonalHintElement> predicate)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return target.HintElements.Where(x => predicate(x));
        }

        public static IEnumerable<T> GetHintElements<T>(Predicate<T> predicate = null) where T : HintElement
        {
            if (predicate is null)
                return elements.Where<T>();
            else
                return elements.Where<T>(x => predicate(x));
        }
        
        public static IEnumerable<T> GetHintElements<T>(this ExPlayer target, Predicate<T> predicate = null) where T : PersonalHintElement
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            
            if (predicate is null)
                return target.HintElements.Where<T>();
            else
                return target.HintElements.Where<T>(x => predicate(x));
        }

        public static T GetHintElement<T>(int id) where T : HintElement
            => GetHintElement<T>(x => x.Id == id);
        
        public static T GetHintElement<T>(this ExPlayer target, int id) where T : PersonalHintElement
            => GetHintElement<T>(target, x => x.Id == id);
        
        public static T GetHintElement<T>(string customId) where T : PersonalHintElement
            => GetHintElement<T>(x => x.CompareId(customId));

        public static T GetHintElement<T>(this ExPlayer target, string customId) where T : PersonalHintElement
            => GetHintElement<T>(target, x => x.CompareId(customId));

        public static HintElement GetHintElement(Predicate<HintElement> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return elements.TryGetFirst(x => predicate(x), out var element) ? element : null;
        }
        
        public static PersonalHintElement GetHintElement(this ExPlayer target, Predicate<PersonalHintElement> predicate)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return target.HintElements.TryGetFirst(x => predicate(x), out var element) ? element : null;
        }

        public static T GetHintElement<T>(Predicate<T> predicate = null) where T : HintElement
        {
            if (predicate != null)
                return elements.TryGetFirst<T>(x => predicate(x), out var element) ? element : null;
            else
                return elements.TryGetFirst<T>(out var element) ? element : null;
        }
        
        public static T GetHintElement<T>(this ExPlayer target, Predicate<T> predicate = null) where T : PersonalHintElement
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            
            if (predicate != null)
                return target.HintElements.TryGetFirst<T>(x => predicate(x), out var element) ? element : null;
            else
                return target.HintElements.TryGetFirst<T>(out var element) ? element : null;
        }

        public static bool TryGetHintElement<T>(out T element) where T : HintElement
            => elements.TryGetFirst(out element);
        
        public static bool TryGetHintElement<T>(this ExPlayer target, out T element) where T : PersonalHintElement
            => target.HintElements.TryGetFirst(out element);

        public static bool TryGetHintElement<T>(int id, out T element) where T : HintElement
            => TryGetHintElement(x => x.Id == id, out element);
        
        public static bool TryGetHintElement<T>(this ExPlayer target, int id, out T element) where T : PersonalHintElement
            => TryGetHintElement(target, x => x.Id == id, out element);

        public static bool TryGetHintElement(int id, out HintElement element)
            => TryGetHintElement(x => x.Id == id, out element);
        
        public static bool TryGetHintElement(this ExPlayer target, int id, out PersonalHintElement hintElement)
            => TryGetHintElement(target, x => x.Id == id, out hintElement);

        public static bool TryGetHintElement<T>(string customId, out T element) where T : HintElement
            => TryGetHintElement(x => x.CompareId(customId), out element);
        
        public static bool TryGetHintElement<T>(this ExPlayer target, string customId, out T element) where T : PersonalHintElement
            => TryGetHintElement(target, x => x.CompareId(customId), out element);

        public static bool TryGetHintElement(string customId, out HintElement element)
            => TryGetHintElement(x => x.CompareId(customId), out element);
        
        public static bool TryGetHintElement(this ExPlayer target, string customId, out PersonalHintElement hintElement)
            => TryGetHintElement(target, x => x.CompareId(customId), out hintElement);

        public static bool TryGetHintElement<T>(Predicate<T> predicate, out T element) where T : HintElement
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return elements.TryGetFirst(x => predicate(x), out element);
        }
        
        public static bool TryGetHintElement<T>(this ExPlayer target, Predicate<T> predicate, out T element) where T : PersonalHintElement
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            return target.HintElements.TryGetFirst(x => predicate(x), out element);
        }

        public static bool TryGetHintElement(Predicate<HintElement> predicate, out HintElement element)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return elements.TryGetFirst(x => predicate(x), out element);
        }
        
        public static bool TryGetHintElement(this ExPlayer target, Predicate<PersonalHintElement> predicate, out PersonalHintElement hintElement)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            return target.HintElements.TryGetFirst(x => predicate(x), out hintElement);
        }

        internal static void OnTick()
        {
            try
            {
                if (ExPlayer.Count < 1) return;
                if (!sendNextFrame && updateInterval > 0 && watch.ElapsedMilliseconds < updateInterval) return;

                float lowestRate = ApiLoader.ApiConfig.HintSection.UpdateInterval;
                
                watch.Restart();

                if (tickNum + 1 >= long.MaxValue) tickNum = 0;

                tickNum++;
                
                foreach (var player in ExPlayer.Players)
                {
                    if (!player || player.Hints is null) continue;
                    if (player.Hints.IsPaused) continue;

                    builder.Clear();
                    builder.Append("~\n<line-height=1285%>\n<line-height=0>\n");

                    var anyAppended = false;
                    var anyOverride = false;
                    
                    var overrideParse = false;

                    player.Hints.RefreshRatio();
                    
                    if (player.Hints.CurrentMessage is null || player.Hints.UpdateTime())
                        player.Hints.NextMessage();

                    if (player.Hints.CurrentMessage != null)
                    {
                        player.Hints.ParseTemp();

                        if (player.Hints.TempData.Count > 0)
                        {
                            AppendMessages(player.Hints.TempData, TemporaryHintAlign, 0f, builder);
                            anyAppended = true;
                        }
                    }

                    void ProcessElement(HintElement element)
                    {
                        if (!element.IsActive) return;
                        
                        element.Builder.Clear();

                        if (element._tickNum != tickNum)
                        {
                            element._tickNum = tickNum;
                            element.OnUpdate();
                        }

                        if (!element.OnDraw(player) || element.Builder.Length < 1) return;

                        if ((builder.Length + element.Builder.Length) >= MaxHintTextLength)
                        {
                            ApiLog.Warn("Hint API", $"Could not append text from element &1{element.GetType().Name}&r for player &3{player.Nickname}&r (&6{player.UserId}&r) " +
                                                    $"due to it exceeding maximum allowed limit (&1{builder.Length + element.Builder.Length}&r / &2{MaxHintTextLength}&r)");

                            return;
                        }
                        
                        var content = element.Builder.ToString();

                        if (element.OverridesOthers)
                        {
                            anyOverride = true;
                            
                            builder.Clear();

                            if (element.ShouldParse)
                                builder.Append("~\n<line-height=1285%>\n<line-height=0>\n");
                            else
                                overrideParse = true;
                        }

                        if (!element.ShouldParse)
                        {
                            builder.Append(content);
                            anyAppended = true;
                        }
                        else
                        {
                            if (!element.ShouldCache || element._prevCompiled is null || element._prevCompiled != content)
                            {
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

                            if (element.Data.Count > 0)
                            {
                                AppendMessages(element.Data, element.GetAlignment(player), player.Hints.LeftOffset, builder);
                                anyAppended = true;
                            }
                        }

                        if (element is IHintRateModifier hintRateModifier)
                        {
                            var requestedInterval = hintRateModifier.ModifyRate(lowestRate);

                            if (requestedInterval >= 0f && requestedInterval < lowestRate)
                                lowestRate = requestedInterval;
                        }
                    }

                    foreach (var element in elements)
                    {
                        if (anyOverride) break;
                        ProcessElement(element);
                    }

                    foreach (var personalElement in player.HintElements)
                    {
                        if (anyOverride) break;
                        ProcessElement(personalElement);
                    }
                    
                    if (!anyAppended)
                    {
                        if (!player.Hints.WasClearedAfterEmpty)
                        {
                            player.Connection.Send(emptyData);
                            player.Hints.WasClearedAfterEmpty = true;
                        }

                        continue;
                    }
                    
                    if (!overrideParse)
                        builder.Append("<voffset=0><line-height=2100%>\n~");

                    var text = builder.ToString();

                    if (text.Length >= MaxHintTextLength)
                    {
                        if (!player.Hints.WasClearedAfterEmpty)
                        {
                            player.Connection.Send(emptyData);
                            player.Hints.WasClearedAfterEmpty = true;
                        }

                        ApiLog.Warn("Hint API", $"The compiled hint is too big! (&1{text.Length}&r / &2{MaxHintTextLength}&r)");
                        continue;
                    }
                    
                    writer.Reset();
                    writer.WriteHintData(ApiLoader.ApiConfig.HintSection.HintDuration, text);

                    player.Connection.Send(writer.ToArraySegment());
                    player.Hints.WasClearedAfterEmpty = false;
                }

                updateInterval = lowestRate;
            }
            catch (Exception ex)
            {
                ApiLog.Error("Hint API", ex);
            }
            
            sendNextFrame = false;
        }

        private static void AppendMessages(IEnumerable<HintData> hints, HintAlign align, float leftOffset, StringBuilder builder)
        {
            foreach (var message in hints)
            {
                if (message.Content is null || message.Content.Length < 1)
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
        }
    }
}