import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface Track {
  name: string;
  blurb: string;
  modules: string[];
  badges: string[];
}

// Traces to: L2-PUB-002
@Component({
  selector: 'wu-programs',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './programs.html',
})
export class Programs {
  protected readonly tracks: Track[] = [
    {
      name: 'Bible & Leadership',
      blurb: 'Grow in faith and character while learning to lead others well.',
      modules: [
        'Who Is Jesus?',
        'Identity in Christ',
        'Servant Leadership',
        'Leading a Small Group',
        'Sharing Your Story',
      ],
      badges: ['First Steps', 'Word Warrior', 'Servant Heart', 'Storyteller'],
    },
    {
      name: 'Coding for Kingdom',
      blurb: 'Build real projects and gain tech skills that open doors.',
      modules: ['Intro to Scratch', 'Web Basics: HTML & CSS'],
      badges: ['Code Starter', 'Faithful Five'],
    },
  ];

  protected readonly mentorship = [
    { title: 'Weekly check-ins', text: 'A mentor who knows your name and your goals.' },
    { title: 'Reflection reviews', text: 'Thoughtful feedback on your reflections and work.' },
    { title: 'Extra support', text: 'Someone in your corner when things get hard.' },
  ];
}
