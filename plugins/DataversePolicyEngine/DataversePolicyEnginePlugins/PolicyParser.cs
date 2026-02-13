using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DataversePolicyEnginePlugins
{
    [DataContract]
    public class PolicyDefinition
    {
        [DataMember(Name = "logicalName")]
        public string LogicalName { get; set; }

        [DataMember(Name = "entityColumnName")]
        public string EntityColumnName { get; set; }

        [DataMember(Name = "attributeColumnName")]
        public string AttributeColumnName { get; set; }

        [DataMember(Name = "valueColumnName")]
        public string ValueColumnName { get; set; }

        [DataMember(Name = "valueColumnType")]
        public string ValueColumnType { get; set; }

        [DataMember(Name = "visibleColumnName")]
        public string VisibleColumnName { get; set; }

        [DataMember(Name = "allowedColumnName")]
        public string AllowedColumnName { get; set; }

        [DataMember(Name = "requiredColumnName")]
        public string RequiredColumnName { get; set; }
    }

    [DataContract]
    public class PolicyInput
    {
        [DataMember(Name = "attributeName")]
        public string AttributeName { get; set; }

        [DataMember(Name = "suppliedPolicy")]
        public PolicyDefinition SuppliedPolicy { get; set; }
    }

    public class PolicyParser
    {
        public static PolicyInput ParseAndValidate(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON string cannot be null or empty.");
            }

            PolicyInput input;
            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var ser = new DataContractJsonSerializer(typeof(PolicyInput));
                    input = (PolicyInput)ser.ReadObject(ms);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid JSON format.", ex);
            }

            // Validation
            if (string.IsNullOrWhiteSpace(input.AttributeName))
            {
                throw new ArgumentException("AttributeName is required and cannot be empty.");
            }

            if (input.SuppliedPolicy == null)
            {
                throw new ArgumentException("SuppliedPolicy is required.");
            }

            // Validate SuppliedPolicy fields
            if (string.IsNullOrWhiteSpace(input.SuppliedPolicy.LogicalName))
            {
                throw new ArgumentException("LogicalName in SuppliedPolicy is required.");
            }

            if (string.IsNullOrWhiteSpace(input.SuppliedPolicy.EntityColumnName))
            {
                throw new ArgumentException("EntityColumnName in SuppliedPolicy is required.");
            }

            if (string.IsNullOrWhiteSpace(input.SuppliedPolicy.AttributeColumnName))
            {
                throw new ArgumentException("AttributeColumnName in SuppliedPolicy is required.");
            }

            if (string.IsNullOrWhiteSpace(input.SuppliedPolicy.ValueColumnName))
            {
                throw new ArgumentException("ValueColumnName in SuppliedPolicy is required.");
            }

            if (string.IsNullOrWhiteSpace(input.SuppliedPolicy.ValueColumnType))
            {
                throw new ArgumentException("ValueColumnType in SuppliedPolicy is required.");
            }

            return input;
        }
    }
}
