import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WButton } from '../button/w-button';
import { WCard } from '../card/w-card';
import { WIcon } from '../icon/w-icon';
import { WEmptyState, WEmptyStateIcon, WEmptyStateText, WEmptyStateTitle } from './w-empty-state';

const meta: Meta<WEmptyState> = {
  title: 'Components/Empty state',
  component: WEmptyState,
  decorators: [
    moduleMetadata({
      imports: [
        WEmptyState,
        WEmptyStateIcon,
        WEmptyStateTitle,
        WEmptyStateText,
        WIcon,
        WButton,
        WCard,
      ],
    }),
  ],
};

export default meta;
type Story = StoryObj<WEmptyState>;

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
