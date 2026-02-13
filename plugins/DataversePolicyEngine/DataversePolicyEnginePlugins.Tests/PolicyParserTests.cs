using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DataversePolicyEnginePlugins.Tests
{
    [TestClass]
    public class PolicyParserTests
    {
        [TestMethod]
        public void ParseAndValidate_ValidJson_ReturnsPolicyInput()
        {
            string json =
                @"{
                ""attributeName"": ""dpe_attribute"",
                ""suppliedPolicy"": {
                    ""logicalName"": ""dpe_policydefinition"",
                    ""entityColumnName"": ""dpe_entity"",
                    ""attributeColumnName"": ""dpe_attribute"",
                    ""valueColumnName"": ""dpe_lookupcomparison"",
                    ""valueColumnType"": ""EntityReference"",
                    ""visiblaeColumnName"": ""dpe_visible"",
                    ""allowedColumnName"": ""dpe_allowed"",
                    ""requiredColumnName"": ""dpe_required""
                }
            }";

            var result = PolicyParser.ParseAndValidate(json);

            Assert.IsNotNull(result);
            Assert.AreEqual("dpe_attribute", result.AttributeName);
            Assert.IsNotNull(result.SuppliedPolicy);
            Assert.AreEqual("dpe_policydefinition", result.SuppliedPolicy.LogicalName);
            Assert.AreEqual("dpe_entity", result.SuppliedPolicy.EntityColumnName);
            Assert.AreEqual("dpe_attribute", result.SuppliedPolicy.AttributeColumnName);
            Assert.AreEqual("dpe_lookupcomparison", result.SuppliedPolicy.ValueColumnName);
            Assert.AreEqual("EntityReference", result.SuppliedPolicy.ValueColumnType);
            Assert.AreEqual("dpe_visible", result.SuppliedPolicy.VisibleColumnName);
            Assert.AreEqual("dpe_allowed", result.SuppliedPolicy.AllowedColumnName);
            Assert.AreEqual("dpe_required", result.SuppliedPolicy.RequiredColumnName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_NullJson_ThrowsArgumentException()
        {
            PolicyParser.ParseAndValidate(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_EmptyJson_ThrowsArgumentException()
        {
            PolicyParser.ParseAndValidate("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_WhitespaceJson_ThrowsArgumentException()
        {
            PolicyParser.ParseAndValidate("   ");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_InvalidJson_ThrowsArgumentException()
        {
            PolicyParser.ParseAndValidate("invalid json");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_MissingAttributeName_ThrowsArgumentException()
        {
            string json =
                @"{
                ""suppliedPolicy"": {
                    ""logicalName"": ""dpe_policydefinition"",
                    ""entityColumnName"": ""dpe_entity"",
                    ""attributeColumnName"": ""dpe_attribute"",
                    ""valueColumnName"": ""dpe_lookupcomparison"",
                    ""valueColumnType"": ""EntityReference"",
                    ""visibleColumnName"": ""dpe_visible"",
                    ""allowedColumnName"": ""dpe_allowed"",
                    ""requiredColumnName"": ""dpe_required""
                }
            }";
            PolicyParser.ParseAndValidate(json);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_EmptyAttributeName_ThrowsArgumentException()
        {
            string json =
                @"{
                ""attributeName"": """",
                ""suppliedPolicy"": {
                    ""logicalName"": ""dpe_policydefinition"",
                    ""entityColumnName"": ""dpe_entity"",
                    ""attributeColumnName"": ""dpe_attribute"",
                    ""valueColumnName"": ""dpe_lookupcomparison"",
                    ""valueColumnType"": ""EntityReference"",
                    ""visibleColumnName"": ""dpe_visible"",
                    ""allowedColumnName"": ""dpe_allowed"",
                    ""requiredColumnName"": ""dpe_required""
                }
            }";
            PolicyParser.ParseAndValidate(json);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_NullSuppliedPolicy_ThrowsArgumentException()
        {
            string json =
                @"{
                ""attributeName"": ""dpe_attribute"",
                ""suppliedPolicy"": null
            }";
            PolicyParser.ParseAndValidate(json);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_MissingLogicalName_ThrowsArgumentException()
        {
            string json =
                @"{
                ""attributeName"": ""dpe_attribute"",
                ""suppliedPolicy"": {
                    ""entityColumnName"": ""dpe_entity"",
                    ""attributeColumnName"": ""dpe_attribute"",
                    ""valueColumnName"": ""dpe_lookupcomparison"",
                    ""valueColumnType"": ""EntityReference"",
                    ""visibleColumnName"": ""dpe_visible"",
                    ""allowedColumnName"": ""dpe_allowed"",
                    ""requiredColumnName"": ""dpe_required""
                }
            }";
            PolicyParser.ParseAndValidate(json);
        }

        // Add similar tests for other required fields in SuppliedPolicy
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_MissingEntityColumnName_ThrowsArgumentException()
        {
            string json =
                @"{
                ""attributeName"": ""dpe_attribute"",
                ""suppliedPolicy"": {
                    ""logicalName"": ""dpe_policydefinition"",
                    ""attributeColumnName"": ""dpe_attribute"",
                    ""valueColumnName"": ""dpe_lookupcomparison"",
                    ""valueColumnType"": ""EntityReference"",
                    ""visibleColumnName"": ""dpe_visible"",
                    ""allowedColumnName"": ""dpe_allowed"",
                    ""requiredColumnName"": ""dpe_required""
                }
            }";
            PolicyParser.ParseAndValidate(json);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_MissingAttributeColumnName_ThrowsArgumentException()
        {
            string json =
                @"{
                ""attributeName"": ""dpe_attribute"",
                ""suppliedPolicy"": {
                    ""logicalName"": ""dpe_policydefinition"",
                    ""entityColumnName"": ""dpe_entity"",
                    ""valueColumnName"": ""dpe_lookupcomparison"",
                    ""valueColumnType"": ""EntityReference"",
                    ""visibleColumnName"": ""dpe_visible"",
                    ""allowedColumnName"": ""dpe_allowed"",
                    ""requiredColumnName"": ""dpe_required""
                }
            }";
            PolicyParser.ParseAndValidate(json);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_MissingValueColumnName_ThrowsArgumentException()
        {
            string json =
                @"{
                ""attributeName"": ""dpe_attribute"",
                ""suppliedPolicy"": {
                    ""logicalName"": ""dpe_policydefinition"",
                    ""entityColumnName"": ""dpe_entity"",
                    ""attributeColumnName"": ""dpe_attribute"",
                    ""valueColumnType"": ""EntityReference"",
                    ""visibleColumnName"": ""dpe_visible"",
                    ""allowedColumnName"": ""dpe_allowed"",
                    ""requiredColumnName"": ""dpe_required""
                }
            }";
            PolicyParser.ParseAndValidate(json);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseAndValidate_MissingValueColumnType_ThrowsArgumentException()
        {
            string json =
                @"{
                ""attributeName"": ""dpe_attribute"",
                ""suppliedPolicy"": {
                    ""logicalName"": ""dpe_policydefinition"",
                    ""entityColumnName"": ""dpe_entity"",
                    ""attributeColumnName"": ""dpe_attribute"",
                    ""valueColumnName"": ""dpe_lookupcomparison"",
                    ""visibleColumnName"": ""dpe_visible"",
                    ""allowedColumnName"": ""dpe_allowed"",
                    ""requiredColumnName"": ""dpe_required""
                }
            }";
            PolicyParser.ParseAndValidate(json);
        }
    }
}
