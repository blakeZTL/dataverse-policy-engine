import { Policy } from '../types';

export function GeneratePolicyDefinitionFetchXML(
    policy: Policy,
    entityName: string,
    value: string | number | boolean | Date | Xrm.LookupValue | Xrm.OptionSetValue
): string {
    if (!policy) {
        throw new TypeError(
            'GeneratePolicyDefinitionFetchXML: policyDefinition is required'
        );
    }
    if (!entityName) {
        throw new TypeError('GeneratePolicyDefinitionFetchXML: entityName is required');
    }
    if (value === null || value === undefined) {
        throw new TypeError('GeneratePolicyDefinitionFetchXML: value is required');
    }
    console.debug(
        'GeneratePolicyDefinitionFetchXML: Generating FetchXML with the following parameters',
        {
            policy,
            entityName,
            value
        }
    );
    let fetchXML = `<fetch >
                        <entity name="${policy.logicalName}">
                            <attribute name="${policy.entityColumnName}" />
                            <attribute name="${policy.attributeColumnName}" />
                            <attribute name="${policy.valueColumnName}" />
                            <attribute name="${policy.shouldHideColumnName}" />
                            <attribute name="${policy.shouldLockColumnName}" />
                            <attribute name="${policy.shouldRequireColumnName}" />
                            <filter>
                                <condition attribute="${policy.entityColumnName}" operator="eq" value="${entityName}" />`;
    console.debug(
        'GeneratePolicyDefinitionFetchXML: Parsing value for FetchXML condition based on valueColumnType',
        policy.valueColumnType
    );

    switch (policy.valueColumnType) {
        case 'String':
            fetchXML += `<condition attribute="${policy.valueColumnName}" operator="eq" value="${value}" />`;
            break;
        case 'Int':
            fetchXML += `<condition attribute="${policy.valueColumnName}" operator="eq" value="${value}" />`;
            break;
        case 'Boolean':
            fetchXML += `<condition attribute="${policy.valueColumnName}" operator="eq" value="${value}" />`;
            break;
        case 'DateTime':
            fetchXML += `<condition attribute="${policy.valueColumnName}" operator="eq" value="${(value as Date).toISOString()}" />`;
            break;
        case 'Decimal':
            fetchXML += `<condition attribute="${policy.valueColumnName}" operator="eq" value="${value}" />`;
            break;
        case 'OptionSetValue':
            fetchXML += `<condition attribute="${policy.valueColumnName}" operator="eq" value="${(value as Xrm.OptionSetValue).value}" />`;
            break;
        case 'EntityReference':
            fetchXML += `<condition attribute="${policy.valueColumnName}" operator="eq" value="${(value as Xrm.LookupValue).id}" />`;
            break;
        default:
            throw new Error(
                `GeneratePolicyDefinitionFetchXML: Unsupported valueColumnType "${policy.valueColumnType}"`
            );
    }
    fetchXML += `</filter>
                </entity>
            </fetch>`;
    return fetchXML;
}
