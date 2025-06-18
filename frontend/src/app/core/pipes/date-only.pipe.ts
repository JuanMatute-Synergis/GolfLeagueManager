import { Pipe, PipeTransform } from '@angular/core';
import { DateUtilService } from '../services/date-util.service';

@Pipe({
  name: 'dateOnly',
  standalone: true
})
export class DateOnlyPipe implements PipeTransform {

  constructor(private dateUtil: DateUtilService) {}

  transform(value: string, format?: 'full' | 'short' | 'dayOnly'): string {
    if (!value) return '';
    
    switch (format) {
      case 'short':
        return this.dateUtil.formatDateShort(value);
      case 'dayOnly':
        return this.dateUtil.getDayOfWeek(value);
      case 'full':
      default:
        return this.dateUtil.formatDateOnly(value);
    }
  }
}
