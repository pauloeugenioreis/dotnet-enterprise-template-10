import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';

export abstract class BaseService {
  protected http = inject(HttpClient);
  protected baseUrl = 'https://localhost:7196'; // Centralizar em environment.ts no futuro
}
