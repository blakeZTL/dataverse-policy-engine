using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System.Collections.Generic;

namespace DataversePolicyEnginePlugins.Tests
{
    [TestClass]
    public class HandleEntityValidationTests
    {
        private const string UnsecureConfig =
            @"{
                ""attributeName"": ""dpe_attribute"",
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

        private class TestHandleEntityValidation : HandleEntityValidation
        {
            public TestHandleEntityValidation(string unsecureConfig, string secureConfig)
                : base(unsecureConfig, secureConfig) { }

            public new void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
            {
                base.ExecuteCdsPlugin(localPluginContext);
            }
        }

        [TestMethod]
        public void ExecuteCdsPlugin_StageNot10_ReturnsWithoutExecution()
        {
            // Arrange
            var plugin = new TestHandleEntityValidation(UnsecureConfig, null);
            var contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(c => c.Stage).Returns(20);
            var localContext = new Mock<ILocalPluginContext>();
            localContext.Setup(l => l.PluginExecutionContext).Returns(contextMock.Object);

            // Act
            plugin.ExecuteCdsPlugin(localContext.Object);

            // Assert - No exception
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecuteCdsPlugin_NoTarget_ThrowsException()
        {
            // Arrange
            var plugin = new TestHandleEntityValidation(UnsecureConfig, null);
            var contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(c => c.Stage).Returns(10);
            contextMock.Setup(c => c.InputParameters).Returns(new ParameterCollection());
            var localContext = new Mock<ILocalPluginContext>();
            localContext.Setup(l => l.PluginExecutionContext).Returns(contextMock.Object);

            // Act
            plugin.ExecuteCdsPlugin(localContext.Object);
        }

        [TestMethod]
        public void ExecuteCdsPlugin_MessageNotCreateOrUpdate_Returns()
        {
            // Arrange
            var plugin = new TestHandleEntityValidation(UnsecureConfig, null);
            var target = new Entity("testentity");
            var contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(c => c.Stage).Returns(10);
            contextMock
                .Setup(c => c.InputParameters)
                .Returns(new ParameterCollection { { "Target", target } });
            contextMock.Setup(c => c.PrimaryEntityName).Returns("testentity");
            contextMock.Setup(c => c.MessageName).Returns("delete");
            var localContext = new Mock<ILocalPluginContext>();
            localContext.Setup(l => l.PluginExecutionContext).Returns(contextMock.Object);

            // Act
            plugin.ExecuteCdsPlugin(localContext.Object);

            // Assert
        }

        [TestMethod]
        public void ExecuteCdsPlugin_NoPolicies_Returns()
        {
            // Arrange
            var plugin = new TestHandleEntityValidation(UnsecureConfig, null);
            var target = new Entity("testentity");
            var contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(c => c.Stage).Returns(10);
            contextMock
                .Setup(c => c.InputParameters)
                .Returns(new ParameterCollection { { "Target", target } });
            contextMock.Setup(c => c.PrimaryEntityName).Returns("testentity");
            contextMock.Setup(c => c.MessageName).Returns("create");
            var serviceMock = new Mock<IOrganizationService>();
            serviceMock
                .Setup(s => s.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection(new List<Entity>()));
            var localContext = new Mock<ILocalPluginContext>();
            localContext.Setup(l => l.PluginExecutionContext).Returns(contextMock.Object);
            localContext.Setup(l => l.SystemUserService).Returns(serviceMock.Object);

            // Act
            plugin.ExecuteCdsPlugin(localContext.Object);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecuteCdsPlugin_RequiredAndNull_ThrowsException()
        {
            // Arrange
            var plugin = new TestHandleEntityValidation(UnsecureConfig, null);
            var target = new Entity("testentity");
            var contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(c => c.Stage).Returns(10);
            contextMock
                .Setup(c => c.InputParameters)
                .Returns(new ParameterCollection { { "Target", target } });
            contextMock.Setup(c => c.PrimaryEntityName).Returns("testentity");
            contextMock.Setup(c => c.MessageName).Returns("create");
            var policy = new Entity("dpe_policydefinition")
            {
                ["dpe_attribute"] = "testattr",
                ["dpe_required"] = true,
                ["dpe_allowed"] = true,
                ["dpe_entity"] = "testentity"
            };
            var serviceMock = new Mock<IOrganizationService>();
            serviceMock
                .Setup(s => s.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection(new List<Entity> { policy }));
            serviceMock
                .Setup(s => s.Execute(It.IsAny<RetrieveAttributeRequest>()))
                .Returns(CreateRetrieveAttributeResponse(new StringAttributeMetadata()));
            var localContext = new Mock<ILocalPluginContext>();
            localContext.Setup(l => l.PluginExecutionContext).Returns(contextMock.Object);
            localContext.Setup(l => l.SystemUserService).Returns(serviceMock.Object);

            // Act
            plugin.ExecuteCdsPlugin(localContext.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecuteCdsPlugin_NotAllowedOnCreate_ThrowsException()
        {
            // Arrange
            var plugin = new TestHandleEntityValidation(UnsecureConfig, null);
            var target = new Entity("testentity") { ["testattr"] = "value" };
            var contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(c => c.Stage).Returns(10);
            contextMock
                .Setup(c => c.InputParameters)
                .Returns(new ParameterCollection { { "Target", target } });
            contextMock.Setup(c => c.PrimaryEntityName).Returns("testentity");
            contextMock.Setup(c => c.MessageName).Returns("create");
            var policy = new Entity("dpe_policydefinition")
            {
                ["dpe_attribute"] = "testattr",
                ["dpe_required"] = false,
                ["dpe_allowed"] = false,
                ["dpe_entity"] = "testentity"
            };
            var serviceMock = new Mock<IOrganizationService>();
            serviceMock
                .Setup(s => s.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection(new List<Entity> { policy }));
            serviceMock
                .Setup(s => s.Execute(It.IsAny<RetrieveAttributeRequest>()))
                .Returns(CreateRetrieveAttributeResponse(new StringAttributeMetadata()));
            var localContext = new Mock<ILocalPluginContext>();
            localContext.Setup(l => l.PluginExecutionContext).Returns(contextMock.Object);
            localContext.Setup(l => l.SystemUserService).Returns(serviceMock.Object);

            // Act
            plugin.ExecuteCdsPlugin(localContext.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ExecuteCdsPlugin_NotAllowedOnUpdate_ThrowsException()
        {
            // Arrange
            var plugin = new TestHandleEntityValidation(UnsecureConfig, null);
            var target = new Entity("testentity") { ["testattr"] = "newvalue" };
            var preImage = new Entity("testentity") { ["testattr"] = "oldvalue" };
            var contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(c => c.Stage).Returns(10);
            contextMock
                .Setup(c => c.InputParameters)
                .Returns(new ParameterCollection { { "Target", target } });
            contextMock.Setup(c => c.PrimaryEntityName).Returns("testentity");
            contextMock.Setup(c => c.MessageName).Returns("update");
            contextMock
                .Setup(c => c.PreEntityImages)
                .Returns(new EntityImageCollection { { "PreImage", preImage } });
            var policy = new Entity("dpe_policydefinition")
            {
                ["dpe_attribute"] = "testattr",
                ["dpe_required"] = false,
                ["dpe_allowed"] = false,
                ["dpe_entity"] = "testentity"
            };
            var serviceMock = new Mock<IOrganizationService>();
            serviceMock
                .Setup(s => s.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection(new List<Entity> { policy }));
            serviceMock
                .Setup(s => s.Execute(It.IsAny<RetrieveAttributeRequest>()))
                .Returns(CreateRetrieveAttributeResponse(new StringAttributeMetadata()));
            var localContext = new Mock<ILocalPluginContext>();
            localContext.Setup(l => l.PluginExecutionContext).Returns(contextMock.Object);
            localContext.Setup(l => l.SystemUserService).Returns(serviceMock.Object);

            // Act
            plugin.ExecuteCdsPlugin(localContext.Object);
        }

        private RetrieveAttributeResponse CreateRetrieveAttributeResponse(
            AttributeMetadata metadata
        )
        {
            var responseMock = new Mock<RetrieveAttributeResponse>();
            responseMock.Setup(r => r.AttributeMetadata).Returns(metadata);
            return responseMock.Object;
        }
    }
}
