import { TestBed } from '@angular/core/testing';
import { FileDownloadService } from './file-download.service';

describe('FileDownloadService', () => {
  let service: FileDownloadService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FileDownloadService);

    vi.spyOn(window.URL, 'createObjectURL').mockReturnValue('blob:http://localhost/fake-url');
    vi.spyOn(window.URL, 'revokeObjectURL').mockReturnValue(undefined);
  });

  afterEach(() => vi.restoreAllMocks());

  it('should call createObjectURL with the blob', () => {
    const blob = new Blob(['content'], { type: 'text/plain' });
    vi.spyOn(document, 'createElement').mockReturnValue(
      { href: '', download: '', click: vi.fn() } as unknown as HTMLAnchorElement
    );

    service.download(blob, 'file.txt');

    expect(window.URL.createObjectURL).toHaveBeenCalledWith(blob);
  });

  it('should set href to the created object URL', () => {
    const blob = new Blob(['data']);
    let capturedAnchor: { href: string; download: string; click: () => void } | null = null;
    vi.spyOn(document, 'createElement').mockImplementation(() => {
      capturedAnchor = { href: '', download: '', click: vi.fn() } as unknown as HTMLAnchorElement;
      return capturedAnchor as unknown as HTMLElement;
    });

    service.download(blob, 'test.pdf');

    expect(capturedAnchor!.href).toBe('blob:http://localhost/fake-url');
  });

  it('should set the download attribute to the provided filename', () => {
    const blob = new Blob(['data']);
    let capturedAnchor: { href: string; download: string; click: () => void } | null = null;
    vi.spyOn(document, 'createElement').mockImplementation(() => {
      capturedAnchor = { href: '', download: '', click: vi.fn() } as unknown as HTMLAnchorElement;
      return capturedAnchor as unknown as HTMLElement;
    });

    service.download(blob, 'relatorio.xlsx');

    expect(capturedAnchor!.download).toBe('relatorio.xlsx');
  });

  it('should trigger a click on the anchor element', () => {
    const blob = new Blob(['data']);
    const clickSpy = vi.fn();
    vi.spyOn(document, 'createElement').mockReturnValue(
      { href: '', download: '', click: clickSpy } as unknown as HTMLAnchorElement
    );

    service.download(blob, 'file.csv');

    expect(clickSpy).toHaveBeenCalledOnce();
  });

  it('should revoke the object URL after triggering click', () => {
    const blob = new Blob(['data']);
    vi.spyOn(document, 'createElement').mockReturnValue(
      { href: '', download: '', click: vi.fn() } as unknown as HTMLAnchorElement
    );

    service.download(blob, 'file.csv');

    expect(window.URL.revokeObjectURL).toHaveBeenCalledWith('blob:http://localhost/fake-url');
  });
});
