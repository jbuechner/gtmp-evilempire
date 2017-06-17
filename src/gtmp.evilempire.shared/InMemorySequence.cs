using System.Threading;

namespace gtmp.evilempire
{
    public class InMemorySequence
    {
        int _current;

        public int Current { get => _current; }

        public InMemorySequence()
            : this(int.MinValue)
        {
        }

        public InMemorySequence(int seed)
        {
            _current = seed;
        }

        public int Next()
        {
            return Interlocked.Increment(ref _current);
        }
    }
}
