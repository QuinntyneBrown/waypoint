// @ts-check
const eslint = require('@eslint/js');
const { defineConfig } = require('eslint/config');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');

module.exports = defineConfig([
  {
    files: ['**/*.ts'],
    extends: [
      eslint.configs.recommended,
      tseslint.configs.recommended,
      tseslint.configs.stylistic,
      angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@typescript-eslint/no-empty-function': ['error', { allow: ['arrowFunctions', 'methods'] }],
      '@angular-eslint/no-output-rename': 'warn',
      '@angular-eslint/directive-selector': [
        'warn',
        {
          type: 'attribute',
          prefix: 'w',
          style: 'camelCase',
        },
      ],
      '@angular-eslint/component-selector': [
        'error',
        {
          type: 'element',
          prefix: 'w',
          style: 'kebab-case',
        },
      ],
    },
  },
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility],
    rules: {
      // Pre-existing (issue #35). Fixing these changes behaviour (keyboard access), so they need
      // a requirement, design and tests first; until then they warn instead of failing CI.
      '@angular-eslint/template/click-events-have-key-events': 'warn',
      '@angular-eslint/template/interactive-supports-focus': 'warn',
      '@angular-eslint/template/valid-aria': 'warn',
      '@angular-eslint/template/label-has-associated-control': 'warn',
    },
  },
]);
