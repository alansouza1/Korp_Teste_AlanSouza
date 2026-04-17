import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { EMPTY, Subject, catchError, finalize, merge, startWith, switchMap } from 'rxjs';

import { Invoice } from '../../core/models/invoice.models';
import { InvoicesApiService } from '../../core/services/invoices-api.service';
import { getApiErrorMessage } from '../../core/services/api-error.util';

@Component({
  selector: 'app-invoices-page',
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatSnackBarModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatChipsModule
  ],
  templateUrl: './invoices-page.component.html',
  styleUrl: './invoices-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoicesPageComponent {
  private readonly invoicesApi = inject(InvoicesApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly refreshSubject = new Subject<void>();

  readonly displayedColumns = ['number', 'status', 'items', 'attempts', 'updatedAt', 'actions'];

  readonly invoices$ = merge(this.refreshSubject, this.invoicesApi.refresh$).pipe(
    startWith(void 0),
    switchMap(() =>
      this.invoicesApi.getInvoices().pipe(
        finalize(() => (this.loading = false)),
        catchError((error) => {
          this.loading = false;
          this.openError(getApiErrorMessage(error, 'Unable to load invoices.'));
          return EMPTY;
        })
      )
    )
  );

  loading = true;
  creating = false;

  createInvoice(): void {
    this.creating = true;
    this.invoicesApi
      .createInvoice()
      .pipe(
        finalize(() => (this.creating = false)),
        catchError((error) => {
          this.openError(getApiErrorMessage(error, 'Unable to create invoice.'));
          return EMPTY;
        })
      )
      .subscribe((invoice) => {
        this.openSuccess(`Invoice #${invoice.sequentialNumber} created.`);
        this.refresh();
      });
  }

  refresh(): void {
    this.loading = true;
    this.refreshSubject.next();
  }

  trackByInvoiceId(_: number, invoice: Invoice): string {
    return invoice.id;
  }

  private openSuccess(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 3500 });
  }

  private openError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 5000, panelClass: ['snackbar-error'] });
  }
}
