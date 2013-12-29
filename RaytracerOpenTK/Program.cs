using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using RaytracerOpenTK;

namespace Example
{
    internal class MyApplication
    {
        [STAThread]
        public static void Main()
        {
            var tracer = new Engine(InitScene(), 800, 600);
            tracer.Render();
            tracer.SaveBmp();
        }

        public static Scene InitScene()
        {
            var entities = new List<Primitive>
            {
                //Add objects
                new Sphere(new Vector3(1, -0.8f, 3), 2.5f)
                {
                    Name = "big sphere",
                    Material = new Material {Reflection = 0.6f, Color = new Color4(1f, 0.7f, 1.0f, 1)}
                },
                new Sphere(new Vector3(-5.5f, -0.5f, 7), 2)
                {
                    Name = "small sphere",
                    Material = new Material {Reflection = 1.0f, Diffuse = 0.1f, Color = new Color4(0.7f, 0.7f, 0.7f, 1)}
                },
                new Plane(new Vector3(0, 1, 0), 4.4f)
                {
                    Name = "plane",
                    Material = new Material {Reflection = 0, Diffuse = 1.0f, Color = new Color4(0.4f, 0.3f, 0.3f, 1)}
                },
                //Add light sources
                new Sphere(new Vector3(0, 5, 5), 0.1f)
                {
                    IsLight = true,
                    Material = new Material {Color = new Color4(0.6f, 0.6f, 0.6f, 1)}
                },
                new Sphere(new Vector3(2, 5, 1), 0.1f)
                {
                    IsLight = true,
                    Material = new Material {Color = new Color4(0.7f, 0.7f, 0.9f, 1)}
                }
            };
            return new Scene(entities);
        }
    }
}