import type { StorybookConfig } from '@storybook/angular';

const config: StorybookConfig = {
  stories: ['../src/**/*.stories.ts'],
  addons: ['@storybook/addon-a11y', '@storybook/addon-docs'],
  staticDirs: [{ from: '../src/lib/video-player/demo/media', to: '/demo' }],
  framework: '@storybook/angular',
};

export default config;
