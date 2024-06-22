namespace LabExtended.Utilities
{
    public struct SimpleRateLimit
    {
        private int _permits = 0;
        private bool _waiting;
        private DateTime _lastPermit;

        public int Limit { get; }
        public int Count { get; }

        public int Permits => _permits;

        public bool IsWaiting => _waiting;

        public DateTime LastPermit => _lastPermit;

        public SimpleRateLimit(int limit, int count)
        {
            Limit = limit;
            Count = count;
        }

        public bool GetPermit()
        {
            if (_waiting)
            {
                if ((DateTime.Now - _lastPermit).TotalMilliseconds >= Limit)
                {
                    _lastPermit = DateTime.Now;
                    _permits = 1;
                    _waiting = false;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if ((DateTime.Now - _lastPermit).TotalMilliseconds < Limit && _permits >= Count)
                {
                    _waiting = true;
                    return false;
                }

                _permits++;
                _lastPermit = DateTime.Now;
                _waiting = false;

                return true;
            }
        }
    }
}