import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { WuToastHost } from '@wu/ui';

@Component({
  selector: 'wu-root',
  imports: [RouterOutlet, WuToastHost],
  template: `<router-outlet /> <wu-toast-host />`,
})
export class App {}
