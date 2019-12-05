using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Chapter5_2
{
    public struct Hit_record
    {
        public float t;
        public Vector3 hitpoint;
        public Vector3 normal;
    };


    public interface Hitable
    {
        bool Hit(Ray r, ref float t_min, ref float t_max, out Hit_record record);
    };

    public class Sphere : Hitable
    {
        public Vector3 center;

        public float radius;

        public Sphere()
        {
            center = Vector3.zero;
            radius = 1;
        }

        public Sphere(Vector3 c, float r)
        {
            center = c;
            radius = r;
        }

        public bool Hit(Ray ray, ref float t_min, ref float t_max, out Hit_record record)
        {
            record = new Hit_record();
            Vector3 oc = ray.origin - center;
            float a = Vector3.Dot(ray.direction, ray.direction);
            float b = Vector3.Dot(oc, ray.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float d = b * b - a * c;
            if (d > 0)
            {
                float temp = (-b - Mathf.Sqrt(d)) / a;
                if (temp < t_max && temp > t_min)
                {
                    record.t = temp;
                    record.hitpoint = ray.GetPoint(temp);
                    record.normal = (record.hitpoint - center) / radius;
                    return true;
                }
                temp = (-b + Mathf.Sqrt(d)) / a;
                if (temp < t_max && temp > t_min)
                {
                    record.t = temp;
                    record.hitpoint = ray.GetPoint(temp);
                    record.normal = (record.hitpoint - center) / radius;
                    return true;
                }
            }
            return false;
        }
    }

    public class HitList : Hitable
    {
        private List<Hitable> list = new List<Hitable>();

        public HitList()
        {
        }

        public int GetCount()
        {
            return list.Count;
        }

        public void Add(Hitable item)
        {
            list.Add(item);
        }

        public bool Hit(Ray r, ref float t_min, ref float t_max, out Hit_record record)
        {
            Hit_record temp_rec = new Hit_record();
            record = temp_rec;
            bool hit_anything = false;
            float closest_so_far = t_max;

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].Hit(r, ref t_min, ref closest_so_far, out temp_rec))
                {
                    hit_anything = true;
                    closest_so_far = temp_rec.t;
                    record = temp_rec;
                }
            }

            return hit_anything;
        }
    }

    public class Chapter5_2
    {

        private static Vector3 topColor = Vector3.one;
        private static Vector3 bottomColor = new Vector3(0.5f, 0.7f, 1.0f);

        public static Vector3 RayCast(Ray ray, Hitable world)
        {
            Hit_record rec;
            float min = 0;
            float max = float.MaxValue;
            if (world.Hit(ray, ref min, ref max, out rec))
            {
                return 0.5f * (rec.normal.normalized + Vector3.one);
            }
            else
            {
                Vector3 unit_direction = ray.direction.normalized;
                float t = 0.5f * (unit_direction.y + 1.0f);
                return Vector3.Lerp(topColor, bottomColor, t);
            }
        }

        [MenuItem("Raytracing/Chapter5/2")]
        public static void Main()
        {
            int nx = 1280;
            int ny = 640;

            Vector3 lower_left_corner = new Vector3(-2.0f, -1.0f, -1.0f);
            Vector3 horizontal = new Vector3(4.0f, 0.0f, 0.0f);
            Vector3 vertical = new Vector3(0.0f, 2.0f, 0.0f);
            Vector3 origin = Vector3.zero;

            HitList list = new HitList();
            list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f));
            list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100));



            Texture2D tex = ImageHelper.CreateImg(nx, ny);
            for (int j = ny - 1; j >= 0; --j)
            {
                for (int i = 0; i < nx; ++i)
                {
                    float u = (float)(i) / (float)(nx);
                    float v = (float)(j) / (float)(ny);

                    Ray r = new Ray(origin, lower_left_corner + u * horizontal + v * vertical);
                    Vector3 color = RayCast(r, list);

                    ImageHelper.SetPixel(tex, i, j, color);
                }
            }

            ImageHelper.SaveImg(tex, "Img/Chapter5_2.png");            
        }
    }
}