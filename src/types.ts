export enum ValueColumnType {
    OptionSetValue = 'OptionSetValue',
    EntityReference = 'EntityReference',
    String = 'String',
    Int = 'Int',
    Decimal = 'Decimal',
    Boolean = 'Boolean',
    DateTime = 'DateTime'
}

export interface PolicyEngineConfig {
    attributeName: string;
    suppliedPolicy: object;
}

export interface PolicyDefinition {
    attribute: string;
    value: string;
    visible: boolean;
    allowed: boolean;
    required: boolean;
}

export class Policy {
    readonly logicalName: string;
    readonly entityColumnName: string;
    readonly attributeColumnName: string;
    readonly valueColumnName: string;
    readonly valueColumnType: ValueColumnType;
    readonly visibleColumnName: string;
    readonly allowedColumnName: string;
    readonly requiredColumnName: string;

    constructor(params: {
        logicalName: string;
        entityColumnName: string;
        attributeColumnName: string;
        valueColumnName: string;
        valueColumnType: ValueColumnType;
        visibleColumnName?: string;
        allowedColumnName?: string;
        requiredColumnName?: string;
    }) {
        this.logicalName = params.logicalName;
        this.entityColumnName = params.entityColumnName;
        this.attributeColumnName = params.attributeColumnName;
        this.valueColumnName = params.valueColumnName;
        this.valueColumnType = params.valueColumnType;
        this.visibleColumnName = params.visibleColumnName ?? '';
        this.allowedColumnName = params.allowedColumnName ?? '';
        this.requiredColumnName = params.requiredColumnName ?? '';
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

        const visibleColumnName = obj.visibleColumnName;
        const allowedColumnName = obj.allowedColumnName;
        const requiredColumnName = obj.requiredColumnName;

        if (visibleColumnName !== undefined && typeof visibleColumnName !== 'string') {
            throw new TypeError(
                'Policy.fromJSON: "visibleColumnName" must be a string'
            );
        }
        if (allowedColumnName !== undefined && typeof allowedColumnName !== 'string') {
            throw new TypeError(
                'Policy.fromJSON: "allowedColumnName" must be a string'
            );
        }
        if (
            requiredColumnName !== undefined &&
            typeof requiredColumnName !== 'string'
        ) {
            throw new TypeError(
                'Policy.fromJSON: "requiredColumnName" must be a string'
            );
        }

        return new Policy({
            logicalName: logicalName as string,
            entityColumnName: entityColumnName as string,
            attributeColumnName: attributeColumnName as string,
            valueColumnName: valueColumnName as string,
            valueColumnType: valueColumnType as ValueColumnType,
            visibleColumnName: (visibleColumnName as string) || '',
            allowedColumnName: (allowedColumnName as string) || '',
            requiredColumnName: (requiredColumnName as string) || ''
        });
    }

    toJSON(): {
        logicalName: string;
        entityColumnName: string;
        attributeColumnName: string;
        valueColumnName: string;
        valueColumnType: ValueColumnType;
        visibleColumnName: string;
        allowedColumnName: string;
        requiredColumnName: string;
    } {
        return {
            logicalName: this.logicalName,
            entityColumnName: this.entityColumnName,
            attributeColumnName: this.attributeColumnName,
            valueColumnName: this.valueColumnName,
            valueColumnType: this.valueColumnType,
            visibleColumnName: this.visibleColumnName,
            allowedColumnName: this.allowedColumnName,
            requiredColumnName: this.requiredColumnName
        };
    }
}
