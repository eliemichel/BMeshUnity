using UnityEngine;

namespace BMeshLib
{
    /**
    * The generic class of values stored in the attribute dictionnary in each
    * topologic entity. It contains an array of float or int, depending on its
    * type.
    */
    public class AttributeValue
    {
        /**
         * Deep copy of an attribute value.
         */
        public static AttributeValue Copy(AttributeValue value)
        {
            if (value is IntAttributeValue valueAsInt)
            {
                var data = new int[valueAsInt.data.Length];
                valueAsInt.data.CopyTo(data, 0);
                return new IntAttributeValue { data = data };
            }
            if (value is FloatAttributeValue valueAsFloat)
            {
                var data = new float[valueAsFloat.data.Length];
                valueAsFloat.data.CopyTo(data, 0);
                return new FloatAttributeValue { data = data };
            }
            Debug.Assert(false);
            return null;
        }

        /**
         * Measure the euclidean distance between two attributes, which is set
         * to infinity if they have different types (int or float / dimension)
         */
        public static float Distance(AttributeValue value1, AttributeValue value2)
        {
            if (value1 is IntAttributeValue value1AsInt)
            {
                if (value2 is IntAttributeValue value2AsInt)
                {
                    return IntAttributeValue.Distance(value1AsInt, value2AsInt);
                }
            }
            if (value1 is FloatAttributeValue value1AsFloat)
            {
                if (value2 is FloatAttributeValue value2AsFloat)
                {
                    return FloatAttributeValue.Distance(value1AsFloat, value2AsFloat);
                }
            }
            return float.PositiveInfinity;
        }

        /**
         * Cast to FloatAttributeValue (return null if it was not actually a
         * float attribute).
         */
        public FloatAttributeValue asFloat()
        {
            return this as FloatAttributeValue;
        }

        /**
         * Cast to IntAttributeValue (return null if it was not actually an
         * integer attribute).
         */
        public IntAttributeValue asInt()
        {
            return this as IntAttributeValue;
        }
    }
}
