export enum ValueColumnType {
    OptionSetValue = 'OptionSetValue',
    EntityReference = 'EntityReference',
    String = 'String',
    Int = 'Int',
    Decimal = 'Decimal',
    Boolean = 'Boolean',
    DateTime = 'DateTime'
}

export interface PolicyDefinition {
    attribute: string;
    value: string;
    shouldHide: boolean;
    shouldLock: boolean;
    shouldRequire: boolean;
}

export class Policy {
    readonly logicalName: string;
    readonly entityColumnName: string;
    readonly attributeColumnName: string;
    readonly valueColumnName: string;
    readonly valueColumnType: ValueColumnType;
    readonly shouldHideColumnName: string;
    readonly shouldLockColumnName: string;
    readonly shouldRequireColumnName: string;

    constructor(params: {
        logicalName: string;
        entityColumnName: string;
        attributeColumnName: string;
        valueColumnName: string;
        valueColumnType: ValueColumnType;
        shouldHideColumnName?: string;
        shouldLockColumnName?: string;
        shouldRequireColumnName?: string;
    }) {
        this.logicalName = params.logicalName;
        this.entityColumnName = params.entityColumnName;
        this.attributeColumnName = params.attributeColumnName;
        this.valueColumnName = params.valueColumnName;
        this.valueColumnType = params.valueColumnType;
        this.shouldHideColumnName = params.shouldHideColumnName ?? '';
        this.shouldLockColumnName = params.shouldLockColumnName ?? '';
        this.shouldRequireColumnName = params.shouldRequireColumnName ?? '';
    }

    static isValidValueColumnType(v: unknown): v is ValueColumnType {
        return (
            typeof v === 'string' &&
            (Object.values(ValueColumnType) as string[]).includes(v)
        );
    }

    static fromJSON(input: unknown): Policy {
        if (!input || typeof input !== 'object') {
            throw new TypeError('Policy.fromJSON: input must be an object');
        }

        const obj = input as Record<string, unknown>;

        const logicalName = obj.logicalName;
        const entityColumnName = obj.entityColumnName;
        const attributeColumnName = obj.attributeColumnName;
        const valueColumnName = obj.valueColumnName;
        const valueColumnType = obj.valueColumnType;

        if (typeof logicalName !== 'string' || !logicalName.trim()) {
            throw new TypeError(
                'Policy.fromJSON: "logicalName" must be a non-empty string'
            );
        }
        if (typeof entityColumnName !== 'string' || !entityColumnName.trim()) {
            throw new TypeError(
                'Policy.fromJSON: "entityColumnName" must be a non-empty string'
            );
        }
        if (typeof attributeColumnName !== 'string' || !attributeColumnName.trim()) {
            throw new TypeError(
                'Policy.fromJSON: "attributeColumnName" must be a non-empty string'
            );
        }
        if (typeof valueColumnName !== 'string' || !valueColumnName.trim()) {
            throw new TypeError(
                'Policy.fromJSON: "valueColumnName" must be a non-empty string'
            );
        }
        if (!Policy.isValidValueColumnType(valueColumnType)) {
            throw new TypeError(
                `Policy.fromJSON: "valueColumnType" must be one of: ${(Object.values(ValueColumnType) as string[]).join(', ')}`
            );
        }

        const shouldHideColumnName = obj.shouldHideColumnName;
        const shouldLockColumnName = obj.shouldLockColumnName;
        const shouldRequireColumnName = obj.shouldRequireColumnName;

        if (
            shouldHideColumnName !== undefined &&
            typeof shouldHideColumnName !== 'string'
        ) {
            throw new TypeError(
                'Policy.fromJSON: "shouldHideColumnName" must be a string'
            );
        }
        if (
            shouldLockColumnName !== undefined &&
            typeof shouldLockColumnName !== 'string'
        ) {
            throw new TypeError(
                'Policy.fromJSON: "shouldLockColumnName" must be a string'
            );
        }
        if (
            shouldRequireColumnName !== undefined &&
            typeof shouldRequireColumnName !== 'string'
        ) {
            throw new TypeError(
                'Policy.fromJSON: "shouldRequireColumnName" must be a string'
            );
        }

        return new Policy({
            logicalName: logicalName as string,
            entityColumnName: entityColumnName as string,
            attributeColumnName: attributeColumnName as string,
            valueColumnName: valueColumnName as string,
            valueColumnType: valueColumnType as ValueColumnType,
            shouldHideColumnName: (shouldHideColumnName as string) || '',
            shouldLockColumnName: (shouldLockColumnName as string) || '',
            shouldRequireColumnName: (shouldRequireColumnName as string) || ''
        });
    }

    toJSON(): {
        logicalName: string;
        entityColumnName: string;
        attributeColumnName: string;
        valueColumnName: string;
        valueColumnType: ValueColumnType;
        shouldHideColumnName: string;
        shouldLockColumnName: string;
        shouldRequireColumnName: string;
    } {
        return {
            logicalName: this.logicalName,
            entityColumnName: this.entityColumnName,
            attributeColumnName: this.attributeColumnName,
            valueColumnName: this.valueColumnName,
            valueColumnType: this.valueColumnType,
            shouldHideColumnName: this.shouldHideColumnName,
            shouldLockColumnName: this.shouldLockColumnName,
            shouldRequireColumnName: this.shouldRequireColumnName
        };
    }
}
