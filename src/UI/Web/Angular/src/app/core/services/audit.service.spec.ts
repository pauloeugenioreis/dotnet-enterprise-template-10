import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AuditService } from './audit.service';

describe('AuditService', () => {
  let service: AuditService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        AuditService
      ]
    });
    service = TestBed.inject(AuditService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get audit logs with filters', () => {
    const now = new Date();
    const from = new Date(now.getFullYear(), now.getMonth(), 1);
    const to = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    service.getAuditLogs(1, 10, 'Order', 'Created', 'user1', from, to).subscribe();

    const req = httpMock.expectOne(r => 
      r.url.includes('/api/v1/audit?page=1&pageSize=10') &&
      r.url.includes('entityType=Order') &&
      r.url.includes('eventType=Created') &&
      r.url.includes('userId=user1') &&
      r.url.includes(`startDate=${from.toISOString()}`) &&
      r.url.includes(`endDate=${to.toISOString()}`)
    );
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
