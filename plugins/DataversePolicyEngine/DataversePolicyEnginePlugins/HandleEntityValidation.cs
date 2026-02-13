using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
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
            var ctx = localPluginContext.PluginExecutionContext;
            var svc = localPluginContext.SystemUserService;
            var trace = localPluginContext.TracingService;

            if (ctx.Stage != 10)
                return; // PreOperation

            if (!ctx.InputParameters.Contains("Target"))
                throw new InvalidPluginExecutionException(PluginClassName + ": Target not found.");

            var target = (Entity)ctx.InputParameters["Target"];
            var entityName = ctx.PrimaryEntityName;
            var message = (ctx.MessageName ?? string.Empty).ToLowerInvariant();

            var isCreate = message == "create";
            var isUpdate = message == "update";

            if (!isCreate && !isUpdate)
                return;

            Entity preImage = null;
            if (isUpdate && ctx.PreEntityImages != null && ctx.PreEntityImages.Contains("PreImage"))
                preImage = ctx.PreEntityImages["PreImage"];

            // 1) Retrieve active policies for this entity
            var policyRows = RetrievePolicies(svc, entityName);
            if (policyRows.Count == 0)
                return;

            trace.Trace($"Policy count: {policyRows.Count}");
            // 2) Evaluate each policy row and enforce
            var metaCache = new Dictionary<string, AttributeMetadata>(
                StringComparer.OrdinalIgnoreCase
            );
            var entityValue = EntityValueResolver.GetEffectiveValue(
                target,
                preImage,
                _policy.AttributeName
            );
            trace.Trace($"Entity comparison value: {entityValue}");

            var entityValueMeta = MetadataCache.GetAttributeMetadata(
                svc,
                metaCache,
                entityName,
                _policy.AttributeName
            );

            var applicablePolicies = policyRows.Where(
                (p) =>
                {
                    var policyValue = EntityValueResolver.GetEffectiveValue(
                        p,
                        p,
                        _policy.SuppliedPolicy.ValueColumnName
                    );

                    return DataverseValueComparer.ValuesEqualByMetadata(
                        entityValueMeta,
                        entityValue,
                        policyValue
                    );
                }
            );

            foreach (var policyRow in applicablePolicies)
            {
                var attrName = policyRow.GetAttributeValue<string>(
                    _policy.SuppliedPolicy.AttributeColumnName
                );
                trace.Trace($"Validating policy for {attrName}");

                if (string.IsNullOrWhiteSpace(attrName))
                    continue;

                var effectiveValue = EntityValueResolver.GetEffectiveValue(
                    target,
                    preImage,
                    attrName
                );
                trace.Trace(
                    $"Effective value: {effectiveValue}, type: {effectiveValue?.GetType()}"
                );

                var attrMeta = MetadataCache.GetAttributeMetadata(
                    svc,
                    metaCache,
                    entityName,
                    attrName
                );

                var required =
                    policyRow.GetAttributeValue<bool?>(_policy.SuppliedPolicy.RequiredColumnName)
                    ?? false;
                trace.Trace($"Required: {required}");
                var allowed =
                    policyRow.GetAttributeValue<bool?>(_policy.SuppliedPolicy.AllowedColumnName)
                    ?? true;
                trace.Trace($"Allowed: {allowed}");

                // REQUIRED: effective value cannot be null
                if (required && IsNullValue(effectiveValue))
                {
                    throw new InvalidPluginExecutionException(
                        string.Format("'{0}' is required by policy and cannot be blank.", attrName)
                    );
                }

                // NOT ALLOWED: cannot set/change
                if (!allowed)
                {
                    trace.Trace($"{attrName} not allowed");
                    if (isCreate)
                    {
                        // If user attempted to set it on create, block (null is okay)
                        if (target.Attributes.Contains(attrName) && !IsNullValue(target[attrName]))
                        {
                            throw new InvalidPluginExecutionException(
                                string.Format("'{0}' is not allowed to be set by policy.", attrName)
                            );
                        }
                    }
                    else if (isUpdate)
                    {
                        // Only block if they attempted to change it
                        if (target.Attributes.Contains(attrName))
                        {
                            var newValue = target[attrName];
                            trace.Trace($"New value: {newValue}");
                            var oldValue =
                                preImage != null && preImage.Attributes.Contains(attrName)
                                    ? preImage[attrName]
                                    : null;
                            trace.Trace($"Old value: {oldValue}");

                            if (
                                !DataverseValueComparer.ValuesEqualByMetadata(
                                    attrMeta,
                                    newValue,
                                    oldValue
                                )
                            )
                            {
                                throw new InvalidPluginExecutionException(
                                    string.Format(
                                        "'{0}' is not allowed to be changed by policy.",
                                        attrName
                                    )
                                );
                            }
                        }
                    }
                }
            }
        }

        private List<Entity> RetrievePolicies(IOrganizationService svc, string entityLogicalName)
        {
            var qe = new QueryExpression(_policy.SuppliedPolicy.LogicalName)
            {
                ColumnSet = new ColumnSet(
                    _policy.SuppliedPolicy.AttributeColumnName,
                    _policy.SuppliedPolicy.ValueColumnName,
                    _policy.SuppliedPolicy.RequiredColumnName,
                    _policy.SuppliedPolicy.AllowedColumnName,
                    _policy.SuppliedPolicy.EntityColumnName
                )
            };

            qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            qe.Criteria.AddCondition(
                _policy.SuppliedPolicy.EntityColumnName,
                ConditionOperator.Equal,
                entityLogicalName
            );

            return svc.RetrieveMultiple(qe).Entities.ToList();
        }

        private static bool IsNullValue(object value)
        {
            if (value == null)
                return true;

            // OptionSetValue can't really be "null" if present, but treat missing as null before this point
            if (value is string)
                return string.IsNullOrWhiteSpace((string)value);

            return false;
        }
    }

    internal static class EntityValueResolver
    {
        public static object GetEffectiveValue(Entity target, Entity preImage, string attribute)
        {
            if (target != null && target.Attributes.Contains(attribute))
                return target[attribute];

            if (preImage != null && preImage.Attributes.Contains(attribute))
                return preImage[attribute];

            return null;
        }
    }

    internal static class MetadataCache
    {
        public static AttributeMetadata GetAttributeMetadata(
            IOrganizationService svc,
            Dictionary<string, AttributeMetadata> cache,
            string entity,
            string attribute
        )
        {
            var key = entity + ":" + attribute;

            AttributeMetadata meta;
            if (cache.TryGetValue(key, out meta))
                return meta;

            var req = new RetrieveAttributeRequest
            {
                EntityLogicalName = entity,
                LogicalName = attribute,
                RetrieveAsIfPublished = true
            };

            var resp = (RetrieveAttributeResponse)svc.Execute(req);
            meta = resp.AttributeMetadata;
            cache[key] = meta;
            return meta;
        }
    }
}
