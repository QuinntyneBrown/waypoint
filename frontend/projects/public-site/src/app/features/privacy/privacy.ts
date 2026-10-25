import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'ph-privacy',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './privacy.html',
  styleUrl: './privacy.css',
})
export class Privacy {}
