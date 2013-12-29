using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;

namespace RaytracerOpenTK
{
    internal class Engine
    {
        private const int TRACEDEPTH = 6;
        private const float EPSILON = 0.0001f;
        private readonly Bitmap bmp;
        protected float Delta_X, Delta_Y;

        protected float DirectionRay_X, DirectionRay_Y;
        protected int Height;
        protected Scene Scene;

        protected float ScreenPlaneWorld_X1;
        protected float ScreenPlaneWorld_X2;
        protected float ScreenPlaneWorld_Y1;
        protected float ScreenPlaneWorld_Y2;
        protected int Width;

        public Engine(Scene scene, int width, int height)
        {
            Scene = scene;
            Height = height;
            Width = width;
            bmp = new Bitmap(width, height);
            InitRender();
        }

        public Scene GetScene()
        {
            return Scene;
        }

        public Primitive Raytrace(Ray a_Ray, ref Color4 a_Acc, int a_Depth, float a_RIndex)
        {
            if (a_Depth > TRACEDEPTH) return null;
            // trace primary ray
            float a_Dist = 1000000.0f;
            Primitive prim = null;
            // find the nearest intersection
            foreach (Primitive pr in Scene.Primitives)
            {
                IntersectResult res = pr.Intersect(a_Ray, ref a_Dist);
                if (res != IntersectResult.Miss)
                {
                    prim = pr;
                }
            }
            // no hit, terminate ray
            if (prim == null) return null;
            // handle intersection
            if (prim.IsLight)
            {
                // we hit a light, stop tracing
                a_Acc = new Color4(1, 1, 1, 1);
            }
            else
            {
                // determine color at point of intersection
                Vector3 intersectionPoint = a_Ray.Origin + a_Ray.Direction*a_Dist;
                // trace lights
                foreach (Primitive p in Scene.Primitives)
                {
                    if (!p.IsLight) continue;
                    // calculate diffuse shading
                    var light = p as Sphere;
                    if (light == null) continue;
                    // handle point light source
                    float shade = 1.0f;

                    Vector3 lightBeam = light.Centre - intersectionPoint;
                    lightBeam.Normalize();
                    Vector3 normal = prim.GetNormal(intersectionPoint);

                    if (prim.Material.Diffuse > 0.0f)
                        CalculateDiffuseLight(ref a_Acc, normal, lightBeam, prim, light);

                    if (prim.Material.Specular > 0.0f)
                        CalculateSpecularLight(a_Ray, ref a_Acc, lightBeam, normal, prim, shade, light);
                }
                // calculate reflection
                CalculateReflection(a_Ray, ref a_Acc, a_Depth, a_RIndex, prim, intersectionPoint);
            }
            // return pointer to primitive hit by primary ray
            return prim;
        }

        private void CalculateReflection(Ray a_Ray, ref Color4 a_Acc, int a_Depth, float a_RIndex, Primitive prim,
            Vector3 intersectionPoint)
        {
            float refl = prim.Material.Reflection;
            if (refl > 0.0f)
            {
                Vector3 N = prim.GetNormal(intersectionPoint);
                Vector3 R = a_Ray.Direction - 2.0f*Vector3.Dot(a_Ray.Direction, N)*N;
                if (a_Depth < TRACEDEPTH)
                {
                    var rcol = new Color4(0, 0, 0, 1);
                    //recursive raytrace
                    Raytrace(new Ray(intersectionPoint + R*EPSILON, R), ref rcol, a_Depth + 1, a_RIndex);
                    BlendReflectColors(ref a_Acc, refl, rcol, prim.Material.Color);
                }
            }
        }

        private void CalculateDiffuseLight(ref Color4 a_Acc, Vector3 normal, Vector3 lightBeam, Primitive prim,
            Sphere light)
        {
            float dot = Vector3.Dot(normal, lightBeam);
            if (dot > 0)
            {
                float diff = dot*prim.Material.Diffuse;
                // add diffuse component to ray color
                BlendDiffuseColors(diff, prim.Material.Color, light.Material.Color, ref a_Acc);
            }
        }

        private void CalculateSpecularLight(Ray a_Ray, ref Color4 a_Acc, Vector3 lightBeam, Vector3 normal,
            Primitive prim,
            float shade, Sphere light)
        {
            Vector3 V = a_Ray.Direction;
            Vector3 R = lightBeam - 2.0f*Vector3.Dot(lightBeam, normal)*normal;

            float dot = Vector3.Dot(V, R);

            if (dot > 0)
            {
                float spec = (float) Math.Pow(dot, 20)*prim.Material.Specular*shade;
                // add specular component to ray color
                BlendSpecularColors(ref a_Acc, spec, light.Material.Color);
            }
        }

        private void BlendSpecularColors(ref Color4 aAcc, float spec, Color4 color)
        {
            aAcc.R += spec*color.R;
            aAcc.G += spec*color.G;
            aAcc.B += spec*color.B;
            aAcc.A += spec*color.A;
        }

        private void BlendReflectColors(ref Color4 aAcc, float refl, Color4 rcol, Color4 color)
        {
            aAcc.R += refl*rcol.R*color.R;
            aAcc.G += refl*rcol.G*color.G;
            aAcc.B += refl*rcol.B*color.B;
            aAcc.A += refl*rcol.A*color.A;
        }

        private void BlendDiffuseColors(float diff, Color4 color1, Color4 color2, ref Color4 a_Acc)
        {
            a_Acc.R += diff*color1.R*color2.R;
            a_Acc.G += diff*color1.G*color2.G;
            a_Acc.B += diff*color1.B*color2.B;
            a_Acc.A += diff*color1.A*color2.A;
        }

        private void InitRender()
        {
            // screen plane in world space coordinates
            ScreenPlaneWorld_X1 = -4;
            ScreenPlaneWorld_X2 = 4;
            ScreenPlaneWorld_Y1 = DirectionRay_Y = 3;
            ScreenPlaneWorld_Y2 = -3;
            // calculate deltas for interpolation
            Delta_X = (ScreenPlaneWorld_X2 - ScreenPlaneWorld_X1)/Width;
            Delta_Y = (ScreenPlaneWorld_Y2 - ScreenPlaneWorld_Y1)/Height;
            DirectionRay_Y += 20*Delta_Y;
            // allocate space to store pointers to primitives for previous line
            new List<Primitive>(Width);
        }

        public void Render()
        {
            // render scene
            // rays are spawned from the origin
            var origin = new Vector3(0, 0, -5);
            // render remaining lines
            for (int y = 0; y < (Height - 20); y++)
            {
                DirectionRay_X = ScreenPlaneWorld_X1;
                // render pixels for current line
                for (int x = 0; x < Width; x++)
                {
                    // fire primary ray
                    var acc = new Color4(0, 0, 0, 0); //default color for pixel
                    Vector3 direction = new Vector3(DirectionRay_X, DirectionRay_Y, 0) - origin;
                    direction.Normalize();

                    var fireRay = new Ray(origin, direction);
                    Raytrace(fireRay, ref acc, 1, 1.0f);
                    var red = (int) (acc.R*256);
                    var green = (int) (acc.G*256);
                    var blue = (int) (acc.B*256);
                    if (red > 255) red = 255;
                    if (green > 255) green = 255;
                    if (blue > 255) blue = 255;
                    bmp.SetPixel(x, y, Color.FromArgb(red, green, blue));
                    DirectionRay_X += Delta_X;
                }
                DirectionRay_Y += Delta_Y;
            }
        }

        public void SaveBmp()
        {
            bmp.Save(string.Format("snapshot.bmp"), ImageFormat.Bmp);
        }
    }
}