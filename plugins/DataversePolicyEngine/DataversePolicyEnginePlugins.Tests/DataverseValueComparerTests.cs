using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace DataversePolicyEnginePlugins.Tests
{
    [TestClass]
    public class DataverseValueComparerTests
    {
        [TestMethod]
        public void ValuesMatchConfigured_BothNull_ReturnsTrue()
        {
            var result = DataverseValueComparer.ValuesMatchConfigured(null, null, null, "String");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_OneNull_ReturnsFalse()
        {
            var result = DataverseValueComparer.ValuesMatchConfigured(
                null,
                "value",
                null,
                "String"
            );
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_OptionSetValue_Match()
        {
            var actual = new OptionSetValue(1);
            var expected = new OptionSetValue(1);
            var result = DataverseValueComparer.ValuesMatchConfigured(
                null,
                actual,
                expected,
                "OptionSetValue"
            );
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_OptionSetValue_NoMatch()
        {
            var actual = new OptionSetValue(1);
            var expected = new OptionSetValue(2);
            var result = DataverseValueComparer.ValuesMatchConfigured(
                null,
                actual,
                expected,
                "OptionSetValue"
            );
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_EntityReference_Match()
        {
            var guid = Guid.NewGuid();
            var actual = new EntityReference("contact", guid);
            var expected = new EntityReference("contact", guid);
            var result = DataverseValueComparer.ValuesMatchConfigured(
                null,
                actual,
                expected,
                "EntityReference"
            );
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_String_Match()
        {
            var result = DataverseValueComparer.ValuesMatchConfigured(
                null,
                "test",
                "TEST",
                "String"
            );
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_Boolean_Match()
        {
            var result = DataverseValueComparer.ValuesMatchConfigured(null, true, true, "Boolean");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_Money_Match()
        {
            var actual = new Money(100.00m);
            var expected = new Money(100.00m);
            var result = DataverseValueComparer.ValuesMatchConfigured(
                null,
                actual,
                expected,
                "Money"
            );
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_DateTime_Match()
        {
            var dt = DateTime.UtcNow;
            var result = DataverseValueComparer.ValuesMatchConfigured(null, dt, dt, "DateTime");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_Int_Match()
        {
            var result = DataverseValueComparer.ValuesMatchConfigured(null, 42, 42, "Int32");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_Decimal_Match()
        {
            var result = DataverseValueComparer.ValuesMatchConfigured(
                null,
                3.14m,
                3.14m,
                "Decimal"
            );
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesMatchConfigured_Default_Fallback()
        {
            var result = DataverseValueComparer.ValuesMatchConfigured(
                null,
                "test",
                "test",
                "UnknownType"
            );
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_BothNull_ReturnsTrue()
        {
            var meta = new StringAttributeMetadata();
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, null, null);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_OneNull_ReturnsFalse()
        {
            var meta = new StringAttributeMetadata();
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, "value", null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_Picklist_Match()
        {
            var meta = new PicklistAttributeMetadata();
            var actual = new OptionSetValue(1);
            var expected = new OptionSetValue(1);
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, actual, expected);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_Lookup_Match()
        {
            var meta = new LookupAttributeMetadata();
            var guid = Guid.NewGuid();
            var actual = new EntityReference("contact", guid);
            var expected = new EntityReference("contact", guid);
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, actual, expected);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_Boolean_Match()
        {
            var meta = new BooleanAttributeMetadata();
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, true, true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_Money_Match()
        {
            var meta = new MoneyAttributeMetadata();
            var actual = new Money(100.00m);
            var expected = new Money(100.00m);
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, actual, expected);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_DateTime_Match()
        {
            var meta = new DateTimeAttributeMetadata();
            var dt = DateTime.UtcNow;
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, dt, dt);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_Integer_Match()
        {
            var meta = new IntegerAttributeMetadata();
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, 42, 42);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_Decimal_Match()
        {
            var meta = new DecimalAttributeMetadata();
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, 3.14m, 3.14m);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_String_Match()
        {
            var meta = new StringAttributeMetadata();
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, "test", "TEST");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValuesEqualByMetadata_Fallback_Match()
        {
            var meta = new BigIntAttributeMetadata(); // Unsupported type
            var result = DataverseValueComparer.ValuesEqualByMetadata(meta, "test", "test");
            Assert.IsTrue(result);
        }

        // Test AliasedValue unwrapping
        [TestMethod]
        public void ValuesMatchConfigured_AliasedValue_Unwraps()
        {
            var actual = new AliasedValue("alias", "attr", "value");
            var expected = "value";
            var result = DataverseValueComparer.ValuesMatchConfigured(
                null,
                actual,
                expected,
                "String"
            );
            Assert.IsTrue(result);
        }

        // Exception tests for invalid types
        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ValuesMatchConfigured_InvalidOptionSetValue_Throws()
        {
            DataverseValueComparer.ValuesMatchConfigured(
                null,
                "invalid",
                new OptionSetValue(1),
                "OptionSetValue"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ValuesMatchConfigured_InvalidEntityReference_Throws()
        {
            DataverseValueComparer.ValuesMatchConfigured(
                null,
                "invalid",
                new EntityReference("contact", Guid.NewGuid()),
                "EntityReference"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ValuesMatchConfigured_InvalidBoolean_Throws()
        {
            DataverseValueComparer.ValuesMatchConfigured(null, "invalid", true, "Boolean");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ValuesMatchConfigured_InvalidMoney_Throws()
        {
            DataverseValueComparer.ValuesMatchConfigured(null, "invalid", new Money(100), "Money");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ValuesMatchConfigured_InvalidDateTime_Throws()
        {
            DataverseValueComparer.ValuesMatchConfigured(
                null,
                "invalid",
                DateTime.UtcNow,
                "DateTime"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ValuesMatchConfigured_InvalidInt_Throws()
        {
            DataverseValueComparer.ValuesMatchConfigured(null, "invalid", 42, "Int32");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ValuesMatchConfigured_InvalidDecimal_Throws()
        {
            DataverseValueComparer.ValuesMatchConfigured(null, "invalid", 3.14m, "Decimal");
        }
    }
}
