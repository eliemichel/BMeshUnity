using UnityEngine;

namespace BMeshLib
{
    public class IntAttributeValue : AttributeValue
    {
        public int[] data;

        public IntAttributeValue() { }
        public IntAttributeValue(int i)
        {
            data = new int[] { i };
        }
        public IntAttributeValue(int i0, int i1)
        {
            data = new int[] { i0, i1 };
        }

        public static float Distance(IntAttributeValue value1, IntAttributeValue value2)
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
