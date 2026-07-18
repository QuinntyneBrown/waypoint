import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'wu-root',
  imports: [RouterLink, RouterOutlet],
  templateUrl: './app.html',
})
export class App {}
