import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { Card } from '../card/card';
import { Stat } from './stat';
import { StatDelta } from './stat-delta';
import { StatLabel } from './stat-label';
import { StatValue } from './stat-value';

const meta: Meta<Stat> = {
  title: 'Components/Stat',
  component: Stat,
  decorators: [moduleMetadata({ imports: [Stat, StatLabel, StatValue, StatDelta, Card] })],
};

export default meta;
type Story = StoryObj<Stat>;

export const Default: Story = {
  render: () => ({
    template: `
      <div ph-card style="max-width: 16rem">
        <div ph-stat>
          <div ph-stat-label>Active youth</div>
          <div ph-stat-value>42</div>
        </div>
      </div>`,
  }),
};

export const WithDelta: Story = {
  render: () => ({
    template: `
      <div ph-card style="max-width: 16rem">
        <div ph-stat>
          <div ph-stat-label>Pending reviews</div>
          <div ph-stat-value>4</div>
          <div ph-stat-delta trend="down">2 overdue</div>
        </div>
      </div>`,
  }),
};
