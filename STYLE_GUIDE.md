# Golf League Manager - Frontend Style Guide

## Overview

This style guide establishes consistent design patterns, component structure, and styling conventions for the Golf League Manager project. The project uses Angular with Tailwind CSS v4 and follows a modern, clean design system with theming support.

## Table of Contents

1. [Design System Foundation](#design-system-foundation)
2. [Color System & Theming](#color-system--theming)
3. [Typography](#typography)
4. [Component Architecture](#component-architecture)
5. [Layout Patterns](#layout-patterns)
6. [Form Components](#form-components)
7. [Button Components](#button-components)
8. [Navigation Components](#navigation-components)
9. [Card Components](#card-components)
10. [Interactive Components](#interactive-components)
11. [Responsive Design](#responsive-design)
12. [Accessibility Guidelines](#accessibility-guidelines)
13. [Animation & Transitions](#animation--transitions)
14. [Code Style Guidelines](#code-style-guidelines)

---

## Design System Foundation

### Core Technologies
- **Angular**: Standalone components with TypeScript
- **Tailwind CSS v4**: Utility-first CSS framework
- **CSS Custom Properties**: For theming and dynamic styling
- **Angular SVG Icons**: For consistent iconography

### Design Principles
- **Consistency**: Use established patterns and components
- **Accessibility**: Ensure WCAG 2.1 AA compliance
- **Responsiveness**: Mobile-first design approach
- **Performance**: Minimize custom CSS, leverage Tailwind utilities
- **Maintainability**: Clear component structure and naming conventions

---

## Color System & Theming

### CSS Custom Properties (Design Tokens)

The project uses a semantic color system based on CSS custom properties:

```css
:root {
  --background: #FFFFFF;
  --foreground: #0C1420;
  --card: #F1F5F9;
  --card-foreground: #000000;
  --primary: #E11D48;
  --primary-foreground: #FFFFFF;
  --muted: #CFD9E5;
  --muted-foreground: #64748B;
  --destructive: #CC0033;
  --destructive-foreground: #FAFAFA;
  --border: #E2E8F0;
}
```

### Theme Support

The application supports multiple color themes and dark mode:

**Available Themes:**
- Base (red): `#E11D48`
- Blue: `#2490FF`
- Green: `#22C55E`
- Orange: `#EA580C`
- Yellow: `#FACC15`
- Violet: `#6E56CF`

**Usage in Components:**
```html
<!-- Use semantic color classes -->
<div class="bg-background text-foreground border-border">
  <h1 class="text-primary">Title</h1>
  <p class="text-muted-foreground">Description</p>
</div>
```

---

## Typography

### Font System

**Primary Font**: Poppins (Google Fonts)
```css
font-family: Poppins, system-ui, sans-serif;
```

### Typography Scale

**Headings:**
```html
<h1 class="text-3xl font-semibold text-foreground">Page Title</h1>
<h2 class="text-2xl font-semibold text-foreground">Section Title</h2>
<h3 class="text-xl font-semibold text-foreground">Subsection Title</h3>
<h4 class="text-lg font-medium text-foreground">Component Title</h4>
```

**Body Text:**
```html
<p class="text-base text-foreground">Regular body text</p>
<p class="text-sm text-muted-foreground">Secondary text</p>
<small class="text-xs text-muted-foreground">Captions and labels</small>
```

**Text Colors:**
- Primary content: `text-foreground`
- Secondary content: `text-muted-foreground`
- Interactive elements: `text-primary`
- Error states: `text-destructive`

---

## Component Architecture

### Component Structure

All components should follow Angular standalone component architecture:

```typescript
@Component({
  selector: 'app-component-name',
  standalone: true,
  imports: [CommonModule, /* other imports */],
  templateUrl: './component-name.component.html',
  styleUrls: ['./component-name.component.css']
})
export class ComponentNameComponent implements OnInit {
  // Component logic
}
```

### File Organization
```
component-name/
├── component-name.component.ts
├── component-name.component.html
├── component-name.component.css
└── component-name.component.spec.ts
```

### Naming Conventions
- **Components**: PascalCase (e.g., `SeasonSettingsComponent`)
- **Files**: kebab-case (e.g., `season-settings.component.ts`)
- **CSS classes**: Use Tailwind utilities primarily
- **Custom CSS**: BEM methodology if needed

---

## Layout Patterns

### Main Layout Structure

```html
<div class="flex h-screen w-full overflow-hidden">
  <!-- Sidebar -->
  <app-sidebar></app-sidebar>
  
  <div class="flex grow flex-col content-start overflow-hidden bg-card">
    <!-- Header -->
    <app-navbar></app-navbar>
    
    <!-- Main Content -->
    <div id="main-content" class="flex-1 overflow-hidden">
      <div class="h-full mx-auto px-4 py-4 sm:px-8 lg:container">
        <router-outlet></router-outlet>
      </div>
    </div>
    
    <!-- Footer -->
    <app-footer></app-footer>
  </div>
</div>
```

### Page Layout Pattern

```html
<div class="h-full flex flex-col">
  <!-- Page Header -->
  <div class="mb-8 flex-shrink-0">
    <h2 class="text-3xl font-semibold text-foreground mb-2">Page Title</h2>
    <p class="text-muted-foreground">Page description</p>
  </div>

  <!-- Error/Success Messages -->
  <div class="bg-destructive/10 border border-destructive/20 text-destructive p-4 rounded-md mb-6 flex justify-between items-center flex-shrink-0" *ngIf="error">
    {{ error }}
    <button class="bg-muted text-muted-foreground px-3 py-1 rounded text-sm hover:bg-muted/80" (click)="error = null">
      Dismiss
    </button>
  </div>

  <!-- Main Content -->
  <div class="flex-1 min-h-0">
    <!-- Content goes here -->
  </div>
</div>
```

### Grid Layouts

**Dashboard Cards:**
```html
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
  <!-- Card items -->
</div>
```

**Form Layouts:**
```html
<!-- Two-column form -->
<div class="grid grid-cols-1 md:grid-cols-2 gap-4">
  <!-- Form fields -->
</div>

<!-- Three-column form -->
<div class="grid grid-cols-1 md:grid-cols-3 gap-4">
  <!-- Form fields -->
</div>
```

---

## Form Components

### Input Fields

**Standard Input:**
```html
<div class="space-y-2">
  <label for="input-id" class="block text-sm font-medium text-foreground">
    Label Text
  </label>
  <input 
    type="text" 
    id="input-id"
    class="w-full px-4 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20 focus:outline-none"
    placeholder="Placeholder text"
  />
</div>
```

**Select Dropdown:**
```html
<div class="space-y-2">
  <label for="select-id" class="block text-sm font-medium text-foreground">
    Select Label
  </label>
  <select 
    id="select-id"
    class="w-full px-4 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20"
  >
    <option value="">Select option</option>
    <option value="value1">Option 1</option>
  </select>
</div>
```

**Date Input with Custom Styling:**
```html
<div class="space-y-2">
  <label for="date-id" class="block text-sm font-medium text-foreground">
    Date Label
  </label>
  <div class="date-input-wrapper">
    <input 
      type="date" 
      id="date-id"
      class="w-full px-4 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20"
    />
  </div>
</div>
```

### Form Validation

**Error States:**
```html
<div class="space-y-2">
  <label class="block text-sm font-medium text-foreground">Label</label>
  <input 
    class="w-full px-4 py-2 border rounded-md bg-background text-foreground"
    [class.border-destructive]="hasError"
    [class.border-border]="!hasError"
  />
  <p class="text-destructive text-sm" *ngIf="hasError">
    Error message here
  </p>
</div>
```

### Form Groups

```html
<form class="space-y-6">
  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
    <!-- Form fields -->
  </div>
  
  <div class="flex gap-3 pt-4 border-t border-border">
    <button type="submit" class="bg-primary text-primary-foreground px-6 py-2 rounded-md font-medium hover:bg-primary/90 transition-colors">
      Save
    </button>
    <button type="button" class="bg-muted text-muted-foreground px-6 py-2 rounded-md font-medium hover:bg-muted/80 transition-colors">
      Cancel
    </button>
  </div>
</form>
```

---

## Button Components

### Button Styles

The project includes a comprehensive button component with multiple variants:

**Primary Button:**
```html
<app-button impact="bold" tone="primary" size="medium" shape="rounded">
  Primary Action
</app-button>
```

**Secondary Button:**
```html
<app-button impact="light" tone="primary" size="medium" shape="rounded">
  Secondary Action
</app-button>
```

**Danger Button:**
```html
<app-button impact="bold" tone="danger" size="medium" shape="rounded">
  Delete
</app-button>
```

### Button Properties

```typescript
type ButtonProps = {
  impact: 'bold' | 'light' | 'none';
  size: 'small' | 'medium' | 'large';
  shape: 'square' | 'rounded' | 'pill';
  tone: 'primary' | 'danger' | 'success' | 'warning' | 'info' | 'light';
  shadow: 'none' | 'small' | 'medium' | 'large';
  type: 'button' | 'submit' | 'reset';
};
```

### Direct Button Styling (Alternative)

```html
<!-- Primary -->
<button class="bg-primary text-primary-foreground px-6 py-2 rounded-md font-medium hover:bg-primary/90 transition-colors">
  Primary Button
</button>

<!-- Secondary -->
<button class="bg-muted text-muted-foreground px-6 py-2 rounded-md font-medium hover:bg-muted/80 transition-colors">
  Secondary Button
</button>

<!-- Destructive -->
<button class="bg-destructive text-destructive-foreground px-6 py-2 rounded-md font-medium hover:bg-destructive/90 transition-colors">
  Delete
</button>

<!-- Ghost -->
<button class="text-primary hover:bg-primary/10 px-6 py-2 rounded-md font-medium transition-colors">
  Ghost Button
</button>
```

---

## Navigation Components

### Sidebar Menu

```html
<div class="text-muted-foreground hover:text-foreground group relative flex grow items-center gap-4 rounded-lg px-2">
  <div class="text-muted-foreground/50">
    <svg-icon src="icon-path.svg" [svgClass]="'h-5 w-5'"></svg-icon>
  </div>
  <a routerLink="/path" routerLinkActive="text-primary" class="truncate text-xs font-semibold tracking-wide">
    Menu Item
  </a>
</div>
```

### Mobile Navigation Tabs

```html
<div class="flex bg-muted rounded-lg p-1">
  <button 
    class="flex-1 py-2 px-3 text-sm font-medium rounded-md transition-colors"
    [class.bg-background]="activeTab === 'tab1'"
    [class.text-foreground]="activeTab === 'tab1'"
    [class.text-muted-foreground]="activeTab !== 'tab1'"
    (click)="setActiveTab('tab1')"
  >
    Tab 1
  </button>
  <button 
    class="flex-1 py-2 px-3 text-sm font-medium rounded-md transition-colors"
    [class.bg-background]="activeTab === 'tab2'"
    [class.text-foreground]="activeTab === 'tab2'"
    [class.text-muted-foreground]="activeTab !== 'tab2'"
    (click)="setActiveTab('tab2')"
  >
    Tab 2
  </button>
</div>
```

### Breadcrumbs

```html
<nav class="text-sm text-muted-foreground mb-4">
  <ol class="flex items-center space-x-2">
    <li><a href="/dashboard" class="hover:text-primary">Dashboard</a></li>
    <li><span class="mx-2">/</span></li>
    <li><a href="/settings" class="hover:text-primary">Settings</a></li>
    <li><span class="mx-2">/</span></li>
    <li class="text-foreground">Current Page</li>
  </ol>
</nav>
```

---

## Card Components

### Basic Card

```html
<div class="bg-card border border-border rounded-lg shadow-sm overflow-hidden">
  <div class="p-6">
    <h3 class="text-lg font-semibold text-foreground mb-2">Card Title</h3>
    <p class="text-muted-foreground">Card content goes here.</p>
  </div>
</div>
```

### Card with Header

```html
<div class="bg-card border border-border rounded-lg shadow-sm overflow-hidden">
  <div class="p-4 border-b border-border bg-muted/20">
    <div class="flex justify-between items-center">
      <h3 class="text-lg font-semibold text-foreground">Card Title</h3>
      <button class="bg-primary text-primary-foreground px-3 py-1.5 rounded text-sm font-medium">
        Action
      </button>
    </div>
  </div>
  <div class="p-6">
    <!-- Card content -->
  </div>
</div>
```

### Dashboard Card

```html
<div class="bg-card hover:shadow-lg rounded-2xl shadow-sm p-8 text-center cursor-pointer transition-all duration-200 border border-border">
  <svg-icon src="icon-path.svg" [svgClass]="'w-12 h-12 mx-auto mb-4 text-muted-foreground'"></svg-icon>
  <h2 class="text-foreground text-xl font-semibold mb-2">Card Title</h2>
  <p class="text-muted-foreground text-base">Card description</p>
</div>
```

---

## Interactive Components

### Loading States

```html
<!-- Button loading state -->
<button class="bg-primary text-primary-foreground px-6 py-2 rounded-md font-medium inline-flex items-center" [disabled]="loading">
  <svg *ngIf="loading" class="animate-spin -ml-1 mr-3 h-5 w-5" fill="none" viewBox="0 0 24 24">
    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
  </svg>
  {{ loading ? 'Loading...' : 'Submit' }}
</button>

<!-- Page loading spinner -->
<div class="flex items-center justify-center p-8">
  <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
</div>
```

### Modal/Dialog

```html
<div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50" *ngIf="showModal">
  <div class="bg-background rounded-lg shadow-xl max-w-md w-full">
    <div class="p-6">
      <div class="flex justify-between items-center mb-4">
        <h3 class="text-lg font-semibold text-foreground">Modal Title</h3>
        <button (click)="closeModal()" class="text-muted-foreground hover:text-foreground">
          <svg-icon src="close-icon.svg" [svgClass]="'h-5 w-5'"></svg-icon>
        </button>
      </div>
      <div class="space-y-4">
        <!-- Modal content -->
      </div>
      <div class="flex gap-3 mt-6">
        <button class="bg-primary text-primary-foreground px-4 py-2 rounded-md font-medium flex-1">
          Confirm
        </button>
        <button class="bg-muted text-muted-foreground px-4 py-2 rounded-md font-medium flex-1" (click)="closeModal()">
          Cancel
        </button>
      </div>
    </div>
  </div>
</div>
```

### Dropdown Menu

```html
<div class="relative inline-block text-left">
  <button (click)="toggleDropdown()" class="inline-flex justify-center w-full px-4 py-2 text-sm font-medium text-foreground bg-background border border-border rounded-md hover:bg-muted focus:outline-none">
    Options
    <svg-icon src="chevron-down.svg" [svgClass]="'ml-2 h-4 w-4'"></svg-icon>
  </button>

  <div *ngIf="showDropdown" class="origin-top-right absolute right-0 mt-2 w-56 rounded-md shadow-lg bg-background ring-1 ring-border">
    <div class="py-1">
      <a href="#" class="block px-4 py-2 text-sm text-foreground hover:bg-muted">Option 1</a>
      <a href="#" class="block px-4 py-2 text-sm text-foreground hover:bg-muted">Option 2</a>
    </div>
  </div>
</div>
```

---

## Responsive Design

### Breakpoint Strategy

Use Tailwind's responsive prefixes:
- `sm:` - 640px and up
- `md:` - 768px and up  
- `lg:` - 1024px and up
- `xl:` - 1280px and up
- `2xl:` - 1536px and up

### Mobile-First Approach

```html
<!-- Mobile-first grid -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
  <!-- Items -->
</div>

<!-- Mobile-first padding -->
<div class="px-4 sm:px-6 lg:px-8">
  <!-- Content -->
</div>

<!-- Responsive text sizes -->
<h1 class="text-2xl md:text-3xl lg:text-4xl font-bold">
  Responsive Heading
</h1>
```

### Mobile Navigation Pattern

```html
<!-- Desktop navigation (hidden on mobile) -->
<div class="hidden md:flex space-x-4">
  <!-- Navigation items -->
</div>

<!-- Mobile navigation tabs -->
<div class="md:hidden">
  <div class="flex bg-muted rounded-lg p-1">
    <!-- Mobile tabs -->
  </div>
</div>

<!-- Mobile-specific layouts -->
<div class="lg:hidden">
  <!-- Mobile layout -->
</div>

<div class="hidden lg:flex">
  <!-- Desktop layout -->
</div>
```

---

## Accessibility Guidelines

### Semantic HTML

```html
<!-- Use proper heading hierarchy -->
<h1>Page Title</h1>
<h2>Section Title</h2>
<h3>Subsection Title</h3>

<!-- Use semantic elements -->
<main>
  <article>
    <section>
      <!-- Content -->
    </section>
  </article>
</main>

<nav>
  <!-- Navigation -->
</nav>

<aside>
  <!-- Sidebar content -->
</aside>
```

### Form Accessibility

```html
<!-- Always associate labels with inputs -->
<div>
  <label for="email-input" class="block text-sm font-medium text-foreground">
    Email Address
  </label>
  <input 
    type="email" 
    id="email-input"
    name="email"
    aria-describedby="email-help"
    class="form-input"
    required
  />
  <p id="email-help" class="text-xs text-muted-foreground mt-1">
    We'll never share your email
  </p>
</div>

<!-- Error states with ARIA -->
<div>
  <label for="password-input">Password</label>
  <input 
    type="password" 
    id="password-input"
    aria-invalid="true"
    aria-describedby="password-error"
    class="form-input border-destructive"
  />
  <p id="password-error" class="text-destructive text-sm" role="alert">
    Password is required
  </p>
</div>
```

### Focus Management

```html
<!-- Visible focus indicators -->
<button class="bg-primary text-primary-foreground px-4 py-2 rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2">
  Accessible Button
</button>

<!-- Skip links -->
<a href="#main-content" class="sr-only focus:not-sr-only focus:absolute focus:top-4 focus:left-4 bg-primary text-primary-foreground px-4 py-2 rounded-md">
  Skip to main content
</a>
```

### ARIA Labels and Roles

```html
<!-- Loading states -->
<button aria-label="Loading, please wait" disabled>
  <span class="sr-only">Loading</span>
  <svg class="animate-spin h-5 w-5" aria-hidden="true">...</svg>
</button>

<!-- Navigation -->
<nav aria-label="Main navigation">
  <ul>
    <li><a href="/dashboard" aria-current="page">Dashboard</a></li>
    <li><a href="/settings">Settings</a></li>
  </ul>
</nav>

<!-- Modal dialogs -->
<div role="dialog" aria-labelledby="modal-title" aria-modal="true">
  <h2 id="modal-title">Modal Title</h2>
  <!-- Modal content -->
</div>
```

---

## Animation & Transitions

### CSS Transitions

All interactive elements should have smooth transitions:

```html
<!-- Button hover effects -->
<button class="bg-primary text-primary-foreground px-4 py-2 rounded-md transition-colors duration-200 hover:bg-primary/90">
  Hover Me
</button>

<!-- Card hover effects -->
<div class="bg-card border border-border rounded-lg shadow-sm transition-all duration-200 hover:shadow-lg">
  <!-- Card content -->
</div>

<!-- Focus transitions -->
<input class="border border-border rounded-md px-3 py-2 transition-all duration-200 focus:border-primary focus:ring-2 focus:ring-primary/20">
```

### Custom Animations

Pre-defined animations available:

```html
<!-- Fade animations -->
<div class="animate-fade-in-up">Content</div>
<div class="animate-fade-in-down">Content</div>
<div class="animate-fade-out-up">Content</div>
<div class="animate-fade-out-down">Content</div>

<!-- Loading spinner -->
<div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>

<!-- Wiggle animation -->
<div class="animate-wiggle">Wiggling element</div>
```

### Page Transitions

```html
<!-- Mobile menu transitions -->
<div [ngClass]="
  showMobileMenu
    ? 'animate-fade-in-up pointer-events-auto scale-100 opacity-100 duration-200'
    : 'pointer-events-none scale-95 opacity-0 duration-100 ease-out'
">
  <!-- Mobile menu content -->
</div>
```

---

## Code Style Guidelines

### Component Development

**1. Component Structure:**
```typescript
@Component({
  selector: 'app-component-name',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './component-name.component.html',
  styleUrls: ['./component-name.component.css']
})
export class ComponentNameComponent implements OnInit {
  // Public properties first
  public loading = false;
  public error: string | null = null;
  
  // Private properties
  private subscription$ = new Subject<void>();
  
  // Constructor
  constructor(private service: SomeService) {}
  
  // Lifecycle hooks
  ngOnInit(): void {
    this.loadData();
  }
  
  ngOnDestroy(): void {
    this.subscription$.next();
    this.subscription$.complete();
  }
  
  // Public methods
  public loadData(): void {
    // Implementation
  }
  
  // Private methods
  private handleError(error: any): void {
    // Implementation
  }
}
```

**2. Template Structure:**
```html
<!-- Comments for major sections -->

<!-- Error/Loading States -->
<div *ngIf="loading" class="flex items-center justify-center p-8">
  <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
</div>

<div *ngIf="error" class="bg-destructive/10 border border-destructive/20 text-destructive p-4 rounded-md mb-6">
  {{ error }}
</div>

<!-- Main Content -->
<div *ngIf="!loading" class="space-y-6">
  <!-- Content sections -->
</div>
```

**3. Styling Approach:**
- **Primary**: Use Tailwind utility classes
- **Custom CSS**: Only when Tailwind utilities are insufficient
- **Component-specific styles**: Use CSS custom properties for dynamic values
- **Avoid**: Inline styles except for dynamic binding

**4. State Management:**
```typescript
// Use TypeScript interfaces for type safety
interface ComponentState {
  loading: boolean;
  error: string | null;
  data: any[];
}

// Use reactive forms for complex forms
public form = this.fb.group({
  name: ['', Validators.required],
  email: ['', [Validators.required, Validators.email]]
});
```

### CSS Guidelines

**1. Custom CSS Structure:**
```css
/* Component-specific overrides only */
.component-specific-class {
  /* Use CSS custom properties for theme integration */
  background-color: hsl(var(--primary) / 0.1);
  border-color: hsl(var(--border));
}

/* Responsive overrides */
@media (max-width: 768px) {
  .mobile-specific {
    /* Mobile-specific styles */
  }
}
```

**2. Class Naming:**
- Use Tailwind utilities first
- BEM methodology for custom classes
- Descriptive, not prescriptive names

### TypeScript Guidelines

**1. Type Definitions:**
```typescript
// Define interfaces for all data structures
interface Player {
  id: string;
  name: string;
  email: string;
  handicap: number;
}

// Use union types for constrained values
type ButtonSize = 'small' | 'medium' | 'large';
type ButtonTone = 'primary' | 'secondary' | 'danger';
```

**2. Method Signatures:**
```typescript
// Clear parameter and return types
public loadPlayers(): Observable<Player[]> {
  return this.http.get<Player[]>('/api/players');
}

public updatePlayer(id: string, player: Partial<Player>): Observable<Player> {
  return this.http.put<Player>(`/api/players/${id}`, player);
}
```

---

## Development Workflow

### Component Creation Checklist

- [ ] Create component with standalone architecture
- [ ] Use semantic HTML structure
- [ ] Apply consistent styling patterns
- [ ] Implement error handling
- [ ] Add loading states
- [ ] Test responsive behavior
- [ ] Verify accessibility compliance
- [ ] Add TypeScript interfaces
- [ ] Write unit tests
- [ ] Document component usage

### Quality Assurance

**Before Committing:**
1. Run linting and formatting
2. Test responsive design
3. Verify accessibility with screen reader
4. Check color contrast ratios
5. Test keyboard navigation
6. Validate form behaviors
7. Test error states

### Design Review Process

**Component Review:**
1. Visual consistency with design system
2. Proper use of color tokens
3. Responsive behavior
4. Interactive state feedback
5. Loading and error states
6. Accessibility compliance

---

## Common Patterns & Examples

### Data Table Pattern

```html
<div class="bg-card border border-border rounded-lg shadow-sm overflow-hidden">
  <div class="overflow-x-auto">
    <table class="w-full">
      <thead class="bg-muted/20 border-b border-border">
        <tr>
          <th class="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
            Column 1
          </th>
          <th class="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
            Column 2
          </th>
        </tr>
      </thead>
      <tbody class="divide-y divide-border">
        <tr class="hover:bg-muted/10" *ngFor="let item of items">
          <td class="px-6 py-4 whitespace-nowrap text-sm text-foreground">
            {{ item.column1 }}
          </td>
          <td class="px-6 py-4 whitespace-nowrap text-sm text-foreground">
            {{ item.column2 }}
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</div>
```

### List with Actions Pattern

```html
<div class="space-y-2">
  <div *ngFor="let item of items" class="flex items-center justify-between p-4 bg-card border border-border rounded-lg">
    <div class="flex-1">
      <h3 class="text-sm font-medium text-foreground">{{ item.name }}</h3>
      <p class="text-xs text-muted-foreground">{{ item.description }}</p>
    </div>
    <div class="flex gap-2">
      <button class="text-primary hover:bg-primary/10 p-2 rounded" (click)="edit(item)">
        <svg-icon src="edit-icon.svg" [svgClass]="'h-4 w-4'"></svg-icon>
      </button>
      <button class="text-destructive hover:bg-destructive/10 p-2 rounded" (click)="delete(item)">
        <svg-icon src="delete-icon.svg" [svgClass]="'h-4 w-4'"></svg-icon>
      </button>
    </div>
  </div>
</div>
```

### Empty State Pattern

```html
<div class="text-center py-12">
  <svg-icon src="empty-state-icon.svg" [svgClass]="'w-16 h-16 mx-auto mb-4 text-muted-foreground/50'"></svg-icon>
  <h3 class="text-lg font-medium text-foreground mb-2">No items found</h3>
  <p class="text-muted-foreground mb-6">Get started by creating your first item.</p>
  <button class="bg-primary text-primary-foreground px-6 py-2 rounded-md font-medium">
    Create Item
  </button>
</div>
```

---

This style guide should be referenced when creating new components or updating existing ones to ensure consistency across the Golf League Manager application. Regular reviews and updates to this guide will help maintain design system integrity as the project evolves.
