using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using DataversePolicyEnginePlugins;
using System;

namespace DataversePolicyEnginePlugins.Tests
{
    [TestClass]
    public class ValueParserTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetValue_NullEntity_ThrowsArgumentNullException()
        {
            ValueParser.GetValue(null, "attr", ValueColumnType.String);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetValue_NullAttributeName_ThrowsArgumentException()
        {
            var entity = new Entity("test");
            ValueParser.GetValue(entity, null, ValueColumnType.String);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetValue_EmptyAttributeName_ThrowsArgumentException()
        {
            var entity = new Entity("test");
            ValueParser.GetValue(entity, "", ValueColumnType.String);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetValue_WhitespaceAttributeName_ThrowsArgumentException()
        {
            var entity = new Entity("test");
            ValueParser.GetValue(entity, "   ", ValueColumnType.String);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetValue_NullValueType_ThrowsArgumentException()
        {
            var entity = new Entity("test");
            ValueParser.GetValue(entity, "attr", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetValue_EmptyValueType_ThrowsArgumentException()
        {
            var entity = new Entity("test");
            ValueParser.GetValue(entity, "attr", "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetValue_UnsupportedValueType_ThrowsArgumentException()
        {
            var entity = new Entity("test");
            ValueParser.GetValue(entity, "attr", "UnsupportedType");
        }

        [TestMethod]
        public void GetValue_StringType_ReturnsStringValue()
        {
            var entity = new Entity("test");
            entity["stringAttr"] = "test value";
            var result = ValueParser.GetValue(entity, "stringAttr", ValueColumnType.String);
            Assert.AreEqual("test value", result);
        }

        [TestMethod]
        public void GetValue_IntType_ReturnsIntValue()
        {
            var entity = new Entity("test");
            entity["intAttr"] = 42;
            var result = ValueParser.GetValue(entity, "intAttr", ValueColumnType.Int);
            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void GetValue_DecimalType_ReturnsDecimalValue()
        {
            var entity = new Entity("test");
            entity["decimalAttr"] = 3.14m;
            var result = ValueParser.GetValue(entity, "decimalAttr", ValueColumnType.Decimal);
            Assert.AreEqual(3.14m, result);
        }

        [TestMethod]
        public void GetValue_BooleanType_ReturnsBooleanValue()
        {
            var entity = new Entity("test");
            entity["boolAttr"] = true;
            var result = ValueParser.GetValue(entity, "boolAttr", ValueColumnType.Boolean);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void GetValue_DateTimeType_ReturnsDateTimeValue()
        {
            var entity = new Entity("test");
            var date = new DateTime(2023, 1, 1);
            entity["dateAttr"] = date;
            var result = ValueParser.GetValue(entity, "dateAttr", ValueColumnType.DateTime);
            Assert.AreEqual(date, result);
        }

        [TestMethod]
        public void GetValue_EntityReferenceType_ReturnsEntityReferenceValue()
        {
            var entity = new Entity("test");
            var er = new EntityReference("contact", Guid.NewGuid());
            entity["refAttr"] = er;
            var result = ValueParser.GetValue(entity, "refAttr", ValueColumnType.EntityReference);
            Assert.AreEqual(er.Id, result);
        }

        [TestMethod]
        public void GetValue_OptionSetValueType_ReturnsOptionSetValue()
        {
            var entity = new Entity("test");
            var osv = new OptionSetValue(1);
            entity["optionAttr"] = osv;
            var result = ValueParser.GetValue(entity, "optionAttr", ValueColumnType.OptionSetValue);
            Assert.AreEqual(osv.Value, result);
        }

        [TestMethod]
        public void GetValue_MissingAttribute_ReturnsNull()
        {
            var entity = new Entity("test");
            var result = ValueParser.GetValue(entity, "missingAttr", ValueColumnType.String);
            Assert.IsNull(result);
        }
    }
}
