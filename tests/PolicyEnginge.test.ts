import { XrmMockGenerator } from 'xrm-mock';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { defineConfig } from 'vitest/config';

import { PolicyEngine } from '../src/PolicyEngine/PolicyEngine';

export default defineConfig({
    test: {
        silent: false // Enable console output
    }
});

describe('PolicyEngine', () => {
    beforeEach(() => {
        XrmMockGenerator.initialise();
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
        await expect(() => PolicyEngine(undefined, 'name', {})).rejects.toThrow(
            /eventContext is required/
        );
    });

    it('throws when policy is missing', async () => {
        const eventContext = XrmMockGenerator.getEventContext();
        await expect(() =>
            PolicyEngine(eventContext, 'name', undefined)
        ).rejects.toThrow(/policy is required/);
    });

    it('throws when attribute is not found on the form', async () => {
        const eventContext = XrmMockGenerator.getEventContext();
        const policy = {
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'status',
            valueColumnType: 'String',
            visibilityColumnName: 'visibilitytest',
            allowedColumnName: 'allowedtest',
            requiredColumnName: 'requiredtest'
        };
        await expect(() =>
            PolicyEngine(eventContext, 'nonexistentattribute', policy)
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
            const eventContext = XrmMockGenerator.getEventContext();
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
            const policy = {
                logicalName: 'policydefinition',
                entityColumnName: 'entity',
                attributeColumnName: 'attribute',
                valueColumnName: 'status',
                valueColumnType: 'String',
                visibilityColumnName: 'visible',
                allowedColumnName: 'allowed',
                requiredColumnName: 'required'
            };

            vi.spyOn(Xrm.WebApi, 'retrieveMultipleRecords').mockResolvedValue(
                mockResponse
            );

            await PolicyEngine(eventContext, 'status', policy);

            const visibilityControl = Xrm.Page.getControl(
                'visibilitytest'
            ) as Xrm.Controls.StandardControl;
            const allowedControl = Xrm.Page.getControl(
                'allowedtest'
            ) as Xrm.Controls.StandardControl;
            const requiredAttribute = Xrm.Page.getAttribute(
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
            const eventContext = XrmMockGenerator.getEventContext();
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
            const policy = {
                logicalName: 'policydefinition',
                entityColumnName: 'entity',
                attributeColumnName: 'attribute',
                valueColumnName: 'status',
                valueColumnType: 'OptionSetValue',
                visibilityColumnName: 'visible',
                allowedColumnName: 'allowed',
                requiredColumnName: 'required'
            };

            vi.spyOn(Xrm.WebApi, 'retrieveMultipleRecords').mockResolvedValue(
                mockResponse
            );

            await PolicyEngine(eventContext, 'status', policy);

            const visibilityControl = Xrm.Page.getControl(
                'visibilitytest'
            ) as Xrm.Controls.StandardControl;
            const allowedControl = Xrm.Page.getControl(
                'allowedtest'
            ) as Xrm.Controls.StandardControl;
            const requiredAttribute = Xrm.Page.getAttribute(
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
            const eventContext = XrmMockGenerator.getEventContext();

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
            const policy = {
                logicalName: 'policydefinition',
                entityColumnName: 'entity',
                attributeColumnName: 'attribute',
                valueColumnName: 'status',
                valueColumnType: 'EntityReference',
                visibilityColumnName: 'visible',
                allowedColumnName: 'allowed',
                requiredColumnName: 'required'
            };

            vi.spyOn(Xrm.WebApi, 'retrieveMultipleRecords').mockResolvedValue(
                mockResponse
            );

            await PolicyEngine(eventContext, 'status', policy);

            const visibilityControl = Xrm.Page.getControl(
                'visibilitytest'
            ) as Xrm.Controls.StandardControl;
            const allowedControl = Xrm.Page.getControl(
                'allowedtest'
            ) as Xrm.Controls.StandardControl;
            const requiredAttribute = Xrm.Page.getAttribute(
                'requiredtest'
            ) as Xrm.Attributes.Attribute<string>;

            expect(visibilityControl.getVisible()).toBe(expected.visible);
            expect(allowedControl.getDisabled()).toBe(expected.disabled);
            expect(requiredAttribute.getRequiredLevel()).toBe(expected.requiredLevel);
        }
    );
});
