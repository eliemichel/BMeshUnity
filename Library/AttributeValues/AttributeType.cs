using System.Data;
using System.Collections.Generic;
using UnityEngine;

namespace BMeshLib
{
    /**
    * Attributes are arbitrary data that can be attached to topologic entities.
    * There are identified by a name and their value is an array of either int
    * or float. This array has theoretically a fixed size but in practice you
    * can do whatever you want becase they are stored per entity, not in a
    * global buffer, so it is flexible. Maybe one day for better efficiency
    * they would use proper data buffers, but the API would change anyway at
    * that point.
    */

    public enum AttributeBaseType
    {
        Int,
        Float,
    }

    /**
    * Attribute type is used when declaring new attributes to be automatically
    * attached to topological entities, using Add*Attributes() methods.
    */
    public class AttributeType
    {
        public AttributeBaseType baseType;
        public int dimensions;

        /**
         * Checks whether a given value matches this type.
         */
        public bool CheckValue(AttributeValue value)
        {
            Debug.Assert(dimensions > 0);
            switch (baseType)
            {
                case AttributeBaseType.Int:
                    {
                        var valueAsInt = value as IntAttributeValue;
                        return valueAsInt != null && valueAsInt.data.Length == dimensions;
                    }
                case AttributeBaseType.Float:
                    {
                        var valueAsFloat = value as FloatAttributeValue;
                        return valueAsFloat != null && valueAsFloat.data.Length == dimensions;
                    }
                default:
                    Debug.Assert(false);
                    return false;
            }
        }
    }
}
