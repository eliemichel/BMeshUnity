using UnityEngine;

namespace BMeshLib
{
    public class FloatAttributeValue : AttributeValue
    {
        public float[] data;

        public FloatAttributeValue() { }
        public FloatAttributeValue(float f)
        {
            data = new float[] { f };
        }
        public FloatAttributeValue(float f0, float f1)
        {
            data = new float[] { f0, f1 };
        }
        public FloatAttributeValue(Vector3 v)
        {
            data = new float[] { v.x, v.y, v.z };
        }

        public void FromVector2(Vector2 v)
        {
            data[0] = v.x;
            data[1] = v.y;
        }
        public void FromVector3(Vector3 v)
        {
            data[0] = v.x;
            data[1] = v.y;
            data[2] = v.z;
        }
        public void FromColor(Color c)
        {
            data[0] = c.r;
            data[1] = c.g;
            data[2] = c.b;
            data[3] = c.a;
        }

        public Vector3 AsVector3()
        {
            return new Vector3(
                data.Length > 0 ? data[0] : 0,
                data.Length > 1 ? data[1] : 0,
                data.Length > 2 ? data[2] : 0
            );
        }
        public Color AsColor()
        {
            return new Color(
                data.Length > 0 ? data[0] : 0,
                data.Length > 1 ? data[1] : 0,
                data.Length > 2 ? data[2] : 0,
                data.Length > 3 ? data[3] : 1
            );
        }

        public static float Distance(FloatAttributeValue value1, FloatAttributeValue value2)
        {
            int n = value1.data.Length;
            if (n != value2.data.Length) return float.PositiveInfinity;
            float s = 0;
            for (int i = 0; i < n; ++i)
            {
                float diff = value1.data[i] - value2.data[i];
                s += diff * diff;
            }
            return Mathf.Sqrt(s);
        }
    }
}
