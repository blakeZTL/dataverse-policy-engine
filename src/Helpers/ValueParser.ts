import { ValueColumnType } from '../types';

export function ValueParser(
    value: string | number | boolean | Date | Xrm.LookupValue[] | Xrm.OptionSetValue,
    valueType: ValueColumnType
): string | number | boolean | Date | Xrm.LookupValue | Xrm.OptionSetValue {
    if (value === null || value === undefined) {
        throw new TypeError('ValueParser: value is required');
    }
    if (!valueType) {
        throw new TypeError('ValueParser: valueType is required');
    }
    console.debug('ValueParser: Parsing value with the following parameters', {
        value,
        valueType
    });
    if (Array.isArray(value) && valueType === 'EntityReference') {
        if (value.length === 0) {
            throw new Error(
                'ValueParser: value array is empty for EntityReference type'
            );
        }
        const lookupValue = value[0] as Xrm.LookupValue;
        console.debug(
            'ValueParser: Parsed EntityReference value from array',
            lookupValue
        );
        return lookupValue;
    }
    if (Array.isArray(value)) {
        throw new Error(
            'ValueParser: unexpected array value for non-EntityReference type'
        );
    }
    console.debug('ValueParser: Returning value without modification', value);
    return value;
}
