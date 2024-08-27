using LabExtended.API.Collections;

using LabExtended.Core.Ticking.Interfaces;
using LabExtended.Core.Ticking.Internals;
using LabExtended.Extensions;

namespace LabExtended.Core.Ticking.Distributors.Unity
{
    public class UnityTickDistributor : ITickDistributor
    {
        private UnityTickComponent _component;
        private AutoArray<InternalTickHandleWrapper<UnityTickOptions>> _handles;

        private readonly bool _sharedComponent;

        public UnityTickDistributor() 
            : this(UnityTickComponent.CreateNew(), false) { }

        public UnityTickDistributor(UnityTickComponent component, bool sharedComponent)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            TickDistribution.InternalAddDistributor(this);

            _component = component;
            _sharedComponent = sharedComponent;

            _component.OnUpdate += OnUpdate;
            _component.OnLateUpdate += OnLateUpdate;
            _component.OnFixedUpdate += OnFixedUpdate;

            _handles = new AutoArray<InternalTickHandleWrapper<UnityTickOptions>>(200, 30);

            Info("Enabled tick distributor.");
        }

        public event Action OnTick;

        public float TickRate => _component.TickRate;
        public int HandleCount => _handles.Size;

        public TickHandle CreateHandle(object handle)
        {
            if (handle is null)
                throw new ArgumentNullException(nameof(handle));

            if (handle is not InternalTickHandle internalTickHandle)
                throw new Exception($"Unsupported handle: {handle.GetType().FullName}");

            if (_handles.Any(x => x.Base.Id == internalTickHandle.Id))
                throw new Exception($"Duplicate handle ID: {internalTickHandle.Id}");

            Debug($"Creating handle: Id={internalTickHandle.Id} Invoker={internalTickHandle.Invoker} Size={_handles.Size} Free={_handles.Free}");

            var wrapper = new InternalTickHandleWrapper<UnityTickOptions>(internalTickHandle, (internalTickHandle.Options as UnityTickOptions) ?? UnityTickOptions.DefaultOptions);

            if (wrapper.Options.HasFlag(TickFlags.Separate))
            {
                var component = UnityTickComponent.CreateNew();

                wrapper.Options._separateComponent = component;

                if (wrapper.Options.Segment is UnityTickSegment.FixedUpdate)
                    component.OnFixedUpdate += () => ExecuteSegment(wrapper);
                else if (wrapper.Options.Segment is UnityTickSegment.LateUpdate)
                    component.OnLateUpdate += () => ExecuteSegment(wrapper);
                else
                    component.OnUpdate += () => ExecuteSegment(wrapper);
            }

            var index = _handles.Add(wrapper);

            Debug($"Added handle: index={index} Size={_handles.Size} Free={_handles.Free}");
            return new TickHandle(internalTickHandle.Id, this);
        }

        public void RemoveHandle(TickHandle handle)
        {
            Debug($"Destroying handle: Id={handle.Id} Size={_handles.Size} Free={_handles.Free}");

            if (_handles.TryGet(x => x.Base.Id == handle.Id, out var wrapperHandle))
            {
                Debug($"Handle found: Invoker={wrapperHandle.Base.Invoker}");

                if (wrapperHandle.Options.HasFlag(TickFlags.Separate))
                {
                    Debug($"Handle has separate component, destroying");

                    wrapperHandle.Options._separateComponent?.Destroy();
                    wrapperHandle.Options._separateComponent = null;
                }

                if (wrapperHandle.Base.Timer != null)
                {
                    Debug($"Handle has an active timer, disposing");

                    wrapperHandle.Base.Timer.Dispose();
                    wrapperHandle.Base.Timer = null;
                }

                wrapperHandle.Base.Paused = true;

                Debug($"Handle disposed");
            }
            else
            {
                Debug($"Failed to find handle");
            }

            var removed = _handles.RemoveMany(x => x.Base.Id == handle.Id);

            Debug($"Removed handles: {removed} Size={_handles.Size} Free={_handles.Free}");

            handle.InternalDestroy();

            Debug($"Handle destroyed");
        }

