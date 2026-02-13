import { describe, it, expect } from 'vitest';

import { GeneratePolicyDefinitionFetchXML } from '../src/Helpers/GeneratePolicyDefinitionFetchXML';
import { Policy, ValueColumnType } from '../src/types';

describe('GeneratePolicyDefinitionFetchXML', () => {
    it('throws when Policy is missing', () => {
        expect(() =>
            GeneratePolicyDefinitionFetchXML(undefined, 'account', 'value')
        ).toThrow(TypeError);
    });

    it('throws when entityName is invalid', () => {
        const pd = new Policy({
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'status',
            valueColumnType: ValueColumnType.String,
            visibleColumnName: 'shouldhide',
            allowedColumnName: 'shouldlock',
            requiredColumnName: 'shouldrequire'
        });
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        expect(() => GeneratePolicyDefinitionFetchXML(pd, '' as any, 'value')).toThrow(
            /entityName/
        );
    });

    it('throws when value is null or undefined', () => {
        const pd = new Policy({
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'status',
            valueColumnType: ValueColumnType.String,
            visibleColumnName: 'shouldhide',
            allowedColumnName: 'shouldlock',
            requiredColumnName: 'shouldrequire'
        });
        expect(() => GeneratePolicyDefinitionFetchXML(pd, 'account', null)).toThrow(
            /value is required/
        );
    });

    it('generates fetch XML for string valueColumnType', () => {
        const pd = new Policy({
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'status',
            valueColumnType: ValueColumnType.String,
            visibleColumnName: 'shouldhide',
            allowedColumnName: 'shouldlock',
            requiredColumnName: 'shouldrequire'
        });

        const xml = GeneratePolicyDefinitionFetchXML(pd, 'account', 'active');
        expect(xml).toContain(`<entity name="${pd.logicalName}">`);
        expect(xml).toContain(`<attribute name="${pd.valueColumnName}" />`);
        expect(xml).toContain(`attribute="${pd.entityColumnName}"`);
        expect(xml).toContain(`value="active"`);
    });

    it('generates fetch XML for int valueColumnType', () => {
        const pd = new Policy({
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'statuscode',
            valueColumnType: ValueColumnType.Int,
            visibleColumnName: 'shouldhide',
            allowedColumnName: 'shouldlock',
            requiredColumnName: 'shouldrequire'
        });

        const xml = GeneratePolicyDefinitionFetchXML(pd, 'account', 1);
        expect(xml).toContain(`value="1"`);
    });

    it('generates fetch XML for boolean valueColumnType', () => {
        const pd = new Policy({
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'isactive',
            valueColumnType: ValueColumnType.Boolean,
            visibleColumnName: 'shouldhide',
            allowedColumnName: 'shouldlock',
            requiredColumnName: 'shouldrequire'
        });

        const xml = GeneratePolicyDefinitionFetchXML(pd, 'account', true);
        expect(xml).toContain(`value="true"`);
    });

    it('generates fetch XML for DateTime valueColumnType', () => {
        const pd = new Policy({
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'createdon',
            valueColumnType: ValueColumnType.DateTime,
            visibleColumnName: 'shouldhide',
            allowedColumnName: 'shouldlock',
            requiredColumnName: 'shouldrequire'
        });

        const date = new Date('2023-01-01T00:00:00Z');
        const xml = GeneratePolicyDefinitionFetchXML(pd, 'account', date);
        expect(xml).toContain(`value="${date.toISOString()}"`);
    });

    it('generates fetch XML for Decimal valueColumnType', () => {
        const pd = new Policy({
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'revenue',
            valueColumnType: ValueColumnType.Decimal,
            visibleColumnName: 'shouldhide',
            allowedColumnName: 'shouldlock',
            requiredColumnName: 'shouldrequire'
        });

        const xml = GeneratePolicyDefinitionFetchXML(pd, 'account', 100.5);
        expect(xml).toContain(`value="100.5"`);
    });

    it('generates fetch XML for OptionSetValue valueColumnType', () => {
        const pd = new Policy({
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'statuscode',
            valueColumnType: ValueColumnType.OptionSetValue,
            visibleColumnName: 'shouldhide',
            allowedColumnName: 'shouldlock',
            requiredColumnName: 'shouldrequire'
        });

        const value: Xrm.OptionSetValue = { value: 1, text: 'Active' };
        const xml = GeneratePolicyDefinitionFetchXML(pd, 'account', value);
        expect(xml).toContain(`value="1"`);
    });

    it('generates fetch XML for EntityReference valueColumnType with single lookup', () => {
        const pd = new Policy({
            logicalName: 'dpe_policydefinition',
            entityColumnName: 'dpe_entity',
            attributeColumnName: 'dpe_attribute',
            valueColumnName: 'dpe_lookupcomparison',
            valueColumnType: ValueColumnType.EntityReference,
            visibleColumnName: 'dpe_shouldhide',
            allowedColumnName: 'dpe_shouldlock',
            requiredColumnName: 'dpe_shouldrequire'
        });

        const value: Xrm.LookupValue = {
            id: '{6603FFD2-BA07-F111-8406-001DD806CFF9}',
            entityType: 'dpe_examplelookup',
            name: 'Example 1'
        };
        const xml = GeneratePolicyDefinitionFetchXML(pd, 'dpe_example', value);
        expect(xml).toContain(`value="{6603FFD2-BA07-F111-8406-001DD806CFF9}"`);
    });

    it('throws for unsupported valueColumnType', () => {
        const fake = {
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'status',
            visibleColumnName: 'shouldhide',
            allowedColumnName: 'shouldlock',
            requiredColumnName: 'shouldrequire',
            // bypass TS to simulate bad runtime value
            valueColumnType: 'NotAType'
        } as unknown as Policy;

        expect(() => GeneratePolicyDefinitionFetchXML(fake, 'account', 'x')).toThrow(
            /Unsupported valueColumnType/
        );
    });
});
