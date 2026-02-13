using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace DataversePolicyEnginePlugins
{
    public static class DataverseValueComparer
    {
        // 1) Compare effective value to policy expected value using configuredValueColumnType
        public static bool ValuesMatchConfigured(
            AttributeMetadata meta,
            object actual,
            object expected,
            string configuredValueColumnType
        )
        {
            actual = Unwrap(actual);
            expected = Unwrap(expected);

            if (actual == null && expected == null)
                return true;
            if (actual == null || expected == null)
                return false;

            // You said the policy columns are typed, so this is a reliable selector per step registration.
            switch (configuredValueColumnType)
            {
                case "OptionSetValue":
                    return ToOption(actual) == ToOption(expected);

                case "EntityReference":
                    return ToEntityRefId(actual) == ToEntityRefId(expected);

                case "String":
                    return string.Equals(
                        Convert.ToString(actual),
                        Convert.ToString(expected),
                        StringComparison.OrdinalIgnoreCase
                    );

                case "Boolean":
                    return ToBool(actual) == ToBool(expected);

                case "Money":
                    return ToMoney(actual) == ToMoney(expected);

                case "DateTime":
                    return ToUtc(actual) == ToUtc(expected);

                case "Int32":
                case "Integer":
                    return ToInt(actual) == ToInt(expected);

                case "Decimal":
                    return ToDecimal(actual) == ToDecimal(expected);

                default:
                    return actual.Equals(expected);
            }
        }

        // 2) Compare two entity values (new vs old) using metadata type (for allowed=false)
        public static bool ValuesEqualByMetadata(AttributeMetadata meta, object a, object b)
        {
            a = Unwrap(a);
            b = Unwrap(b);

            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;

            var t = meta.AttributeType;

            if (
                t == AttributeTypeCode.Picklist
                || t == AttributeTypeCode.State
                || t == AttributeTypeCode.Status
            )
                return ToOption(a) == ToOption(b);

            if (
                t == AttributeTypeCode.Lookup
                || t == AttributeTypeCode.Owner
                || t == AttributeTypeCode.Customer
            )
                return ToEntityRefId(a) == ToEntityRefId(b);

            if (t == AttributeTypeCode.Boolean)
                return ToBool(a) == ToBool(b);

            if (t == AttributeTypeCode.Money)
                return ToMoney(a) == ToMoney(b);

            if (t == AttributeTypeCode.DateTime)
                return ToUtc(a) == ToUtc(b);

            if (t == AttributeTypeCode.Integer)
                return ToInt(a) == ToInt(b);

            if (t == AttributeTypeCode.Decimal || t == AttributeTypeCode.Double)
                return ToDecimal(a) == ToDecimal(b);

            if (t == AttributeTypeCode.String || t == AttributeTypeCode.Memo)
                return string.Equals(
                    Convert.ToString(a),
                    Convert.ToString(b),
                    StringComparison.OrdinalIgnoreCase
                );

            // fallback
            return a.Equals(b);
        }

        private static object Unwrap(object o)
        {
            if (o is AliasedValue)
                return ((AliasedValue)o).Value;
            return o;
        }

        private static int ToOption(object o)
        {
            if (o is OptionSetValue)
                return ((OptionSetValue)o).Value;
            if (o is int)
                return (int)o;
            throw new InvalidPluginExecutionException(
                "Expected OptionSetValue/int but got " + o.GetType().Name
            );
        }

        private static Guid ToEntityRefId(object o)
        {
            if (o is EntityReference)
                return ((EntityReference)o).Id;
            if (o is Guid)
                return (Guid)o;
            throw new InvalidPluginExecutionException(
                "Expected EntityReference/Guid but got " + o.GetType().Name
            );
        }

        private static bool ToBool(object o)
        {
            if (o is bool)
                return (bool)o;
            throw new InvalidPluginExecutionException("Expected bool but got " + o.GetType().Name);
        }

        private static decimal ToMoney(object o)
        {
            if (o is Money)
                return ((Money)o).Value;
            if (o is decimal)
                return (decimal)o;
            throw new InvalidPluginExecutionException(
                "Expected Money/decimal but got " + o.GetType().Name
            );
        }

        private static DateTime ToUtc(object o)
        {
            if (!(o is DateTime))
                throw new InvalidPluginExecutionException(
                    "Expected DateTime but got " + o.GetType().Name
                );

            var dt = (DateTime)o;
            if (dt.Kind == DateTimeKind.Utc)
                return dt;
            if (dt.Kind == DateTimeKind.Local)
                return dt.ToUniversalTime();
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }

        private static int ToInt(object o)
        {
            if (o is int)
                return (int)o;
            throw new InvalidPluginExecutionException("Expected int but got " + o.GetType().Name);
        }

        private static decimal ToDecimal(object o)
        {
            if (o is decimal)
                return (decimal)o;
            if (o is double)
                return (decimal)(double)o;
            if (o is int)
                return (int)o;
            throw new InvalidPluginExecutionException(
                "Expected decimal-compatible but got " + o.GetType().Name
            );
        }
    }
}
