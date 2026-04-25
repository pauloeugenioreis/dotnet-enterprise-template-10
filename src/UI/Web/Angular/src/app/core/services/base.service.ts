import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';

export abstract class BaseService {
  protected http = inject(HttpClient);
  protected baseUrl = environment.apiUrl;
}
