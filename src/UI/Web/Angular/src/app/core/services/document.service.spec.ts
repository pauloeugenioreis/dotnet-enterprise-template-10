import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { DocumentService } from './document.service';

describe('DocumentService', () => {
  let service: DocumentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        DocumentService
      ]
    });
    service = TestBed.inject(DocumentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should upload a file', () => {
    const file = new File([''], 'test.txt');
    service.upload(file).subscribe();

    const req = httpMock.expectOne(r => r.url.includes('/api/v1/Document/upload'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body instanceof FormData).toBe(true);
    req.flush({ fileName: 'test.txt' });
  });

  it('should download a file', () => {
    const fileName = 'test.txt';
    service.download(fileName).subscribe();

    const req = httpMock.expectOne(r => r.url.endsWith(`/api/v1/Document/${fileName}`));
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    req.flush(new Blob());
  });

  it('should delete a document', () => {
    const fileName = 'test.txt';
    service.delete(fileName).subscribe();

    const req = httpMock.expectOne(r => r.url.endsWith(`/api/v1/Document/${fileName}`));
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
