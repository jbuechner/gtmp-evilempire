namespace gtmp.evilempire.entities
{
    public struct Vector3f
    {
        public static readonly Vector3f One = new Vector3f(1f, 1f, 1f);

        float _x;
        float _y;
        float _z;

        public float X { get { return _x; } set { _x = value; } }
        public float Y { get { return _y; } set { _y = value; } }
        public float Z { get { return _z; } set { _z = value; } }

        public Vector3f(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }
    }
}
