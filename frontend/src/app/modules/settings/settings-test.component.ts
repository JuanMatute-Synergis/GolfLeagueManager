import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-settings-test',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h1>Settings Test Component</h1>
      <p>This is a test to verify the component works.</p>
    </div>
  `,
  styles: []
})
export class SettingsTestComponent {
  
}
