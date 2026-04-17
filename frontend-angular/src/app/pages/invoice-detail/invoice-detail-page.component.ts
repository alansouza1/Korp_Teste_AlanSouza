import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { EMPTY, Observable, Subject, catchError, distinctUntilChanged, filter, finalize, map, shareReplay, startWith, switchMap, tap } from 'rxjs';

import { Invoice } from '../../core/models/invoice.models';
import { InvoicesApiService } from '../../core/services/invoices-api.service';
import { getApiErrorMessage } from '../../core/services/api-error.util';

@Component({
  selector: 'app-invoice-detail-page',
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatChipsModule
  ],
  templateUrl: './invoice-detail-page.component.html',
  styleUrl: './invoice-detail-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoiceDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly invoicesApi = inject(InvoicesApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);
  private readonly refreshSubject = new Subject<void>();

  readonly displayedColumns = ['productCode', 'productDescription', 'quantity'];

  readonly addItemForm = this.fb.nonNullable.group({
    productCode: ['', [Validators.required, Validators.maxLength(50)]],
    productDescription: ['', [Validators.required, Validators.maxLength(255)]],
    quantity: [1, [Validators.required, Validators.min(1)]]
  });

  readonly invoiceId$ = this.route.paramMap.pipe(
    map((params) => params.get('id')),
    filter((id): id is string => !!id),
    distinctUntilChanged()
  );

  readonly invoice$: Observable<Invoice> = this.invoiceId$.pipe(
    switchMap((invoiceId) =>
      this.refreshSubject.pipe(
        startWith(void 0),
        switchMap(() =>
          this.invoicesApi.getInvoiceById(invoiceId).pipe(
            tap(() => (this.loading = false)),
            catchError((error) => {
              this.loading = false;
              this.openError(getApiErrorMessage(error, 'Unable to load invoice details.'));
              return EMPTY;
            })
          )
        )
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  loading = true;
  addingItem = false;
  printing = false;

  constructor() {
    this.invoiceId$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.loading = true;
        this.refreshSubject.next();
      });
  }

  addItem(invoice: Invoice): void {
    if (invoice.status === 'CLOSED') {
      return;
    }

    if (this.addItemForm.invalid) {
      this.addItemForm.markAllAsTouched();
      return;
    }

    this.addingItem = true;
    this.invoicesApi
      .addItems(invoice.id, {
        items: [this.addItemForm.getRawValue()]
      })
      .pipe(
        finalize(() => (this.addingItem = false)),
        catchError((error) => {
          this.openError(getApiErrorMessage(error, 'Unable to add invoice item.'));
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.addItemForm.reset({ productCode: '', productDescription: '', quantity: 1 });
        this.openSuccess('Item added to invoice.');
        this.invoicesApi.requestRefresh();
        this.refreshSubject.next();
      });
  }

  printInvoice(invoice: Invoice): void {
    if (invoice.status === 'CLOSED') {
      return;
    }

    this.printing = true;
    this.invoicesApi
      .printInvoice(invoice.id)
      .pipe(
        finalize(() => (this.printing = false)),
        catchError((error) => {
          this.openError(
            getApiErrorMessage(error, 'Unable to print invoice right now. Please try again later.')
          );
          this.invoicesApi.requestRefresh();
          this.refreshSubject.next();
          return EMPTY;
        })
      )
      .subscribe((response) => {
        this.openSuccess(response.message);
        this.invoicesApi.requestRefresh();
        this.refreshSubject.next();
      });
  }

  private openSuccess(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 3500 });
  }

  private openError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 5000, panelClass: ['snackbar-error'] });
  }
}
