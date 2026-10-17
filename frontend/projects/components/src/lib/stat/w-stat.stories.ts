import type { Meta, StoryObj } from '@storybook/angular';
import { moduleMetadata } from '@storybook/angular';
import { WCard } from '../card/w-card';
import { WStat, WStatDelta, WStatLabel, WStatValue } from './w-stat';

const meta: Meta<WStat> = {
  title: 'Components/Stat',
  component: WStat,
  decorators: [moduleMetadata({ imports: [WStat, WStatLabel, WStatValue, WStatDelta, WCard] })],
};

export default meta;
type Story = StoryObj<WStat>;

export const Default: Story = {
  render: () => ({
    template: `
      <div w-card style="max-width: 16rem">
        <div w-stat>
          <div w-stat-label>Active youth</div>
          <div w-stat-value>42</div>
        </div>
      </div>`,
  }),
};

export const WithDelta: Story = {
  render: () => ({
    template: `
      <div w-card style="max-width: 16rem">
        <div w-stat>
          <div w-stat-label>Pending reviews</div>
          <div w-stat-value>4</div>
          <div w-stat-delta trend="down">2 overdue</div>
        </div>
      </div>`,
  }),
};
