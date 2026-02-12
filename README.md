# Dataverse Policy Engine

A TypeScript library for enforcing dynamic policies on Microsoft Dataverse (Dynamics 365) form fields. It retrieves policy definitions via the Web API and applies visibility, editability, and requirement rules based on attribute values.

## Features

- Fetches policy definitions using FetchXML queries.
- Supports various data types: String, Int, Boolean, DateTime, Decimal, OptionSetValue, EntityReference.
- Applies policies to form controls: visibility, disabled state, and required level.
- Integrates with Xrm.WebApi for Dataverse interactions.
- Includes helpers for parsing values and generating FetchXML.

## Installation

1. Clone the repository:

    ```bash
    git clone https://github.com/blakeZTL/dataverse-policy-engine.git
    cd dataverse-policy-engine
    ```

2. Install dependencies:

    ```bash
    npm install
    ```

3. Build the project:
    ```bash
    npm run build
    ```

## Usage

### Basic Example

Import the `PolicyEngine` function and call it in a form event handler (e.g., onLoad or onChange).

```typescript
import { PolicyEngine } from './PolicyEngine/index';

// Define your policy configuration
const policy = {
    logicalName: 'dpe_policydefinition',
    entityColumnName: 'dpe_entity',
    attributeColumnName: 'dpe_attribute',
    valueColumnName: 'dpe_value',
    valueColumnType: 'String',
    visibilityColumnName: 'dpe_visible',
    allowedColumnName: 'dpe_allowed',
    requiredColumnName: 'dpe_required'
};

// In your form script (e.g., onLoad)
function onLoad(executionContext: Xrm.Events.EventContext) {
    PolicyEngine(executionContext, 'name', policy).catch(console.error);
}
```

### Policy Configuration

- `logicalName`: The Dataverse table name for policy definitions.
- `entityColumnName`: Column for the entity name.
- `attributeColumnName`: Column for the attribute name.
- `valueColumnName`: Column for the value to match.
- `valueColumnType`: Data type of the value (String, Int, Boolean, DateTime, Decimal, OptionSetValue, EntityReference).
- `visibilityColumnName`: Column for visibility flag.
- `allowedColumnName`: Column for editability flag.
- `requiredColumnName`: Column for required flag.

### Helpers

- `GeneratePolicyDefinitionFetchXML`: Generates FetchXML for querying policy definitions.
- `ValueParser`: Parses attribute values for FetchXML conditions.

## Testing

Run tests with Vitest:

```bash
npm test
```

Tests use Xrm-Mock for simulating Dataverse environments.

## Contributing

1. Fork the repository.
2. Create a feature branch.
3. Make changes and add tests.
4. Submit a pull request.

## License

MIT License. See LICENSE file for details.
