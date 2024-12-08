using LabExtended.Core.Ticking.Interfaces;

namespace LabExtended.Core.Ticking
{
    public struct TickHandle
    {
        private int _id;
        private ITickDistributor _distributor;

        internal TickHandle(int id, ITickDistributor distributor)
        {
            _id = id;
            _distributor = distributor;
        }

        public ITickDistributor Distributor => _distributor;

        public int Id => _id;

        public bool IsActive => Distributor != null && Distributor.IsActive(this);
        public bool IsValid => Distributor != null && Id > 0;
        public bool IsDestroyed => Distributor is null;
        
        public bool IsPaused
        {
            get => Distributor != null && Distributor.IsPaused(this);
            set
            {
                if (Distributor is null)
                    return;

                if (value)
                {
                    Distributor.Pause(this);
                    return;
                }

                Distributor.Resume(this);
                return;
            }
        }

        public void Destroy()
        {
            if (Distributor is null)
                return;

            Distributor.RemoveHandle(this);
        }

        internal void InternalDestroy()
        {
            _id = 0;
            _distributor = null;
        }

        public override string ToString()
            => $"Handle Id={Id} Distributor={_distributor?.ToString() ?? "null"}";
    }
}