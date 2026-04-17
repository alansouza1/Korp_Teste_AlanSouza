import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { EMPTY, Subject, catchError, finalize, startWith, switchMap, tap } from 'rxjs';

import { Product } from '../../core/models/product.models';
import { ProductsApiService } from '../../core/services/products-api.service';
import { getApiErrorMessage } from '../../core/services/api-error.util';

@Component({
  selector: 'app-products-page',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './products-page.component.html',
  styleUrl: './products-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductsPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly productsApi = inject(ProductsApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);
  private readonly refreshSubject = new Subject<void>();

  readonly displayedColumns = ['code', 'description', 'stockQuantity', 'updatedAt', 'actions'];

  readonly filterForm = this.fb.nonNullable.group({
    code: [''],
    description: ['']
  });

  readonly createForm = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    description: ['', [Validators.required, Validators.maxLength(255)]],
    stockQuantity: [0, [Validators.required, Validators.min(0)]]
  });

  readonly editDescriptionForm = this.fb.nonNullable.group({
    description: ['', [Validators.required, Validators.maxLength(255)]]
  });

  readonly stockForm = this.fb.nonNullable.group({
    stockQuantity: [0, [Validators.required, Validators.min(0)]]
  });

  readonly products$ = this.refreshSubject.pipe(
    startWith(void 0),
    switchMap(() =>
      this.productsApi.getProducts(this.filterForm.getRawValue()).pipe(
        tap(() => (this.loadingList = false)),
        catchError((error) => {
          this.loadingList = false;
          this.openError(getApiErrorMessage(error, 'Não foi possível carregar os produtos.'));
          return EMPTY;
        })
      )
    )
  );

  loadingList = true;
  creating = false;
  savingDescription = false;
  savingStock = false;
  selectedProduct: Product | null = null;

  constructor() {
    this.filterForm.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.loadingList = true;
        this.refreshSubject.next();
      });
  }

  refreshProducts(): void {
    this.loadingList = true;
    this.refreshSubject.next();
  }

  selectProduct(product: Product): void {
    this.selectedProduct = product;
    this.editDescriptionForm.patchValue({ description: product.description });
    this.stockForm.patchValue({ stockQuantity: product.stockQuantity });
  }

  clearSelection(): void {
    this.selectedProduct = null;
    this.editDescriptionForm.reset({ description: '' });
    this.stockForm.reset({ stockQuantity: 0 });
  }

  createProduct(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.creating = true;
    this.productsApi
      .createProduct(this.createForm.getRawValue())
      .pipe(
        finalize(() => (this.creating = false)),
        catchError((error) => {
          this.openError(getApiErrorMessage(error, 'Não foi possível cadastrar o produto.'));
          return EMPTY;
        })
      )
      .subscribe((product) => {
        this.createForm.reset({ code: '', description: '', stockQuantity: 0 });
        this.openSuccess(`Produto ${product.code} cadastrado com sucesso.`);
        this.refreshProducts();
      });
  }

  saveDescription(): void {
    if (!this.selectedProduct) {
      return;
    }

    if (this.editDescriptionForm.invalid) {
      this.editDescriptionForm.markAllAsTouched();
      return;
    }

    this.savingDescription = true;
    this.productsApi
      .updateDescription(this.selectedProduct.id, this.editDescriptionForm.getRawValue())
      .pipe(
        finalize(() => (this.savingDescription = false)),
        catchError((error) => {
          this.openError(getApiErrorMessage(error, 'Não foi possível atualizar a descrição do produto.'));
          return EMPTY;
        })
      )
      .subscribe((product) => {
        this.openSuccess(`Descrição atualizada para ${product.code}.`);
        this.selectProduct(product);
        this.refreshProducts();
      });
  }

  saveStock(): void {
    if (!this.selectedProduct) {
      return;
    }

    if (this.stockForm.invalid) {
      this.stockForm.markAllAsTouched();
      return;
    }

    this.savingStock = true;
    this.productsApi
      .updateStock(this.selectedProduct.id, this.stockForm.getRawValue())
      .pipe(
        finalize(() => (this.savingStock = false)),
        catchError((error) => {
          this.openError(getApiErrorMessage(error, 'Não foi possível atualizar o estoque administrativo.'));
          return EMPTY;
        })
      )
      .subscribe((product) => {
        this.openSuccess(`Estoque atualizado para ${product.code}.`);
        this.selectProduct(product);
        this.refreshProducts();
      });
  }

  private openSuccess(message: string): void {
    this.snackBar.open(message, 'Fechar', { duration: 3500 });
  }

  private openError(message: string): void {
    this.snackBar.open(message, 'Fechar', { duration: 5000, panelClass: ['snackbar-error'] });
  }
}