        public bool HasHandle(TickHandle handle)
            => _handles.Any(x => x.Base.Id == handle.Id);

        public bool IsActive(TickHandle handle)
            => _handles.Any(x => x.Base.Id == handle.Id && !x.Base.Paused);

        public bool IsPaused(TickHandle handle)
            => _handles.Any(x => x.Base.Id == handle.Id && x.Base.Paused);

        public void Pause(TickHandle handle)
        {
            Debug($"Pausing handle: {handle.Id}");
                
            _handles.ForEach(x =>
            {
                if (x.Base.Id != handle.Id)
                    return;

                if (x.Base.Paused)
                {
                    Debug($"Handle Id={x.Base.Id} Invoker={x.Base.Invoker} is already paused");
                    return;
                }

                Debug($"Paused handle Id={x.Base.Id} Invoker={x.Base.Invoker}");
                x.Base.Paused = true;
            });
        }

        public void Resume(TickHandle handle)
        {
            Debug($"Resuming handle: {handle.Id}");

            _handles.ForEach(x =>
            {
                if (x.Base.Id != handle.Id)
                    return;

                if (!x.Base.Paused)
                {
                    Debug($"Handle Id={x.Base.Id} Invoker={x.Base.Invoker} is not paused");
                    return;
                }

                Debug($"Resumed handle Id={x.Base.Id} Invoker={x.Base.Invoker}");
                x.Base.Paused = false;
            });
        }

        public void ClearEvent()
            => OnTick = null;

        public void ClearHandles()
            => _handles.Clear();

        public void Dispose()
        {
            Debug($"Disposing ..");

            OnTick = null;

            _handles.ForEach(x => TickDistribution.InternalDestroyHandle(x.Base));

            _handles.Clear();
            _handles = null;

            if (!_sharedComponent)
                _component.Destroy();
            
            _component = null;

            Debug("Disposed");
        }

        private void OnLateUpdate()
            => ExecuteSegment(UnityTickSegment.LateUpdate);

        private void OnFixedUpdate()
            => ExecuteSegment(UnityTickSegment.FixedUpdate);

        private void OnUpdate()
            => ExecuteSegment(UnityTickSegment.Update);

        private void ExecuteSegment(InternalTickHandleWrapper<UnityTickOptions> wrapper)
        {
            if (wrapper.Base.Paused)
                return;

            if (wrapper.Base.Timer != null && !wrapper.Base.Timer.CanContinue())
                return;

            if (wrapper.Options.HasUnityFlag(UnityTickFlags.SkipFrames) && wrapper.Options._skippedFrames < wrapper.Options.SkipFrames)
            {
                wrapper.Options._skippedFrames++;
                return;
            }

            wrapper.Base.Invoker?.Invoke();
            wrapper.Base.Timer?.OnExecuted();

            wrapper.Options._skippedFrames = 0;
        }

        private void ExecuteSegment(UnityTickSegment segment)
        {
            if (segment is UnityTickSegment.Update)
                OnTick.InvokeSafe();

            _handles.ForEach(x =>
            {
                if (x.Base.Paused)
                    return;

                if (x.Options.Segment != segment)
                    return;

                if (x.Options.HasFlag(TickFlags.Separate))
                    return;

                if (x.Base.Timer != null && !x.Base.Timer.CanContinue())
                    return;

                if (x.Options.HasUnityFlag(UnityTickFlags.SkipFrames) && x.Options._skippedFrames < x.Options.SkipFrames)
                {
                    x.Options._skippedFrames++;
                    return;
                }

                x.Base.Invoker?.Invoke();
                x.Base.Timer?.OnExecuted();

                x.Options._skippedFrames = 0;
            });
        }

        private void Info(object msg)
            => ApiLoader.Info("Unity Tick Distributor", msg);

        private void Warn(object msg)
            => ApiLoader.Warn("Unity Tick Distributor", msg);

        private void Error(object msg)
            => ApiLoader.Error("Unity Tick Distributor", msg);

        private void Debug(object msg)
            => ApiLoader.Debug("Unity Tick Distributor", msg);
    }
}