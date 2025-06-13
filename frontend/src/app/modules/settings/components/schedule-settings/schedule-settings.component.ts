import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

interface ScheduleTemplate {
  id?: string;
  name: string;
  description: string;
  numberOfRounds: number;
  startDate: string;
  endDate: string;
  playDays: string[];
  startTime: string;
}

interface ScheduleSettings {
  defaultStartTime: string;
  defaultPlayDays: string[];
  autoGenerateSchedule: boolean;
  allowPlayerRescheduling: boolean;
  rescheduleDeadlineHours: number;
}

@Component({
  selector: 'app-schedule-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './schedule-settings.component.html',
  styleUrls: ['./schedule-settings.component.css']
})
export class ScheduleSettingsComponent implements OnInit {
  templateForm: FormGroup;
  settingsForm: FormGroup;
  scheduleTemplates: ScheduleTemplate[] = [];
  scheduleSettings: ScheduleSettings = {
    defaultStartTime: '09:00',
    defaultPlayDays: ['Saturday'],
    autoGenerateSchedule: true,
    allowPlayerRescheduling: false,
    rescheduleDeadlineHours: 24
  };
  
  showTemplateForm = false;
  editingTemplateId: string | null = null;
  loading = false;
  error: string | null = null;

  daysOfWeek = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

  constructor(private fb: FormBuilder) {
    this.templateForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      numberOfRounds: [18, [Validators.required, Validators.min(1)]],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      playDays: [[], Validators.required],
      startTime: ['09:00', Validators.required]
    });

    this.settingsForm = this.fb.group({
      defaultStartTime: ['09:00', Validators.required],
      defaultPlayDays: [[], Validators.required],
      autoGenerateSchedule: [true],
      allowPlayerRescheduling: [false],
      rescheduleDeadlineHours: [24, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit() {
    this.loadScheduleTemplates();
    this.loadScheduleSettings();
  }

  loadScheduleTemplates() {
    this.loading = true;
    // Mock data for now
    setTimeout(() => {
      this.scheduleTemplates = [
        {
          id: '1',
          name: 'Standard Weekly',
          description: 'Weekly games every Saturday',
          numberOfRounds: 18,
          startDate: '2024-01-01',
          endDate: '2024-12-31',
          playDays: ['Saturday'],
          startTime: '09:00'
        },
        {
          id: '2',
          name: 'Bi-Weekly Tournament',
          description: 'Tournament every other weekend',
          numberOfRounds: 18,
          startDate: '2024-01-01',
          endDate: '2024-12-31',
          playDays: ['Saturday', 'Sunday'],
          startTime: '08:00'
        }
      ];
      this.loading = false;
    }, 500);
  }

  loadScheduleSettings() {
    // Mock loading settings
    setTimeout(() => {
      this.settingsForm.patchValue(this.scheduleSettings);
    }, 100);
  }

  showAddTemplateForm() {
    this.showTemplateForm = true;
    this.editingTemplateId = null;
    this.templateForm.reset({
      name: '',
      description: '',
      numberOfRounds: 18,
      startDate: '',
      endDate: '',
      playDays: [],
      startTime: '09:00'
    });
  }

  editTemplate(template: ScheduleTemplate) {
    this.showTemplateForm = true;
    this.editingTemplateId = template.id!;
    this.templateForm.patchValue({
      name: template.name,
      description: template.description,
      numberOfRounds: template.numberOfRounds,
      startDate: template.startDate,
      endDate: template.endDate,
      playDays: template.playDays,
      startTime: template.startTime
    });
  }

  saveTemplate() {
    if (this.templateForm.invalid) {
      return;
    }

    this.loading = true;
    const formValue = this.templateForm.value;

    setTimeout(() => {
      if (this.editingTemplateId) {
        // Update existing template
        const index = this.scheduleTemplates.findIndex(t => t.id === this.editingTemplateId);
        if (index !== -1) {
          this.scheduleTemplates[index] = {
            ...this.scheduleTemplates[index],
            ...formValue
          };
        }
      } else {
        // Add new template
        const newTemplate: ScheduleTemplate = {
          id: Date.now().toString(),
          ...formValue
        };
        this.scheduleTemplates.push(newTemplate);
      }

      this.resetTemplateForms();
      this.loading = false;
    }, 500);
  }

  deleteTemplate(id: string) {
    if (confirm('Are you sure you want to delete this schedule template?')) {
      this.loading = true;
      setTimeout(() => {
        this.scheduleTemplates = this.scheduleTemplates.filter(t => t.id !== id);
        this.loading = false;
      }, 300);
    }
  }

  saveSettings() {
    if (this.settingsForm.invalid) {
      return;
    }

    this.loading = true;
    const formValue = this.settingsForm.value;

    setTimeout(() => {
      this.scheduleSettings = { ...formValue };
      this.loading = false;
      // Show success message
      alert('Schedule settings saved successfully!');
    }, 500);
  }

  resetTemplateForms() {
    this.showTemplateForm = false;
    this.editingTemplateId = null;
    this.templateForm.reset();
    this.error = null;
  }

  onPlayDayChange(day: string, event: any) {
    const formControl = this.templateForm.get('playDays');
    let playDays = formControl?.value || [];
    
    if (event.target.checked) {
      if (!playDays.includes(day)) {
        playDays.push(day);
      }
    } else {
      playDays = playDays.filter((d: string) => d !== day);
    }
    
    formControl?.setValue(playDays);
  }

  onDefaultPlayDayChange(day: string, event: any) {
    const formControl = this.settingsForm.get('defaultPlayDays');
    let playDays = formControl?.value || [];
    
    if (event.target.checked) {
      if (!playDays.includes(day)) {
        playDays.push(day);
      }
    } else {
      playDays = playDays.filter((d: string) => d !== day);
    }
    
    formControl?.setValue(playDays);
  }

  isPlayDaySelected(day: string): boolean {
    const playDays = this.templateForm.get('playDays')?.value || [];
    return playDays.includes(day);
  }

  isDefaultPlayDaySelected(day: string): boolean {
    const playDays = this.settingsForm.get('defaultPlayDays')?.value || [];
    return playDays.includes(day);
  }
}
