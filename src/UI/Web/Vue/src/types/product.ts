export interface ProductResponse {
  id: string | number;
  name: string;
  category: string;
  price: number;
  stock: number;
  isActive: boolean;
}

export interface CreateProductRequest {
  name: string;
  category: string;
  price: number;
  stock: number;
  isActive: boolean;
}
