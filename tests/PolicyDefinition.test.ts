import { describe, it, expect } from 'vitest';

import { Policy, ValueColumnType } from '../src/types';

describe('Policy', () => {
    it('parses valid JSON and round-trips to JSON', () => {
        const json = {
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'status',
            valueColumnType: ValueColumnType.String,
            visibilityColumnName: 'hide',
            allowedColumnName: 'lock',
            requiredColumnName: 'require'
        } as const;

        const pd = Policy.fromJSON(json);
        expect(pd).toBeInstanceOf(Policy);
        expect(pd.logicalName).toBe('account');
        expect(pd.entityColumnName).toBe('accountid');
        expect(pd.attributeColumnName).toBe('name');
        expect(pd.valueColumnName).toBe('status');
        expect(pd.valueColumnType).toBe(ValueColumnType.String);
        expect(pd.visibilityColumnName).toBe('hide');
        expect(pd.allowedColumnName).toBe('lock');
        expect(pd.requiredColumnName).toBe('require');
        expect(pd.toJSON()).toEqual(json);
    });

    it('throws when required string fields are missing or empty', () => {
        const missingLogical = {
            entityColumnName: 'accountid',
            valueColumnName: 'status',
            valueColumnType: ValueColumnType.String
        };
        expect(() => Policy.fromJSON(missingLogical as unknown)).toThrow(/logicalName/);

        const emptyEntity = {
            logicalName: 'account',
            entityColumnName: '   ',
            valueColumnName: 'status',
            valueColumnType: ValueColumnType.String
        };
        expect(() => Policy.fromJSON(emptyEntity as unknown)).toThrow(
            /entityColumnName/
        );
    });

    it('throws when valueColumnType is invalid', () => {
        const badType = {
            logicalName: 'account',
            entityColumnName: 'accountid',
            attributeColumnName: 'name',
            valueColumnName: 'status',
            valueColumnType: 'NotAType'
        };
        expect(() => Policy.fromJSON(badType as unknown)).toThrow(/valueColumnType/);
    });
});
