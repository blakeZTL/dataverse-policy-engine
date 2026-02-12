import { defineConfig } from 'vitest/config';

export default defineConfig({
    test: {
        globals: true,
        environment: 'node',
        setupFiles: './setupTests.ts',
        coverage: {
            provider: 'istanbul',
            reporter: ['text', 'json', 'html']
        }
    }
});
