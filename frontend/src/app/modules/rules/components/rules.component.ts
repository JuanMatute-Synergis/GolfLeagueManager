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
      // Use a simple approach: create PDF directly from text without html2canvas
      const pdf = new jsPDF('p', 'mm', 'a4');
      const pageWidth = 210;
      const pageHeight = 297;
      const margin = 20;
      const contentWidth = pageWidth - (margin * 2);
      const lineHeight = 7;
      const maxLinesPerPage = Math.floor((pageHeight - (margin * 2)) / lineHeight) - 3;
      
      // Clean the HTML content to extract structured text
      const tempDiv = document.createElement('div');
      tempDiv.innerHTML = this.rules()?.content || '';
      
      // Extract structured content with formatting
      const structuredContent = this.extractStructuredContent(tempDiv);
      
      let y = margin;
      let lineCount = 0;
      let pageNumber = 1;
      
      // Add title
      pdf.setFontSize(20);
      pdf.setFont('helvetica', 'bold');
      pdf.text('League Rules', margin, y);
      y += lineHeight * 2;
      lineCount += 2;
      
      // Process each content item
      for (const item of structuredContent) {
        // Check if we need a new page
        const linesNeeded = item.type.startsWith('h') ? 3 : item.lines.length + 1;
        if (lineCount + linesNeeded > maxLinesPerPage) {
          // Add page number to current page
          pdf.setFontSize(10);
          pdf.setFont('helvetica', 'normal');
          pdf.text(`Page ${pageNumber}`, pageWidth - margin - 15, pageHeight - 10);
          
          pdf.addPage();
          y = margin;
          lineCount = 0;
          pageNumber++;
        }
        
        // Set font based on item type
        switch (item.type) {
          case 'h1':
            pdf.setFontSize(18);
            pdf.setFont('helvetica', 'bold');
            y += lineHeight; // Extra space before h1
            lineCount++;
            break;
          case 'h2':
            pdf.setFontSize(16);
            pdf.setFont('helvetica', 'bold');
            y += lineHeight * 0.5; // Space before h2
            lineCount += 0.5;
            break;
          case 'h3':
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            break;
          case 'list':
            pdf.setFontSize(12);
            pdf.setFont('helvetica', 'normal');
            break;
          default:
            pdf.setFontSize(12);
            pdf.setFont('helvetica', 'normal');
        }
        
        // Add the content lines
        for (const line of item.lines) {
          if (lineCount >= maxLinesPerPage) {
            // Add page number to current page
            pdf.setFontSize(10);
            pdf.setFont('helvetica', 'normal');
            pdf.text(`Page ${pageNumber}`, pageWidth - margin - 15, pageHeight - 10);
            
            pdf.addPage();
            y = margin;
            lineCount = 0;
            pageNumber++;
            
            // Reset font for content continuation
            switch (item.type) {
              case 'h1':
                pdf.setFontSize(18);
                pdf.setFont('helvetica', 'bold');
                break;
              case 'h2':
                pdf.setFontSize(16);
                pdf.setFont('helvetica', 'bold');
                break;
              case 'h3':
                pdf.setFontSize(14);
                pdf.setFont('helvetica', 'bold');
                break;
              case 'list':
                pdf.setFontSize(12);
                pdf.setFont('helvetica', 'normal');
                break;
              default:
                pdf.setFontSize(12);
                pdf.setFont('helvetica', 'normal');
            }
          }
          
          // Split long lines that might exceed page width
          const wrappedLines = pdf.splitTextToSize(line, contentWidth);
          for (const wrappedLine of wrappedLines) {
            if (lineCount >= maxLinesPerPage) {
              // Add page number to current page
              pdf.setFontSize(10);
              pdf.setFont('helvetica', 'normal');
              pdf.text(`Page ${pageNumber}`, pageWidth - margin - 15, pageHeight - 10);
              
              pdf.addPage();
              y = margin;
              lineCount = 0;
              pageNumber++;
              
              // Reset font for content continuation
              switch (item.type) {
                case 'h1':
                  pdf.setFontSize(18);
                  pdf.setFont('helvetica', 'bold');
                  break;
                case 'h2':
                  pdf.setFontSize(16);
                  pdf.setFont('helvetica', 'bold');
                  break;
                case 'h3':
                  pdf.setFontSize(14);
                  pdf.setFont('helvetica', 'bold');
                  break;
                case 'list':
                  pdf.setFontSize(12);
                  pdf.setFont('helvetica', 'normal');
                  break;
                default:
                  pdf.setFontSize(12);
                  pdf.setFont('helvetica', 'normal');
              }
            }
            
            pdf.text(wrappedLine, margin, y);
            y += lineHeight;
            lineCount++;
          }
        }
        
        // Add extra spacing after headers
        if (item.type.startsWith('h')) {
          y += lineHeight * 0.5;
          lineCount += 0.5;
        }
      }
      
      // Add page number to final page
      pdf.setFontSize(10);
      pdf.setFont('helvetica', 'normal');
      pdf.text(`Page ${pageNumber}`, pageWidth - margin - 15, pageHeight - 10);
      
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
      // Create PDF with proper margins and page breaks
      const pdf = new jsPDF('p', 'mm', 'a4');
      const pageWidth = 210;
      const pageHeight = 297;
      const margin = 20; // 20mm margins
      const contentWidth = pageWidth - (margin * 2); // 170mm content width
      const lineHeight = 6;
      const maxLinesPerPage = Math.floor((pageHeight - (margin * 2)) / lineHeight) - 2; // Leave space at bottom
      
      // Clean the HTML content to extract structured text
      const tempDiv = document.createElement('div');
      tempDiv.innerHTML = this.rules()?.content || '';
      
      // Extract structured content with formatting
      const structuredContent = this.extractStructuredContent(tempDiv);
      
      let currentPage = 1;
      let y = margin;
      let lineCount = 0;
      
      // Add title
      pdf.setFontSize(18);
      pdf.setFont('helvetica', 'bold');
      pdf.text('League Rules', margin, y);
      y += lineHeight * 2;
      lineCount += 2;
      
      // Process each content item
      structuredContent.forEach((item: StructuredContentItem) => {
        // Check if we need a new page
        const linesNeeded = item.type.startsWith('h') ? 3 : Math.ceil(item.lines.length);
        if (lineCount + linesNeeded > maxLinesPerPage) {
          pdf.addPage();
          y = margin;
          lineCount = 0;
          currentPage++;
        }
        
        // Set font based on item type
        switch (item.type) {
          case 'h1':
            pdf.setFontSize(16);
            pdf.setFont('helvetica', 'bold');
            y += lineHeight; // Extra space before h1
            lineCount++;
            break;
          case 'h2':
            pdf.setFontSize(14);
            pdf.setFont('helvetica', 'bold');
            y += lineHeight * 0.5; // Space before h2
            lineCount += 0.5;
            break;
          case 'h3':
            pdf.setFontSize(12);
            pdf.setFont('helvetica', 'bold');
            break;
          case 'list':
            pdf.setFontSize(11);
            pdf.setFont('helvetica', 'normal');
            break;
          default:
            pdf.setFontSize(11);
            pdf.setFont('helvetica', 'normal');
        }
        
        // Add the content
        item.lines.forEach((line: string, index: number) => {
          if (lineCount >= maxLinesPerPage) {
            pdf.addPage();
            y = margin;
            lineCount = 0;
            currentPage++;
          }
          
          pdf.text(line, margin, y);
          y += lineHeight;
          lineCount++;
        });
        
        // Add extra spacing after headers
        if (item.type.startsWith('h')) {
          y += lineHeight * 0.5;
          lineCount += 0.5;
        }
      });
      
      // Add page numbers using getNumberOfPages method
      const totalPages = (pdf as any).internal.getNumberOfPages();
      for (let i = 1; i <= totalPages; i++) {
        pdf.setPage(i);
        pdf.setFontSize(10);
        pdf.setFont('helvetica', 'normal');
        pdf.text(`Page ${i} of ${totalPages}`, pageWidth - margin - 20, pageHeight - 10);
      }
      
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
        text += this.cleanTextForPdf(child.textContent?.trim()) + ' ';
      } else if (child.nodeType === Node.ELEMENT_NODE) {
        const el = child as HTMLElement;
        
        // Add spacing for headers
        if (['H1', 'H2', 'H3', 'H4', 'H5', 'H6'].includes(el.tagName)) {
          text += '\n\n' + this.cleanTextForPdf(el.textContent?.trim()) + '\n';
        }
        // Add spacing for paragraphs
        else if (el.tagName === 'P') {
          text += '\n' + this.cleanTextForPdf(el.textContent?.trim()) + '\n';
        }
        // Add bullet points for list items
        else if (el.tagName === 'LI') {
          text += '\n• ' + this.cleanTextForPdf(el.textContent?.trim());
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

  private extractStructuredContent(element: HTMLElement): StructuredContentItem[] {
    const items: StructuredContentItem[] = [];
    
    const processElement = (el: HTMLElement) => {
      const childNodes = Array.from(el.childNodes);
      
      for (const child of childNodes) {
        if (child.nodeType === Node.ELEMENT_NODE) {
          const childEl = child as HTMLElement;
          const tagName = childEl.tagName.toLowerCase();
          
          switch (tagName) {
            case 'h1':
              const h1Text = this.cleanTextForPdf(childEl.textContent?.trim());
              if (h1Text) {
                items.push({
                  type: 'h1',
                  lines: [h1Text]
                });
              }
              break;
            case 'h2':
              const h2Text = this.cleanTextForPdf(childEl.textContent?.trim());
              if (h2Text) {
                items.push({
                  type: 'h2',
                  lines: [h2Text]
                });
              }
              break;
            case 'h3':
              const h3Text = this.cleanTextForPdf(childEl.textContent?.trim());
              if (h3Text) {
                items.push({
                  type: 'h3',
                  lines: [h3Text]
                });
              }
              break;
            case 'p':
              const pText = this.cleanTextForPdf(childEl.textContent?.trim());
              if (pText) {
                items.push({
                  type: 'p',
                  lines: [pText]
                });
              }
              break;
            case 'ul':
            case 'ol':
              const listItems = Array.from(childEl.querySelectorAll('li'));
              const listLines = listItems
                .map(li => this.cleanTextForPdf(li.textContent?.trim()))
                .filter(text => text)
                .map(text => '• ' + text);
              if (listLines.length > 0) {
                items.push({
                  type: 'list',
                  lines: listLines
                });
              }
              break;
            case 'hr':
              items.push({
                type: 'hr',
                lines: ['─'.repeat(60)]
              });
              break;
            default:
              // Recursively process nested elements
              processElement(childEl);
          }
        }
      }
    };
    
    processElement(element);
    return items;
  }

  private cleanTextForPdf(text: string | undefined): string {
    if (!text) return '';
    
    // Replace common emojis with text equivalents
    const emojiReplacements: { [key: string]: string } = {
      '🏌️': '[Golf]',
      '🏌️‍♂️': '[Golfer]',
      '⚡': '[Lightning]',
      '📋': '[Overview]',
      '📊': '[Example]',
      '🚫': '[Absence]',
      '📈': '[Season]',
      '⚖️': '[Fair Play]',
      '📅': '[Administrative]',
      '🏆': '[Season/Trophy]',
      '💵': '[Weekly Prizes]',
      '💰': '[Prize Pool]',
      '🎯': '',
      '✅': '[Check]',
      '❌': '[X]',
      '📄': '[Document]',
      '📑': '[Pages]',
      '🖨️': '[Print]',
      '📖': '[Book]',
      '📝': '[Notes]',
      '🎉': '[Celebration]',
      '💪': '[Strong]',
      '&amp;': '&'
    };
    
    let cleanedText = text;
    
    // Replace emojis with text equivalents
    for (const [emoji, replacement] of Object.entries(emojiReplacements)) {
      cleanedText = cleanedText.replace(new RegExp(emoji, 'g'), replacement);
    }
    
    // Remove any remaining emoji characters (anything in Unicode emoji ranges)
    cleanedText = cleanedText.replace(/[\u{1F600}-\u{1F64F}]|[\u{1F300}-\u{1F5FF}]|[\u{1F680}-\u{1F6FF}]|[\u{1F1E0}-\u{1F1FF}]|[\u{2600}-\u{26FF}]|[\u{2700}-\u{27BF}]/gu, '');
    
    // Clean up extra spaces
    cleanedText = cleanedText.replace(/\s+/g, ' ').trim();
    
    return cleanedText;
  }
}

interface StructuredContentItem {
  type: 'h1' | 'h2' | 'h3' | 'p' | 'list' | 'hr';
  lines: string[];
}
