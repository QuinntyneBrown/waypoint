import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from '@waypoint/components';

@Component({
  selector: 'w-public-not-found',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, Button],
  templateUrl: './not-found.html',
  styleUrl: './not-found.css',
})
export class NotFound {}
