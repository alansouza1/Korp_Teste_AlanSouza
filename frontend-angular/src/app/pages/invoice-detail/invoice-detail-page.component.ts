import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import {
  EMPTY,
  Observable,
  Subject,
  catchError,
  combineLatest,
  distinctUntilChanged,
  filter,
  finalize,
  map,
  of,
  shareReplay,
  startWith,
  switchMap,
  tap
} from 'rxjs';

import { Invoice } from '../../core/models/invoice.models';
import { InvoicesApiService } from '../../core/services/invoices-api.service';
import { getApiErrorMessage } from '../../core/services/api-error.util';
import { Product } from '../../core/models/product.models';
import { ProductsApiService } from '../../core/services/products-api.service';

@Component({
  selector: 'app-invoice-detail-page',
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatAutocompleteModule,
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
  private readonly productsApi = inject(ProductsApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);
  private readonly refreshSubject = new Subject<void>();

  readonly displayedColumns = ['productCode', 'productDescription', 'quantity'];

  readonly addItemForm = this.fb.nonNullable.group({
    productCode: ['', [Validators.required, Validators.maxLength(50)]],
    productDescription: ['', [Validators.required, Validators.maxLength(255)]],
    quantity: [1, [Validators.required, Validators.min(1)]]
  });

  readonly productSearchControl = this.fb.control<string | Product>('', {
    nonNullable: true,
    validators: [Validators.required]
  });

  readonly invoiceId$ = this.route.paramMap.pipe(
    map((params) => params.get('id')),
    filter((id): id is string => !!id),
    distinctUntilChanged()
  );

  readonly products$: Observable<Product[]> = this.productsApi.getProducts().pipe(
    tap(() => {
      this.productsLoading = false;
      this.productsLoadError = false;
    }),
    catchError((error) => {
      this.productsLoading = false;
      this.productsLoadError = true;
      this.openError(getApiErrorMessage(error, 'Não foi possível carregar o catálogo de produtos.'));
      return of([]);
    }),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  readonly filteredProducts$: Observable<Product[]> = combineLatest([
    this.products$,
    this.productSearchControl.valueChanges.pipe(startWith(this.productSearchControl.getRawValue()))
  ]).pipe(
    map(([products, search]) => this.filterProducts(products, search))
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
              this.openError(getApiErrorMessage(error, 'Não foi possível carregar os detalhes da nota fiscal.'));
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
  productsLoading = true;
  productsLoadError = false;
  selectedProduct: Product | null = null;

  constructor() {
    this.invoiceId$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.loading = true;
        this.refreshSubject.next();
      });

    this.productSearchControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') {
          return;
        }

        if (this.selectedProduct && value !== this.displayProduct(this.selectedProduct)) {
          this.clearSelectedProduct();
        }
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

    if (!this.selectedProduct) {
      this.productSearchControl.markAsTouched();
      this.openError('Selecione um produto do catálogo antes de adicionar o item.');
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
          this.openError(getApiErrorMessage(error, 'Não foi possível adicionar o item da nota fiscal.'));
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.addItemForm.reset({ productCode: '', productDescription: '', quantity: 1 });
        this.productSearchControl.reset('');
        this.selectedProduct = null;
        this.openSuccess('Item adicionado a nota fiscal.');
        this.invoicesApi.requestRefresh();
        this.refreshSubject.next();
      });
  }

  selectProduct(product: Product): void {
    this.selectedProduct = product;
    this.productSearchControl.setValue(this.displayProduct(product), { emitEvent: false });
    this.addItemForm.patchValue({
      productCode: product.code,
      productDescription: product.description
    });
  }

  displayProduct(product: Product | string | null): string {
    if (!product) {
      return '';
    }

    if (typeof product === 'string') {
      return product;
    }

    return `${product.code} - ${product.description}`;
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
            getApiErrorMessage(error, 'Não foi possível emitir a nota fiscal agora. Tente novamente mais tarde.')
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
    this.snackBar.open(message, 'Fechar', { duration: 3500 });
  }

  private openError(message: string): void {
    this.snackBar.open(message, 'Fechar', { duration: 5000, panelClass: ['snackbar-error'] });
  }

  private clearSelectedProduct(): void {
    this.selectedProduct = null;
    this.addItemForm.patchValue({
      productCode: '',
      productDescription: ''
    });
  }

  private filterProducts(products: Product[], search: string | Product): Product[] {
    const searchValue = typeof search === 'string' ? search : this.displayProduct(search);
    const normalizedSearch = searchValue.trim().toLowerCase();
    if (!normalizedSearch) {
      return products;
    }

    return products.filter((product) =>
      product.code.toLowerCase().includes(normalizedSearch) ||
      product.description.toLowerCase().includes(normalizedSearch)
    );
  }
}
