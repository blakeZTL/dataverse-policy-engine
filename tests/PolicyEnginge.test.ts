import { XrmMockGenerator } from 'xrm-mock';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { defineConfig } from 'vitest/config';

import { PolicyEngine } from '../src/PolicyEngine';

export default defineConfig({
    test: {
        silent: false // Enable console output
    }
});

const policy = {
    logicalName: 'policydefinition',
    entityColumnName: 'entity',
    attributeColumnName: 'attribute',
    valueColumnName: 'status',
    valueColumnType: 'String',
    visibleColumnName: 'visible',
    allowedColumnName: 'allowed',
    requiredColumnName: 'required'
};
const policyConfig = {
    attributeName: 'status',
    suppliedPolicy: policy
};
let eventContext: Xrm.Events.EventContext;
let formContext: Xrm.FormContext;

describe('PolicyEngine', () => {
    beforeEach(() => {
        XrmMockGenerator.initialise();
        eventContext = XrmMockGenerator.getEventContext();
        formContext = eventContext.getFormContext();
        const nameAttribute = XrmMockGenerator.Attribute.createString(
            'name',
            'Test Name'
        );
        const visibilityAttribute = XrmMockGenerator.Attribute.createString(
            'visibilitytest',
            'Visibility Test'
        );
        const allowedAttribute = XrmMockGenerator.Attribute.createString(
            'allowedtest',
            'Allowed Test'
        );
        const requiredAttribute = XrmMockGenerator.Attribute.createString(
            'requiredtest',
            'Required Test'
        );
        XrmMockGenerator.Control.createString(nameAttribute, 'name', true, false);
        XrmMockGenerator.Control.createString(
            visibilityAttribute,
            'visibilitytest',
            true,
            false
        );
        XrmMockGenerator.Control.createString(
            allowedAttribute,
            'allowedtest',
            true,
            false
        );
        XrmMockGenerator.Control.createString(
            requiredAttribute,
            'requiredtest',
            true,
            false
        );
    });

    it('throws when eventContext is missing', async () => {
        await expect(() =>
            PolicyEngine(undefined, { attributeName: 'name', suppliedPolicy: policy })
        ).rejects.toThrow(/eventContext is required/);
    });

    it('throws when policy is missing', async () => {
        const eventContext = XrmMockGenerator.getEventContext();
        await expect(() => PolicyEngine(eventContext, undefined)).rejects.toThrow(
            /policy is required/
        );
    });

    it('throws when supplied policy is invalid', async () => {
        const eventContext = XrmMockGenerator.getEventContext();
        const invalidPolicy = {
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'status',
            valueColumnType: 'InvalidType'
        };
        const policyConfig = {
            attributeName: 'name',
            suppliedPolicy: invalidPolicy
        };
        await expect(() =>
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            PolicyEngine(eventContext, policyConfig as any)
        ).rejects.toThrow(
            'Policy.fromJSON: "valueColumnType" must be one of: OptionSetValue, EntityReference, String, Int, Decimal, Boolean, DateTime'
        );
    });

    it('throws when attribute is not found on the form', async () => {
        const eventContext = XrmMockGenerator.getEventContext();

        await expect(() =>
            PolicyEngine(eventContext, {
                attributeName: 'nonexistentattribute',
                suppliedPolicy: policy
            })
        ).rejects.toThrow(/Attribute "nonexistentattribute" not found on the form/);
    });

    it.each([
        {
            name: 'sets visibility to true, allowed to false, required to none',
            mockResponse: {
                entities: [
                    {
                        attribute: 'visibilitytest',
                        status: 'allow',
                        visible: true,
                        allowed: true,
                        required: false
                    },
                    {
                        attribute: 'allowedtest',
                        status: 'allow',
                        visible: true,
                        allowed: false,
                        required: false
                    },
                    {
                        attribute: 'requiredtest',
                        status: 'allow',
                        visible: true,
                        allowed: true,
                        required: false
                    }
                ],
                nextLink: ''
            },
            expected: {
                visible: true,
                disabled: true,
                requiredLevel: 'none'
            }
        },
        {
            name: 'sets visibility to false, allowed to true, required to required',
            mockResponse: {
                entities: [
                    {
                        attribute: 'visibilitytest',
                        status: 'deny',
                        visible: false,
                        allowed: true,
                        required: true
                    },
                    {
                        attribute: 'allowedtest',
                        status: 'deny',
                        visible: false,
                        allowed: true,
                        required: true
                    },
                    {
                        attribute: 'requiredtest',
                        status: 'deny',
                        visible: false,
                        allowed: true,
                        required: true
                    }
                ],
                nextLink: ''
            },
            expected: {
                visible: false,
                disabled: false,
                requiredLevel: 'required'
            }
        }
    ])(
        'String Comparison: applies policy correctly when a matching policy definition is found: $name',
        async ({ mockResponse, expected }) => {
            const statusAttribute = XrmMockGenerator.Attribute.createString(
                'status',
                'allow'
            );
            XrmMockGenerator.Control.createString(
                statusAttribute,
                'status',
                true,
                false
            );
            vi.spyOn(Xrm.WebApi, 'retrieveMultipleRecords').mockResolvedValue(
                mockResponse
            );

            await PolicyEngine(eventContext, policyConfig);

            const visibilityControl = formContext.getControl(
                'visibilitytest'
            ) as Xrm.Controls.StandardControl;
            const allowedControl = formContext.getControl(
                'allowedtest'
            ) as Xrm.Controls.StandardControl;
            const requiredAttribute = formContext.getAttribute(
                'requiredtest'
            ) as Xrm.Attributes.Attribute<string>;

            expect(visibilityControl.getVisible()).toBe(expected.visible);
            expect(allowedControl.getDisabled()).toBe(expected.disabled);
            expect(requiredAttribute.getRequiredLevel()).toBe(expected.requiredLevel);
        }
    );

    it.each([
        {
            name: 'sets visibility to true, allowed to false, required to none',
            mockResponse: {
                entities: [
                    {
                        attribute: 'visibilitytest',
                        status: 0,
                        visible: true,
                        allowed: true,
                        required: false
                    },
                    {
                        attribute: 'allowedtest',
                        status: 0,
                        visible: true,
                        allowed: false,
                        required: false
                    },
                    {
                        attribute: 'requiredtest',
                        status: 0,
                        visible: true,
                        allowed: true,
                        required: false
                    }
                ],
                nextLink: ''
            },
            expected: {
                visible: true,
                disabled: true,
                requiredLevel: 'none'
            }
        },
        {
            name: 'sets visibility to false, allowed to true, required to required',
            mockResponse: {
                entities: [
                    {
                        attribute: 'visibilitytest',
                        status: 1,
                        visible: false,
                        allowed: true,
                        required: true
                    },
                    {
                        attribute: 'allowedtest',
                        status: 1,
                        visible: false,
                        allowed: true,
                        required: true
                    },
                    {
                        attribute: 'requiredtest',
                        status: 1,
                        visible: false,
                        allowed: true,
                        required: true
                    }
                ],
                nextLink: ''
            },
            expected: {
                visible: false,
                disabled: false,
                requiredLevel: 'required'
            }
        }
    ])(
        'Choice Comparison: applies policy correctly when a matching policy definition is found: $name',
        async ({ mockResponse, expected }) => {
            const statusAttribute = XrmMockGenerator.Attribute.createOptionSet(
                'status',
                0
            );
            XrmMockGenerator.Control.createOptionSet(
                statusAttribute,
                'status',
                true,
                false
            );
            policy.valueColumnType = 'OptionSetValue';

            vi.spyOn(Xrm.WebApi, 'retrieveMultipleRecords').mockResolvedValue(
                mockResponse
            );

            await PolicyEngine(eventContext, policyConfig);

            const visibilityControl = formContext.getControl(
                'visibilitytest'
            ) as Xrm.Controls.StandardControl;
            const allowedControl = formContext.getControl(
                'allowedtest'
            ) as Xrm.Controls.StandardControl;
            const requiredAttribute = formContext.getAttribute(
                'requiredtest'
            ) as Xrm.Attributes.Attribute<string>;

            expect(visibilityControl.getVisible()).toBe(expected.visible);
            expect(allowedControl.getDisabled()).toBe(expected.disabled);
            expect(requiredAttribute.getRequiredLevel()).toBe(expected.requiredLevel);
        }
    );

    const lookupValue: Xrm.LookupValue = {
        id: '00000000-0000-0000-0000-000000000001',
        name: 'Allow',
        entityType: 'status'
    };
    it.each([
        {
            name: 'sets visibility to true, allowed to false, required to none',
            mockResponse: {
                entities: [
                    {
                        attribute: 'visibilitytest',
                        status: lookupValue,
                        visible: true,
                        allowed: true,
                        required: false
                    },
                    {
                        attribute: 'allowedtest',
                        status: lookupValue,
                        visible: true,
                        allowed: false,
                        required: false
                    },
                    {
                        attribute: 'requiredtest',
                        status: lookupValue,
                        visible: true,
                        allowed: true,
                        required: false
                    }
                ],
                nextLink: ''
            },
            expected: {
                visible: true,
                disabled: true,
                requiredLevel: 'none'
            }
        },
        {
            name: 'sets visibility to false, allowed to true, required to required',
            mockResponse: {
                entities: [
                    {
                        attribute: 'visibilitytest',
                        status: 1,
                        visible: false,
                        allowed: true,
                        required: true
                    },
                    {
                        attribute: 'allowedtest',
                        status: 1,
                        visible: false,
                        allowed: true,
                        required: true
                    },
                    {
                        attribute: 'requiredtest',
                        status: 1,
                        visible: false,
                        allowed: true,
                        required: true
                    }
                ],
                nextLink: ''
            },
            expected: {
                visible: false,
                disabled: false,
                requiredLevel: 'required'
            }
        }
    ])(
        'Lookup Comparison: applies policy correctly when a matching policy definition is found: $name',
        async ({ mockResponse, expected }) => {
            const statusAttribute = XrmMockGenerator.Attribute.createLookup(
                'status',
                lookupValue
            );
            XrmMockGenerator.Control.createLookup(
                statusAttribute,
                'status',
                true,
                false
            );
            policy.valueColumnType = 'EntityReference';

            vi.spyOn(Xrm.WebApi, 'retrieveMultipleRecords').mockResolvedValue(
                mockResponse
            );

            await PolicyEngine(eventContext, policyConfig);

            const visibilityControl = formContext.getControl(
                'visibilitytest'
            ) as Xrm.Controls.StandardControl;
            const allowedControl = formContext.getControl(
                'allowedtest'
            ) as Xrm.Controls.StandardControl;
            const requiredAttribute = formContext.getAttribute(
                'requiredtest'
            ) as Xrm.Attributes.Attribute<string>;

            expect(visibilityControl.getVisible()).toBe(expected.visible);
            expect(allowedControl.getDisabled()).toBe(expected.disabled);
            expect(requiredAttribute.getRequiredLevel()).toBe(expected.requiredLevel);
        }
    );
});
