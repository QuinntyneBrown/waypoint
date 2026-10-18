import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'w-privacy',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './privacy.html',
  styleUrl: './privacy.css',
})
export class Privacy {}
