import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DateUtilService {

  /**
   * Formats a date-only string (YYYY-MM-DD) as a local date without timezone conversion.
   * This prevents issues where the date shifts by one day due to timezone differences.
   */
  formatDateOnly(dateString: string, options?: Intl.DateTimeFormatOptions): string {
    if (!dateString) return '';
    
    // Parse the date string as YYYY-MM-DD and create a local date
    const [year, month, day] = dateString.split('T')[0].split('-').map(Number);
    const localDate = new Date(year, month - 1, day); // month is 0-based in Date constructor
    
    const defaultOptions: Intl.DateTimeFormatOptions = {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    };
    
    return localDate.toLocaleDateString('en-US', options || defaultOptions);
  }

  /**
   * Formats a date-only string as a short date (e.g., "Wed, Apr 9")
   */
  formatDateShort(dateString: string): string {
    return this.formatDateOnly(dateString, {
      weekday: 'short',
      month: 'short',
      day: 'numeric'
    });
  }

  /**
   * Gets just the day of the week from a date string
   */
  getDayOfWeek(dateString: string): string {
    return this.formatDateOnly(dateString, {
      weekday: 'long'
    });
  }
}
