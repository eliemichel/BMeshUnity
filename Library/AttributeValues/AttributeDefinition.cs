using UnityEngine;

namespace BMeshLib
{
    /**
    * Attributes definitions are stored in the mesh to automatically add an
    * attribute with a default value to all existing and added topological
    * entities of the target type.
    */
    public class AttributeDefinition
    {
        public string name;
        public AttributeType type;
        public AttributeValue defaultValue;

        public AttributeDefinition(string name, AttributeBaseType baseType, int dimensions)
        {
            this.name = name;
            type = new AttributeType { baseType = baseType, dimensions = dimensions };
            defaultValue = NullValue();
        }

        /**
         * Return a null value of the target type
         * (should arguably be in AttributeType)
         */
        public AttributeValue NullValue()
        {
            //Debug.Assert(type.dimensions > 0);
            switch (type.baseType)
            {
                case AttributeBaseType.Int:
                    return new IntAttributeValue { data = new int[type.dimensions] };
                case AttributeBaseType.Float:
                    return new FloatAttributeValue { data = new float[type.dimensions] };
                default:
                    Debug.Assert(false);
                    return new AttributeValue();
            }
        }
    }
}
