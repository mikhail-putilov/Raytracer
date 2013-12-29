using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;

namespace RaytracerOpenTK
{
    internal enum IntersectResult
    {
        Miss,
        Hit,
        Inprim
    }

    internal class Scene
    {
        public Scene(IEnumerable<Primitive> primitives)
        {
            Primitives = primitives.ToList();
        }

        public List<Primitive> Primitives { get; set; }
    }

    internal abstract class Primitive
    {
        public bool IsLight { get; set; }
        public string Name { get; set; }

        public Color4 Color
        {
            get { return Material.Color; }
        }

        public Material Material { get; set; }
        public abstract Vector3 GetNormal(Vector3 position);
        public abstract IntersectResult Intersect(Ray ray, ref float dist);
    }

    internal class Sphere : Primitive
    {
        public Sphere(Vector3 centre, float radius)
        {
            Centre = centre;
            Radius = radius;
            IsLight = false;
        }

        public Vector3 Centre { get; private set; }
        public float Radius { get; private set; }

        public float SquareRadius
        {
            get { return Radius*Radius; }
        }

        public float ReverseRadius
        {
            get { return 1.0f/Radius; }
        }

        public override Vector3 GetNormal(Vector3 position)
        {
            return (position - Centre)*ReverseRadius;
        }

        public override IntersectResult Intersect(Ray ray, ref float dist)
        {
            Vector3 v = ray.Origin - Centre;
            float b = -Vector3.Dot(v, ray.Direction);
            float det = (b*b) - Vector3.Dot(v, v) + SquareRadius;
            var retval = IntersectResult.Miss;
            if (det > 0)
            {
                det = (float) Math.Sqrt(det);
                float i1 = b - det;
                float i2 = b + det;
                if (i2 > 0)
                {
                    if (i1 < 0)
                    {
                        if (i2 < dist)
                        {
                            dist = i2;
                            retval = IntersectResult.Inprim;
                        }
                    }
                    else
                    {
                        if (i1 < dist)
                        {
                            dist = i1;
                            retval = IntersectResult.Hit;
                        }
                    }
                }
            }
            return retval;
        }
    }

    internal class Plane : Primitive
    {
        private readonly Vector3 Normal;
        private readonly float d;

        public Plane()
        {
            Normal = new Vector3(0, 0, 0);
            d = 0;
        }

        public Plane(Vector3 normal, float d)
        {
            Normal = normal;
            this.d = d;
        }

        public float D
        {
            get { return d; }
        }

        public Vector3 GetNormal()
        {
            return Normal;
        }

        public override Vector3 GetNormal(Vector3 position)
        {
            return Normal;
        }

        public override IntersectResult Intersect(Ray ray, ref float dist)
        {
            float dot = Vector3.Dot(Normal, ray.Direction);
            if (Math.Abs(dot) > float.Epsilon*10)
            {
                float dist2 = -(Vector3.Dot(Normal, ray.Origin) + d)/dot;
                if (dist2 > 0)
                {
                    if (dist2 < dist)
                    {
                        dist = dist2;
                        return IntersectResult.Hit;
                    }
                }
            }
            return IntersectResult.Miss;
        }
    }

    internal class Material
    {
        public Material()
        {
            Color = new Color4(0.2f, 0.2f, 0.2f, 1);
            Reflection = 0;
            Diffuse = 0.2f;
        }

        public float Diffuse { get; set; }
        public float Reflection { get; set; }
        public Color4 Color { get; set; }

        public float Specular
        {
            get { return 1.0f - Diffuse; }
        }
    }
}