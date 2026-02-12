using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace DataversePolicyEnginePlugins
{
    public class HandleEntityValidation : PluginBase
    {
        private readonly PolicyInput _policy;

        public HandleEntityValidation(string unsecureConfig, string _)
            : base(typeof(HandleEntityValidation))
        {
            _policy = PolicyParser.ParseAndValidate(unsecureConfig);
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            var context = localPluginContext.PluginExecutionContext;
            var sysService = localPluginContext.SystemUserService;
            var tracer = localPluginContext.TracingService;

            if (context.Stage != 10)
            {
                tracer.Trace(
                    "Plugin is registered on stage {0}, but it should be registered on stage 10. Exiting plugin execution.",
                    context.Stage
                );
                return;
            }

            if (!context.InputParameters.TryGetValue("Target", out Entity target))
            {
                throw new InvalidPluginExecutionException(
                    $"{PluginClassName}: Target not found in input parameters"
                );
            }

            var policyQuery = new QueryExpression(_policy.SuppliedPolicy.LogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0),
                        new ConditionExpression(
                            _policy.SuppliedPolicy.EntityColumnName,
                            ConditionOperator.Equal,
                            context.PrimaryEntityName
                        )
                    }
                }
            };

            var policies = sysService.RetrieveMultiple(policyQuery).Entities;
            if (policies.Count == 0)
            {
                tracer.Trace(
                    "No active policy found for entity {0}. Exiting plugin execution.",
                    context.PrimaryEntityName
                );
                return;
            }

            var attributeValue = ValueParser.GetValue(
                target,
                _policy.AttributeName,
                _policy.SuppliedPolicy.ValueColumnType
            );
            var applicablePolicies = policies
                .Where(policy =>
                {
                    var policyValue = ValueParser.GetValue(
                        policy,
                        _policy.SuppliedPolicy.ValueColumnName,
                        _policy.SuppliedPolicy.ValueColumnType
                    );
                    return policyValue == attributeValue;
                })
                .ToList();

            foreach (var policy in applicablePolicies)
            {
                var policyAttribute = policy.GetAttributeValue<string>(
                    _policy.SuppliedPolicy.AttributeColumnName
                );
                if (string.IsNullOrWhiteSpace(policyAttribute))
                {
                    tracer.Trace(
                        "Policy {0} has an empty attribute column. Skipping this policy.",
                        policy.Id
                    );
                    continue;
                }
            }
        }
    }
}
