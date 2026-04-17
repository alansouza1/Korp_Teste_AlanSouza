export interface Product {
  id: string;
  code: string;
  description: string;
  stockQuantity: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductRequest {
  code: string;
  description: string;
  stockQuantity: number;
}

export interface UpdateProductDescriptionRequest {
  description: string;
}

export interface UpdateProductStockRequest {
  stockQuantity: number;
}
