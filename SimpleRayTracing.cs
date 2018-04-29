using System.Collections;
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace SimpleRT
{

    public class RayTracingDemo : EditorWindow
    {
        [MenuItem("Noobdawn/光线追踪渲染器")]
        public static void OnClick()
        {
            RayTracingDemo window = GetWindow<RayTracingDemo>();
            window.Show();
        }

        void OnGUI()
        {
            if (GUILayout.Button("测试图片"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestPNG(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试射线"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestRay(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试简单球体"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestSphere(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试球体法线"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestNormal(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("抽象碰撞信息"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestHitRecord(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试反锯齿"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestAntialiasing(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试散射模型"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestDiffusing(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试镜面模型"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestMetal(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试透明模型"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestDielectric(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试相机FOV和位置角度"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestCamera(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试景深"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestDefocus(WIDTH, HEIGHT));
            }
        }
        #region 参数设定
        const string IMG_PATH = @"C:\RT\1.png";
        const int WIDTH = 400;
        const int HEIGHT = 200;
        const int SAMPLE = 100;
        const float SAMPLE_WEIGHT = 0.01f;
        const int MAX_SCATTER_TIME = 50;
        #endregion
        #region 第一版（测试输出图片）
        Color[] CreateColorForTestPNG(int width, int height)
        {
            int l = width * height;
            Color[] colors = new Color[l];
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    colors[i + j * width] = new Color(
                        i / (float)width,
                        j / (float)height,
                        0.2f);
                }
            return colors;
        }
        #endregion
        #region 第二版（测试射线、简单的摄像机和背景）
        Color GetColorForTestRay(Ray ray)
        {
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1,1,1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestRay(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            Color[] colors = new Color[l];
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                    colors[i + j * width] = GetColorForTestRay(r);
                }
            return colors;
        }
        #endregion
        #region 第三版（测试一个简单的球体）
        bool isHitSphereForTestSphere(Vector3 center, float radius, Ray ray)
        {
            var oc = ray.original - center;
            float a = Vector3.Dot(ray.direction, ray.direction);
            float b = 2f * Vector3.Dot(oc, ray.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            //实际上是判断这个方程有没有根，如果有2个根就是击中
            float discriminant = b * b - 4 * a * c;
            return (discriminant > 0);
        }

        Color GetColorForTestSphere(Ray ray)
        {
            if (isHitSphereForTestSphere(new Vector3(0, 0, -1), 0.5f, ray))
                return new Color(1, 0, 0);
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestSphere(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            Color[] colors = new Color[l];
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                    colors[i + j * width] = GetColorForTestSphere(r);
                }
            return colors;
        }
        #endregion
        #region 第四版（测试球体的表面法线）
        float HitSphereForTestNormal(Vector3 center, float radius, Ray ray)
        {
            var oc = ray.original - center;
            float a = Vector3.Dot(ray.direction, ray.direction);
            float b = 2f * Vector3.Dot(oc, ray.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            //实际上是判断这个方程有没有根，如果有2个根就是击中
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                return -1;
            }
            else
            {
                //返回距离最近的那个根
                return (-b - Mathf.Sqrt(discriminant)) / (2f * a);
            }
        }

        Color GetColorForTestNormal(Ray ray)
        {
            float t = HitSphereForTestNormal(new Vector3(0, 0, -1), 0.5f, ray);
            if (t > 0)
            {
                Vector3 normal = Vector3.Normalize(ray.GetPoint(t) - new Vector3(0,0,-1));
                return 0.5f * new Color(normal.x + 1, normal.y + 1, normal.z + 1, 2f);
            }
            t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestNormal(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            Color[] colors = new Color[l];
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                    colors[i + j * width] = GetColorForTestNormal(r);
                }
            return colors;
        }
        #endregion
        #region 第五版（测试Hit的抽象）
        Color GetColorForTestHitRecord(Ray ray, HitableList hitableList)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0f, float.MaxValue, ref record))
            {
                return 0.5f * new Color(record.normal.x + 1, record.normal.y + 1, record.normal.z + 1, 2f);
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestHitRecord(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            hitableList.list.Add(new SimpleSphere(new Vector3(0, 0, -1), 0.5f));
            hitableList.list.Add(new SimpleSphere(new Vector3(0, -100.5f, -1), 100f));
            Color[] colors = new Color[l];
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                    colors[i + j * width] = GetColorForTestHitRecord(r, hitableList);
                }
            return colors;
        }
        #endregion
        #region 第六版（测试抗锯齿）
        Color GetColorForTestAntialiasing(Ray ray, HitableList hitableList)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0f, float.MaxValue, ref record))
            {
                return 0.5f * new Color(record.normal.x + 1, record.normal.y + 1, record.normal.z + 1, 2f);
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestAntialiasing(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            hitableList.list.Add(new SimpleSphere(new Vector3(0, 0, -1), 0.5f));
            hitableList.list.Add(new SimpleSphere(new Vector3(0, -100.5f, -1), 100f));
            Color[] colors = new Color[l];
            SimpleCamera camera = new SimpleCamera(original, lowLeftCorner, horizontal, vertical);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0,0,0);
                    for (int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestAntialiasing(r, hitableList);
                    }
                    color *= SAMPLE_WEIGHT;
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            return colors;
        }
        #endregion
        #region 第七版（测试Diffuse）
        //此处用于取得无序的反射方向，并用于模拟散射模型
        Vector3 GetRandomPointInUnitSphereForTestDiffusing()
        {
            Vector3 p = 2f * new Vector3(_M.R(), _M.R(), _M.R()) - Vector3.one;
            p = p.normalized * _M.R();
            //Vector3 p = Vector3.zero;
            //do
            //{
            //    p = 2f * new Vector3(_M.R(), _M.R(), _M.R()) - Vector3.one;
            //}
            //while (p.sqrMagnitude > 1f);
            return p;
        }
        
        Color GetColorForTestDiffusing(Ray ray, HitableList hitableList)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Vector3 target = record.p + record.normal + GetRandomPointInUnitSphereForTestDiffusing();
                //此处假定有50%的光被吸收，剩下的则从入射点开始取随机方向再次发射一条射线
                return 0.5f * GetColorForTestDiffusing(new Ray(record.p, target - record.p), hitableList);
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestDiffusing(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            hitableList.list.Add(new SimpleSphere(new Vector3(0, 0, -1), 0.5f));
            hitableList.list.Add(new SimpleSphere(new Vector3(0, -100.5f, -1), 100f));
            Color[] colors = new Color[l];
            SimpleCamera camera = new SimpleCamera(original, lowLeftCorner, horizontal, vertical);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestDiffusing(r, hitableList);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    //color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            return colors;
        }
        #endregion
        #region 第八版（测试镜面）
        Color GetColorForTestMetal(Ray ray, HitableList hitableList, int depth)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;
                if (depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestMetal(r, hitableList, depth + 1);
                    return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
                }
                else
                {
                    //假设已经反射了太多次，或者压根就没有发生反射，那么就认为黑了
                    return Color.black;
                }
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestMetal(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.8f, 0.3f, 0.3f))));
            hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f), 0.3f)));
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.8f, 0.8f), 1.0f)));
            Color[] colors = new Color[l];
            SimpleCamera camera = new SimpleCamera(original, lowLeftCorner, horizontal, vertical);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestMetal(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            return colors;
        }
        #endregion
        #region 第九版（测试透明）
        Color GetColorForTestDielectric(Ray ray, HitableList hitableList, int depth)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;
                if (depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestDielectric(r, hitableList, depth + 1);
                    return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
                }
                else
                {
                    //假设已经反射了太多次，或者压根就没有发生反射，那么就认为黑了
                    return Color.black;
                }
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestDielectric(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.8f, 0.3f, 0.3f))));
            hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f), 0f)));
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Dielectirc(1.5f)));
            Color[] colors = new Color[l];
            SimpleCamera camera = new SimpleCamera(original, lowLeftCorner, horizontal, vertical);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestDielectric(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            return colors;
        }
        #endregion
        #region 第十版（测试FOV和相机角度）
        Color GetColorForTestCamera(Ray ray, HitableList hitableList, int depth)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;
                if (depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestCamera(r, hitableList, depth + 1);
                    return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
                }
                else
                {
                    //假设已经反射了太多次，或者压根就没有发生反射，那么就认为黑了
                    return Color.black;
                }
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestCamera(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.8f, 0.3f, 0.3f))));
            hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f), 0f)));
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Dielectirc(1.5f)));
            Color[] colors = new Color[l];
            Camera camera = new Camera(new Vector3(-2,1f,-1), new Vector3(-1,0,-1),  Vector3.up, 75, width / height);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestCamera(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            return colors;
        }
        #endregion
        #region 第十一版（测试景深）
        Color GetColorForTestDefocus(Ray ray, HitableList hitableList, int depth)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;
                if (depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestDefocus(r, hitableList, depth + 1);
                    return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
                }
                else
                {
                    //假设已经反射了太多次，或者压根就没有发生反射，那么就认为黑了
                    return Color.black;
                }
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestDefocus(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            //这里注释的两句话是随机场景渲染用的……
            //HitableList hitableList = _M.CreateRandomScene();
            hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.2f, 0.2f, 0.8f))));
            hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f), 0f)));
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Dielectirc(1.5f)));
            Color[] colors = new Color[l];
            Vector3 from = new Vector3(10, 2f, -2);
            Vector3 to = new Vector3(0, 1, 0);
            Camera camera = new Camera(from, to, Vector3.up, 20, width / height, 2, (from - to).magnitude);
            //Camera camera = new Camera(from, to, Vector3.up, 35, width / height);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
            {
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestDefocus(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
                EditorUtility.DisplayProgressBar("", "", j / (float)height);
            }
            EditorUtility.ClearProgressBar();
            return colors;
        }
        #endregion
        #region 图像生成
        void CreatePng(int width, int height, Color[] colors)
        {
            if (width * height != colors.Length)
            {
                EditorUtility.DisplayDialog("ERROR", "长宽与数组长度无法对应！", "ok");
                return;
            }
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.SetPixels(colors);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            FileStream fs = new FileStream(IMG_PATH, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(bytes);
            fs.Close();
            bw.Close();
        }
        #endregion

    }
    #region 辅助类
    public class Ray
    {
        public Vector3 original;
        public Vector3 direction;
        public Vector3 normalDirection;
        public Ray(Vector3 o, Vector3 d)
        {
            original = o;
            direction = d;
            normalDirection = d.normalized;
        }

        public Vector3 GetPoint(float t)
        {
            return original + t * direction;
        }
    }

    public class HitRecord
    {
        public float t;
        public Vector3 p;
        public Vector3 normal;
        public Material material;
    }

    public abstract class Hitable
    {
        public abstract bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec);
    }

    public class HitableList : Hitable
    {
        public List<Hitable> list;
        public HitableList() { list = new List<Hitable>(); }
        /// <summary>
        /// 返回所有Hitable中最靠近射线源的命中信息
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="t_min"></param>
        /// <param name="t_max"></param>
        /// <param name="rec"></param>
        /// <returns></returns>
        public override bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec)
        {
            HitRecord tempRecord = new HitRecord();
            bool hitAnything = false;
            float closest = t_max;
            foreach(var h in list)
            {
                if (h.Hit(ray, t_min, closest, ref tempRecord))
                {
                    hitAnything = true;
                    closest = tempRecord.t;
                    rec = tempRecord;
                }
            }
            return hitAnything;
        }
    }
    /// <summary>
    /// 前几个部分使用的简化版摄像机
    /// </summary>
    public class SimpleCamera
    {
        public Vector3 position;
        public Vector3 lowLeftCorner;
        public Vector3 horizontal;
        public Vector3 vertical;
        public SimpleCamera(Vector3 pos, Vector3 llc, Vector3 hor, Vector3 ver)
        {
            position = pos;
            lowLeftCorner = llc;
            horizontal = hor;
            vertical = ver;
        }
        public Ray CreateRay(float u, float v)
        {
            return new Ray(position, lowLeftCorner + u * horizontal + v * vertical - position);
        }
    }

    public class Camera
    {
        public Vector3 position;
        public Vector3 lowLeftCorner;
        public Vector3 horizontal;
        public Vector3 vertical;
        public Vector3 u, v, w;
        public float radius;
        ///此处FOV是欧拉角
        public Camera(Vector3 lookFrom, Vector3 lookat, Vector3 vup, float vfov, float aspect, float r = 0, float focus_dist = 1)
        {
            radius = r * 0.5f;
            float unitAngle = Mathf.PI / 180f * vfov;
            float halfHeight = Mathf.Tan(unitAngle * 0.5f);
            float halfWidth = aspect * halfHeight;
            position = lookFrom;
            w = (lookat - lookFrom).normalized;
            u = Vector3.Cross(vup, w).normalized;
            v = Vector3.Cross(w, u).normalized;
            lowLeftCorner = lookFrom + w * focus_dist - halfWidth * u * focus_dist - halfHeight * v * focus_dist;
            horizontal = 2 * halfWidth * focus_dist * u;
            vertical = 2 * halfHeight * focus_dist * v;
        }
        public Ray CreateRay(float x, float y)
        {
            ///假如光圈为0就不随机了，节省资源
            if (radius == 0f)
                return new Ray(position, lowLeftCorner + x * horizontal + y * vertical - position);
            else
            {
                Vector3 rd = radius * _M.GetRandomPointInUnitDisk();
                Vector3 offset = rd.x * u + rd.y * v;
                return new Ray(position + offset, lowLeftCorner + x * horizontal + y * vertical - position - offset);
            }
        }
    }
    public abstract class Material
    {
        /// <summary>
        /// 材质表面发生的光线变化过程
        /// </summary>
        /// <param name="rayIn"></param>
        /// <param name="record"></param>
        /// <param name="attenuation">衰减</param>
        /// <param name="scattered"></param>
        /// <returns>是否发生了光线变化</returns>
        public abstract bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered);
    }

    public static class _M
    {
        public static Vector3 GetRandomPointInUnitSphere()
        {
            Vector3 p = 2f * new Vector3(_M.R(), _M.R(), _M.R()) - Vector3.one;
            p = p.normalized * _M.R();
            return p;
        }
        /// <summary>
        /// 这个取的是圆面而不是球
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetRandomPointInUnitDisk()
        {
            Vector3 p = 2f * new Vector3(_M.R(), _M.R(), 0) - new Vector3(1, 1, 0);
            p = p.normalized * _M.R();
            return p;
        }

        public static Vector3 reflect(Vector3 vin, Vector3 normal)
        {
            return vin - 2 * Vector3.Dot(vin, normal) * normal;
        }

        public static bool refract(Vector3 vin, Vector3 normal, float ni_no, ref Vector3 refracted)
        {
            Vector3 uvin = vin.normalized;
            float dt = Vector3.Dot(uvin, normal);
            float discrimination = 1 - ni_no * ni_no * (1 - dt * dt);
            if (discrimination > 0)
            {
                refracted = ni_no * (uvin - normal * dt) - normal * Mathf.Sqrt(discrimination);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Schilick近似菲涅尔反射
        /// </summary>
        /// <returns>返回的是菲涅尔反射比</returns>
        public static float schlick(float cos, float ref_idx)
        {
            float r0 = (1 - ref_idx) / (1 + ref_idx);
            r0 *= r0;
            return r0 + (1 - r0) * Mathf.Pow((1 - cos), 5);
        }

        public static float R()
        {
            return Random.Range(0f, 1f);
        }

        public static HitableList CreateRandomScene()
        {
            HitableList list = new HitableList();
            list.list.Add(new Sphere(new Vector3(0f, -1000f, 0f), 1000f, new Lambertian(Color.gray)));
            for (int a = -4; a < 4; a++)
                for (int b = -4; b < 4; b++)
                {
                    float choose_mat = R();
                    Vector3 center = new Vector3(a + 0.9f * R(), 0.2f, b + 0.9f * R());
                    if (Vector3.Distance(center, new Vector3(4, 0.2f, 4)) > 0.9f)
                    {
                        if (choose_mat < 0.8f)
                            list.list.Add(new Sphere(center, 0.2f, new Lambertian(new Color(R() * R(), R() * R(), R() * R()))));
                        else if (choose_mat < 0.95f)
                            list.list.Add(new Sphere(center, 0.2f, new Metal(new Color(0.5f + 0.5f * R(), 0.5f + 0.5f * R(), 0.5f + 0.5f * R()), 0.5f * R())));
                        else
                            list.list.Add(new Sphere(center, 0.2f, new Dielectirc(1.5f)));
                    }
                }
            list.list.Add(new Sphere(new Vector3(0, 1, 0), 1, new Dielectirc(1.5f)));
            list.list.Add(new Sphere(new Vector3(-4, 1, 0), 1, new Lambertian(new Color( 0.9f, 0.5f, 0.1f))));
            list.list.Add(new Sphere(new Vector3(4, 1, 0), 1, new Metal(new Color(0.7f, 0.6f, 0.5f), 0.01f)));
            return list;
        }
    }
    #endregion
    #region 各式各样的SDF类
    /// <summary>
    /// 前几个测试中用到的简化版SDF球
    /// </summary>
    public class SimpleSphere : Hitable
    {
        public Vector3 center;
        public float radius;
        public SimpleSphere(Vector3 cen, float rad)
        {
            center = cen; radius = rad;
        }
        public override bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec)
        {
            var oc = ray.original - center;
            float a = Vector3.Dot(ray.direction, ray.direction);
            float b = 2f * Vector3.Dot(oc, ray.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            //实际上是判断这个方程有没有根，如果有2个根就是击中
            float discriminant = b * b - 4 * a * c;
            if (discriminant > 0)
            {
                //带入并计算出最靠近射线源的点
                float temp = (-b - Mathf.Sqrt(discriminant)) / a * 0.5f;
                if (temp < t_max && temp > t_min)
                {
                    rec.t = temp;
                    rec.p = ray.GetPoint(rec.t);
                    rec.normal = (rec.p - center).normalized;
                    return true;
                }
                //否则就计算远离射线源的点
                temp = (-b + Mathf.Sqrt(discriminant)) / a * 0.5f;
                if (temp < t_max && temp > t_min)
                {
                    rec.t = temp;
                    rec.p = ray.GetPoint(rec.t);
                    rec.normal = (rec.p - center).normalized;
                    return true;
                }
            }
            return false;
        }
    }

    public class Sphere : Hitable
    {
        public Vector3 center;
        public float radius;
        public Material material;

        public Sphere (Vector3 cen, float rad, Material mat)
        {
            center = cen; radius = rad; material = mat;
        }

        public override bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec)
        {
            var oc = ray.original - center;
            float a = Vector3.Dot(ray.direction, ray.direction);
            float b = 2f * Vector3.Dot(oc, ray.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            //实际上是判断这个方程有没有根，如果有2个根就是击中
            float discriminant = b * b - 4 * a * c;
            if (discriminant > 0)
            {
                //带入并计算出最靠近射线源的点
                float temp = (-b - Mathf.Sqrt(discriminant)) / a * 0.5f;
                if (temp < t_max && temp > t_min)
                {
                    rec.t = temp;
                    rec.p = ray.GetPoint(rec.t);
                    rec.normal = (rec.p - center).normalized;
                    rec.material = material;
                    return true;
                }
                //否则就计算远离射线源的点
                temp = (-b + Mathf.Sqrt(discriminant)) / a * 0.5f;
                if (temp < t_max && temp > t_min)
                {
                    rec.t = temp;
                    rec.p = ray.GetPoint(rec.t);
                    rec.normal = (rec.p - center).normalized;
                    rec.material = material;
                    return true;
                }
            }
            return false;
        }
    }
    #endregion
    #region 各式各样的散射模型
    /// <summary>
    /// 理想的漫反射模型
    /// </summary>
    public class Lambertian : Material
    {
        Color albedo;
        public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
        {
            Vector3 target = record.p + record.normal + _M.GetRandomPointInUnitSphere();
            scattered = new Ray(record.p, target - record.p);
            attenuation = albedo;
            return true;
        }
        public Lambertian(Color a) { albedo = a; }
    }
    /// <summary>
    /// 理想的镜面反射模型
    /// </summary>
    public class Metal : Material
    {
        Color albedo;
        float fuzz;
        public Metal(Color a, float f = 0f) { albedo = a; fuzz = f < 1 ? f : 1; }
        public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
        {
            Vector3 reflected = _M.reflect(rayIn.normalDirection, record.normal);
            scattered = new Ray(record.p, reflected + fuzz * _M.GetRandomPointInUnitSphere());
            attenuation = albedo;
            return Vector3.Dot(scattered.direction, record.normal) > 0;
        }
    }
    /// <summary>
    /// 透明折射模型
    /// </summary>
    public class Dielectirc : Material
    {
        //相对空气的折射率
        float ref_idx;
        public Dielectirc(float ri) { ref_idx = ri; }
        public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
        {
            Vector3 outNormal;
            Vector3 reflected = _M.reflect(rayIn.direction, record.normal);
            //透明的物体当然不会吸收任何
            attenuation = Color.white;
            float ni_no = 1f;
            Vector3 refracted = Vector3.zero;
            float cos = 0;
            //反射比
            float reflect_prob = 0;
            //假如光线是从介质内向介质外传播，那么法线就要反转一下
            if (Vector3.Dot(rayIn.direction, record.normal) > 0)
            {
                outNormal = -record.normal;
                ni_no = ref_idx;
                cos = ni_no * Vector3.Dot(rayIn.normalDirection, record.normal);
            }
            else
            {
                outNormal = record.normal;
                ni_no = 1f / ref_idx;
                cos = -Vector3.Dot(rayIn.normalDirection, record.normal);
            }
            //如果没发生折射，就用反射
            if (_M.refract(rayIn.direction, outNormal, ni_no, ref refracted))
            {
                reflect_prob = _M.schlick(cos, ref_idx);
            }
            else
            {
                //此时反射比为100%
                reflect_prob = 1; 
            }
            //因为一条光线只会采样一个点，所以这里就用蒙特卡洛模拟的方法，用概率去决定数值
            if (_M.R() <= reflect_prob)
            {
                scattered = scattered = new Ray(record.p, reflected);
            }
            else
            {
                scattered = scattered = new Ray(record.p, refracted);
            }
            return true;
        }
    }
    #endregion
}