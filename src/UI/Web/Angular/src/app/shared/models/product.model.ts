export interface CreateProductRequest {
  name: string;
  category: string;
  price: number;
  stock: number;
  isActive: boolean;
}

export interface ProductResponse {
  id: string;
  name: string;
  category: string;
  price: number;
  stock: number;
  isActive: boolean;
}
