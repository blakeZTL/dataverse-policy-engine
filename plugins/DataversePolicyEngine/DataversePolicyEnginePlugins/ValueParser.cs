using System;
using Microsoft.Xrm.Sdk;

namespace DataversePolicyEnginePlugins
{
    public static class ValueParser
    {
        public static object GetValue(Entity entity, string attributeName, string valueType)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new ArgumentException(
                    "Attribute name cannot be null or empty.",
                    nameof(attributeName)
                );
            }

            if (string.IsNullOrWhiteSpace(valueType))
            {
                throw new ArgumentException(
                    "Value type cannot be null or empty.",
                    nameof(valueType)
                );
            }

            switch (valueType)
            {
                case ValueColumnType.String:
                    return entity.GetAttributeValue<string>(attributeName);
                case ValueColumnType.Int:
                    return entity.GetAttributeValue<int>(attributeName);
                case ValueColumnType.Decimal:
                    return entity.GetAttributeValue<decimal>(attributeName);
                case ValueColumnType.Boolean:
                    return entity.GetAttributeValue<bool>(attributeName);
                case ValueColumnType.DateTime:
                    return entity.GetAttributeValue<DateTime>(attributeName);
                case ValueColumnType.EntityReference:
                    return entity.GetAttributeValue<EntityReference>(attributeName).Id;
                case ValueColumnType.OptionSetValue:
                    return entity.GetAttributeValue<OptionSetValue>(attributeName).Value;
                default:
                    throw new ArgumentException(
                        $"Unsupported value type: {valueType}",
                        nameof(valueType)
                    );
            }
        }
    }
}
