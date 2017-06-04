using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire
{
    public static class Math
    {
        public static bool IsPointInSphere(Vector3f position, float radius, Vector3f point)
        {
            var x = point.X - position.X;
            var y = point.Y - position.Y;
            var z = point.Z - position.Z;
            var distance = x * x + y * y + z * z;
            return distance < radius * radius;
        }
    }
}
