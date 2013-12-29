using OpenTK;

namespace RaytracerOpenTK
{
    internal class Ray
    {
        private readonly Vector3 direction;
        private readonly Vector3 origin;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public Ray()
        {
            origin = new Vector3(0, 0, 0);
            direction = new Vector3(0, 0, 0);
        }

        public Vector3 Origin
        {
            get { return origin; }
        }

        public Vector3 Direction
        {
            get { return direction; }
        }
    }
}