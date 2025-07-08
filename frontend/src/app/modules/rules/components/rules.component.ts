import { Component, OnInit, inject, signal, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { QuillModule } from 'ngx-quill';
import { RulesService } from '../services/rules.service';
import { UserProfileService } from '../../../core/services/user-profile.service';
import { SeasonService } from '../../settings/services/season.service';
import { LeagueRules, UpdateRulesRequest } from '../models/league-rules.model';
import { firstValueFrom } from 'rxjs';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

@Component({
  selector: 'app-rules',
  standalone: true,
  imports: [CommonModule, FormsModule, QuillModule],
  templateUrl: './rules.component.html',
  styleUrls: ['./rules.component.css']
})
export class RulesComponent implements OnInit {
  private rulesService = inject(RulesService);
  private userProfileService = inject(UserProfileService);
  private seasonService = inject(SeasonService);
  
  @ViewChild('pdfContent', { static: false }) pdfContent!: ElementRef;
  
  rules = signal<LeagueRules | null>(null);
  isEditing = signal(false);
  isLoading = signal(false);
  isExporting = signal(false);
  error = signal<string | null>(null);
  editContent = signal('');
  
  // Will be set dynamically from the active season
  private currentSeasonId = '';
  
  quillConfig = {
    toolbar: [
      ['bold', 'italic', 'underline', 'strike'],
      ['blockquote', 'code-block'],
      [{ 'header': 1 }, { 'header': 2 }],
      [{ 'list': 'ordered'}, { 'list': 'bullet' }],
      [{ 'script': 'sub'}, { 'script': 'super' }],
      [{ 'indent': '-1'}, { 'indent': '+1' }],
      [{ 'size': ['small', false, 'large', 'huge'] }],
      [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
      [{ 'color': [] }, { 'background': [] }],
      [{ 'align': [] }],
      ['clean'],
      ['link']
    ]
  };

  get isAdmin(): boolean {
    const profile = this.userProfileService.getProfile();
    return profile?.isAdmin || false;
  }

  async ngOnInit() {
    await this.loadCurrentSeason();
    await this.loadRules();
  }

  private async loadCurrentSeason() {
    try {
      // Try to get active seasons first
      const activeSeasons = await firstValueFrom(this.seasonService.getActiveSeasons());
      if (activeSeasons && activeSeasons.length > 0) {
        this.currentSeasonId = activeSeasons[0].id;
        return;
      }
      
      // Fallback: get all seasons and use the most recent one
      const allSeasons = await firstValueFrom(this.seasonService.getSeasons());
      if (allSeasons && allSeasons.length > 0) {
        // Sort by start date descending and take the first one
        const sortedSeasons = allSeasons.sort((a, b) => 
          new Date(b.startDate).getTime() - new Date(a.startDate).getTime()
        );
        this.currentSeasonId = sortedSeasons[0].id;
        return;
      }
      
      // Last resort: use the known season ID
      this.currentSeasonId = '50048169-218c-411d-bcf2-195587b197bf';
      
    } catch (error) {
      console.error('Error loading current season:', error);
      // Use the known season ID as fallback
      this.currentSeasonId = '50048169-218c-411d-bcf2-195587b197bf';
    }
  }

  private async loadRules() {
    this.isLoading.set(true);
    this.error.set(null);
    
    try {
      const rules = await firstValueFrom(this.rulesService.getRules(this.currentSeasonId));
      this.rules.set(rules);
    } catch (error: any) {
      if (error.status === 404) {
        // No rules exist yet for this season
        this.rules.set({
          id: '',
          seasonId: this.currentSeasonId,
          content: '<h2>League Rules</h2><p>No rules have been set for this season yet.</p>',
          createdAt: '',
          updatedAt: '',
          createdBy: '',
          updatedBy: ''
        });
      } else {
        this.error.set('Failed to load rules');
        console.error('Error loading rules:', error);
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  startEditing() {
    if (!this.isAdmin) return;
    
    this.editContent.set(this.rules()?.content || '');
    this.isEditing.set(true);
  }

  cancelEditing() {
    this.isEditing.set(false);
    this.editContent.set('');
  }

  async saveRules() {
    if (!this.isAdmin) return;
    
    this.isLoading.set(true);
    this.error.set(null);
    
    try {
      const request: UpdateRulesRequest = {
        content: this.editContent()
      };
      
      const updatedRules = await firstValueFrom(
        this.rulesService.updateRules(this.currentSeasonId, request)
      );
      
      this.rules.set(updatedRules);
      this.isEditing.set(false);
      this.editContent.set('');
    } catch (error) {
      this.error.set('Failed to save rules');
      console.error('Error saving rules:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  async exportToPdf() {
    if (!this.rules()?.content) {
      this.error.set('No content to export');
      return;
    }

    this.isExporting.set(true);
    this.error.set(null);

    try {
      // Create a completely isolated iframe for rendering
      const iframe = document.createElement('iframe');
      iframe.style.position = 'absolute';
      iframe.style.left = '-9999px';
      iframe.style.top = '0';
      iframe.style.width = '800px';
      iframe.style.height = '600px';
      document.body.appendChild(iframe);

      const iframeDoc = iframe.contentDocument || iframe.contentWindow?.document;
      if (!iframeDoc) {
        throw new Error('Unable to create iframe document');
      }

      // Write clean HTML with only safe CSS to the iframe
      iframeDoc.open();
      iframeDoc.write(`
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset="utf-8">
          <title>League Rules</title>
          <style>
            * {
              margin: 0;
              padding: 0;
              box-sizing: border-box;
              color: #000000 !important;
              background-color: transparent !important;
              border-color: #cccccc !important;
            }
            body {
              font-family: Arial, sans-serif;
              line-height: 1.6;
              color: #000000;
              background-color: #ffffff;
              padding: 20px;
              max-width: 800px;
              margin: 0 auto;
            }
            h1 { 
              font-size: 24px; 
              font-weight: bold; 
              margin: 20px 0 10px 0;
              color: #000000;
            }
            h2 { 
              font-size: 20px; 
              font-weight: bold; 
              margin: 18px 0 8px 0;
              color: #000000;
            }
            h3 { 
              font-size: 16px; 
              font-weight: bold; 
              margin: 16px 0 6px 0;
              color: #000000;
            }
            p { 
              margin: 10px 0;
              color: #000000;
            }
            ul, ol { 
              margin: 10px 0;
              padding-left: 20px;
            }
            li { 
              margin: 4px 0;
              color: #000000;
            }
            strong, b { 
              font-weight: bold;
              color: #000000;
            }
            hr {
              border: none;
              border-top: 1px solid #cccccc;
              margin: 20px 0;
            }
            blockquote {
              border-left: 4px solid #cccccc;
              margin: 15px 0;
              padding-left: 15px;
              font-style: italic;
              color: #000000;
            }
            code {
              background-color: #f5f5f5;
              padding: 2px 4px;
              border-radius: 3px;
              font-family: monospace;
              color: #000000;
            }
          </style>
        </head>
        <body>
          <div id="content">
            ${this.rules()?.content || ''}
          </div>
        </body>
        </html>
      `);
      iframeDoc.close();

      // Wait for iframe to load
      await new Promise(resolve => setTimeout(resolve, 500));

      // Get the content element from iframe
      const contentElement = iframeDoc.getElementById('content');
      if (!contentElement) {
        throw new Error('Content element not found in iframe');
      }

      // Use html2canvas on the iframe content
      const canvas = await html2canvas(contentElement, {
        scale: 2,
        useCORS: true,
        allowTaint: true,
        backgroundColor: '#ffffff',
        logging: false,
        width: 800,
        height: contentElement.scrollHeight,
        windowWidth: 800,
        windowHeight: contentElement.scrollHeight
      });

      // Remove iframe
      document.body.removeChild(iframe);

      // Generate PDF
      const imgWidth = 210; // A4 width in mm
      const pageHeight = 295; // A4 height in mm
      const imgHeight = (canvas.height * imgWidth) / canvas.width;
      let heightLeft = imgHeight;

      const pdf = new jsPDF('p', 'mm', 'a4');
      let position = 0;

      // Add first page
      pdf.addImage(canvas.toDataURL('image/png'), 'PNG', 0, position, imgWidth, imgHeight);
      heightLeft -= pageHeight;

      // Add additional pages if needed
      while (heightLeft >= 0) {
        position = heightLeft - imgHeight;
        pdf.addPage();
        pdf.addImage(canvas.toDataURL('image/png'), 'PNG', 0, position, imgWidth, imgHeight);
        heightLeft -= pageHeight;
      }

      // Download the PDF
      const fileName = `league-rules-${new Date().toISOString().split('T')[0]}.pdf`;
      pdf.save(fileName);

    } catch (error) {
      this.error.set('Failed to export PDF');
      console.error('Error exporting PDF:', error);
    } finally {
      this.isExporting.set(false);
    }
  }

  // Alternative method: Print to PDF using browser's print dialog (more reliable)
  printToPdf() {
    if (!this.rules()?.content) {
      this.error.set('No content to print');
      return;
    }

    // Open print dialog with optimized settings for PDF
    const printWindow = window.open('', '_blank');
    if (printWindow) {
      // Get clean content without problematic CSS
      const content = this.rules()?.content || '';
      
      printWindow.document.write(`
        <!DOCTYPE html>
        <html>
        <head>
          <title>League Rules</title>
          <style>
            body {
              font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
              line-height: 1.6;
              color: #000000;
              max-width: 800px;
              margin: 0 auto;
              padding: 20px;
              background-color: #ffffff;
            }
            h1, h2, h3, h4, h5, h6 {
              color: #000000;
              margin-top: 1.5rem;
              margin-bottom: 0.5rem;
            }
            h1 { font-size: 2rem; }
            h2 { font-size: 1.5rem; }
            h3 { font-size: 1.25rem; }
            p { margin-bottom: 1rem; color: #000000; }
            ul, ol { margin-bottom: 1rem; padding-left: 1.5rem; }
            li { margin-bottom: 0.25rem; color: #000000; }
            blockquote {
              border-left: 4px solid #cccccc;
              margin: 1rem 0;
              padding-left: 1rem;
              font-style: italic;
              color: #000000;
            }
            code {
              background-color: #f5f5f5;
              padding: 0.2rem 0.4rem;
              border-radius: 3px;
              font-size: 0.9em;
              color: #000000;
            }
            table {
              width: 100%;
              border-collapse: collapse;
              margin-bottom: 1rem;
            }
            th, td {
              border: 1px solid #dddddd;
              padding: 0.5rem;
              text-align: left;
              color: #000000;
            }
            th { 
              background-color: #f5f5f5; 
              font-weight: bold;
            }
            hr {
              border: none;
              border-top: 1px solid #cccccc;
              margin: 2rem 0;
            }
            strong {
              font-weight: bold;
              color: #000000;
            }
            @media print {
              body { 
                margin: 0; 
                background-color: white !important;
              }
              * { 
                -webkit-print-color-adjust: exact !important; 
                color: black !important;
                background-color: white !important;
              }
            }
          </style>
        </head>
        <body>
          <h1>League Rules</h1>
          <div class="content">
            ${content}
          </div>
        </body>
        </html>
      `);
      printWindow.document.close();
      printWindow.focus();
      
      // Wait a bit for content to load, then print
      setTimeout(() => {
        printWindow.print();
        printWindow.close();
      }, 250);
    }
  }

  // Fallback method: Simple PDF generation without html2canvas
  async exportToPdfSimple() {
    if (!this.rules()?.content) {
      this.error.set('No content to export');
      return;
    }

    this.isExporting.set(true);
    this.error.set(null);

    try {
      // Create PDF with just text content (no images/styling)
      const pdf = new jsPDF('p', 'mm', 'a4');
      
      // Clean the HTML content to extract just text
      const tempDiv = document.createElement('div');
      tempDiv.innerHTML = this.rules()?.content || '';
      
      // Extract text content and format it
      const textContent = this.extractTextContent(tempDiv);
      
      // Add title
      pdf.setFontSize(20);
      pdf.text('League Rules', 20, 20);
      
      // Add content
      pdf.setFontSize(12);
      const lines = pdf.splitTextToSize(textContent, 170);
      let y = 40;
      
      lines.forEach((line: string) => {
        if (y > 280) { // Near bottom of page
          pdf.addPage();
          y = 20;
        }
        pdf.text(line, 20, y);
        y += 6;
      });
      
      // Download the PDF
      const fileName = `league-rules-simple-${new Date().toISOString().split('T')[0]}.pdf`;
      pdf.save(fileName);

    } catch (error) {
      this.error.set('Failed to export simple PDF');
      console.error('Error exporting simple PDF:', error);
    } finally {
      this.isExporting.set(false);
    }
  }

  private extractTextContent(element: HTMLElement): string {
    let text = '';
    
    const childNodes = Array.from(element.childNodes);
    for (const child of childNodes) {
      if (child.nodeType === Node.TEXT_NODE) {
        text += child.textContent?.trim() + ' ';
      } else if (child.nodeType === Node.ELEMENT_NODE) {
        const el = child as HTMLElement;
        
        // Add spacing for headers
        if (['H1', 'H2', 'H3', 'H4', 'H5', 'H6'].includes(el.tagName)) {
          text += '\n\n' + el.textContent?.trim() + '\n';
        }
        // Add spacing for paragraphs
        else if (el.tagName === 'P') {
          text += '\n' + el.textContent?.trim() + '\n';
        }
        // Add bullet points for list items
        else if (el.tagName === 'LI') {
          text += '\n• ' + el.textContent?.trim();
        }
        // Add line breaks for HR
        else if (el.tagName === 'HR') {
          text += '\n' + '-'.repeat(50) + '\n';
        }
        // Recursively process other elements
        else {
          text += this.extractTextContent(el);
        }
      }
    }
    
    return text.replace(/\n\s*\n\s*\n/g, '\n\n'); // Clean up extra newlines
  }
}
