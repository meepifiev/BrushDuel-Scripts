namespace _Project.Core.Drawing
{
    public class DeterministicRandom
    {
        private const float Inverse24Bit = 1f / 16777216f;

        private uint _state;

        public DeterministicRandom(uint seed)
        {
            _state = seed == 0 ? 1u : seed;
        }

        public float NextFloat01()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (_state & 0xFFFFFF) * Inverse24Bit;
        }
    }
}
