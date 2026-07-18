import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface JourneyStep {
  n: number;
  title: string;
  text: string;
}
interface Stat {
  value: string;
  label: string;
}

// Traces to: L2-PUB-002
@Component({
  selector: 'wu-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './home.html',
})
export class Home {
  protected readonly journey: JourneyStep[] = [
    { n: 1, title: 'Apply', text: 'Tell us about yourself — it takes five minutes.' },
    { n: 2, title: 'Join a cohort', text: 'Get placed with a mentor and a group near you.' },
    {
      n: 3,
      title: 'Learn the modules',
      text: 'Grow through Bible & Leadership or Coding for Kingdom.',
    },
    { n: 4, title: 'Lead & graduate', text: 'Serve your community and step into leadership.' },
  ];

  protected readonly stats: Stat[] = [
    { value: '120+', label: 'Young leaders' },
    { value: '3', label: 'Active cohorts' },
    { value: '2', label: 'Learning tracks' },
    { value: '94%', label: 'Would recommend' },
  ];
}
