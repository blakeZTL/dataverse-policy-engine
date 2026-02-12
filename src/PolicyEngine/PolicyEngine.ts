import { GeneratePolicyDefinitionFetchXML } from '../Helpers/GeneratePolicyDefinitionFetchXML';
import { ValueParser } from '../Helpers/ValueParser';
import { Policy, PolicyDefinition } from '../types';

export async function PolicyEngine(
    eventContext: Xrm.Events.EventContext,
    attributeName: string,
    suppliedPolicy: object
): Promise<void> {
    if (!eventContext) {
        throw new TypeError('PolicyEngine: eventContext is required');
    }
    if (!suppliedPolicy) {
        throw new TypeError('PolicyEngine: policy is required');
    }
    let debugPrefix = 'PolicyEngine';

    let policy: Policy;
    try {
        policy = Policy.fromJSON(suppliedPolicy);
    } catch (error) {
        console.error(`${debugPrefix}: Error parsing policy`, error);
        throw error;
    }

    const formContext = eventContext.getFormContext();
    const entityName = formContext.data.entity.getEntityName();
    debugPrefix += ` [${entityName}-${attributeName}]`;
    console.debug(`${debugPrefix}: Evaluating policy for entity "${entityName}"`);

    const valueColumn = formContext.getAttribute(attributeName);
    if (!valueColumn) {
        throw new Error(
            `${debugPrefix}: Attribute "${attributeName}" not found on the form`
        );
    }
    if (valueColumn.getValue() === null) {
        console.debug(
            `${debugPrefix}: Attribute "${attributeName}" has a null value, skipping policy evaluation`
        );
        return;
    }
    console.debug(
        `${debugPrefix}: Retrieved value from attribute "${attributeName}" for policy evaluation`,
        valueColumn.getValue()
    );

    const value = ValueParser(
        valueColumn.getValue() as
            | string
            | number
            | boolean
            | Date
            | Xrm.LookupValue[]
            | Xrm.OptionSetValue,
        policy.valueColumnType
    );

    const fetchXML = GeneratePolicyDefinitionFetchXML(policy, entityName, value);
    console.debug(
        `${debugPrefix}: Generated FetchXML for policy definition retrieval`,
        fetchXML
    );

    const response = await Xrm.WebApi.retrieveMultipleRecords(
        policy.logicalName,
        `?fetchXml=${encodeURIComponent(fetchXML)}`
    );
    if (response.entities.length === 0) {
        console.debug(`${debugPrefix}: No matching policy definition found, skipping`);
        return;
    }
    console.debug(
        `${debugPrefix}: Retrieved ${response.entities.length} matching policy definitions from "${policy.logicalName}"`,
        response.entities
    );

    let policies = response.entities.map((entity) => {
        const pd: PolicyDefinition = {
            attribute: entity[policy.attributeColumnName],
            value: entity[policy.valueColumnName],
            visible: entity[policy.visibilityColumnName],
            allowed: entity[policy.allowedColumnName],
            required: entity[policy.requiredColumnName]
        };
        return pd;
    });
    console.debug(
        `${debugPrefix}: Mapped ${policies.length} policy definitions for application`,
        policies
    );

    for (const pd of policies) {
        const control = formContext.getControl(
            pd.attribute
        ) as Xrm.Controls.StandardControl;

        if (!control) {
            console.warn(
                `${debugPrefix}: Control for attribute "${pd.attribute}" not found on the form, skipping this policy definition`
            );
            continue;
        }

        const attribute = formContext.getAttribute(pd.attribute);
        if (!attribute) {
            console.warn(
                `${debugPrefix}: Attribute "${pd.attribute}" not found on the form, cannot set required level for "${pd.attribute}"`
            );
            continue;
        }
        console.debug(
            `${debugPrefix}: Applying policy to attribute "${pd.attribute}": Visible=${pd.visible}, Allowed=${pd.allowed}, Require=${pd.required}`
        );
        control.setVisible(pd.visible);

        control.setDisabled(!pd.allowed);

        attribute.setRequiredLevel(pd.required ? 'required' : 'none');
    }
}
