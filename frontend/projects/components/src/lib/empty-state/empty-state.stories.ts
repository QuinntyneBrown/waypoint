import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Button } from '../button/button';
import { Card } from '../card/card';
import { Icon } from '../icon/icon';
import { EmptyState } from './empty-state';
import { EmptyStateIcon } from './empty-state-icon';
import { EmptyStateText } from './empty-state-text';
import { EmptyStateTitle } from './empty-state-title';

const meta: Meta<EmptyState> = {
  title: 'Components/Empty state',
  component: EmptyState,
  decorators: [
    moduleMetadata({
      imports: [EmptyState, EmptyStateIcon, EmptyStateTitle, EmptyStateText, Icon, Button, Card],
    }),
  ],
};

export default meta;
type Story = StoryObj<EmptyState>;

export const Default: Story = {
  render: () => ({
    template: `
      <div w-empty-state>
        <div w-empty-state-icon><w-icon name="layers" /></div>
        <h2 w-empty-state-title>No cohorts yet</h2>
        <p w-empty-state-text>Create the first cohort to begin placing young people.</p>
        <button w-button variant="primary" type="button">New cohort</button>
      </div>`,
  }),
};

export const InsideACard: Story = {
  render: () => ({
    template: `
      <div w-card>
        <div w-empty-state>
          <div w-empty-state-icon><w-icon name="users" /></div>
          <h3 w-empty-state-title>No youth assigned yet</h3>
          <p w-empty-state-text>Assigned youth will appear here.</p>
        </div>
      </div>`,
  }),
};
