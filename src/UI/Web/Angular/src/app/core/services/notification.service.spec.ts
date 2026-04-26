import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationService);
  });

  describe('toast notifications', () => {
    it('should add a success toast with correct type and message', () => {
      service.success('Operação concluída');
      expect(service.toasts().length).toBe(1);
      expect(service.toasts()[0].type).toBe('success');
      expect(service.toasts()[0].message).toBe('Operação concluída');
    });

    it('should add an error toast', () => {
      service.error('Algo deu errado');
      expect(service.toasts()[0].type).toBe('error');
    });

    it('should add a warning toast', () => {
      service.warning('Atenção');
      expect(service.toasts()[0].type).toBe('warning');
    });

    it('should add an info toast', () => {
      service.info('Informação');
      expect(service.toasts()[0].type).toBe('info');
    });

    it('should accumulate multiple toasts', () => {
      service.success('One');
      service.error('Two');
      service.warning('Three');
      expect(service.toasts().length).toBe(3);
    });

    it('should assign unique ids to each toast', () => {
      service.success('A');
      service.success('B');
      const ids = service.toasts().map(t => t.id);
      expect(ids[0]).not.toBe(ids[1]);
    });

    it('should auto-dismiss a toast after 4500ms', () => {
      vi.useFakeTimers();
      service.success('Auto-dismiss');
      expect(service.toasts().length).toBe(1);
      vi.advanceTimersByTime(4500);
      expect(service.toasts().length).toBe(0);
      vi.useRealTimers();
    });

    it('should not dismiss before 4500ms', () => {
      vi.useFakeTimers();
      service.success('Still here');
      vi.advanceTimersByTime(4499);
      expect(service.toasts().length).toBe(1);
      vi.useRealTimers();
    });

    it('should manually dismiss a toast by id', () => {
      service.success('Manual');
      const id = service.toasts()[0].id;
      service.dismiss(id);
      expect(service.toasts().length).toBe(0);
    });

    it('should only dismiss the toast with the matching id', () => {
      service.success('Keep me');
      service.error('Remove me');
      const idToRemove = service.toasts()[1].id;
      service.dismiss(idToRemove);
      expect(service.toasts().length).toBe(1);
      expect(service.toasts()[0].message).toBe('Keep me');
    });
  });

  describe('confirm dialog', () => {
    it('should set confirmState when confirm is called', () => {
      service.confirm('Você tem certeza?').subscribe();
      expect(service.confirmState()).not.toBeNull();
      expect(service.confirmState()!.message).toBe('Você tem certeza?');
    });

    it('should emit true when resolved with true', async () => {
      const promise = firstValueFrom(service.confirm('Confirmar?'));
      service.confirmState()!.resolve(true);
      expect(await promise).toBe(true);
    });

    it('should emit false when resolved with false', async () => {
      const promise = firstValueFrom(service.confirm('Cancelar?'));
      service.confirmState()!.resolve(false);
      expect(await promise).toBe(false);
    });

    it('should clear confirmState after resolution', async () => {
      const promise = firstValueFrom(service.confirm('Test'));
      service.confirmState()!.resolve(true);
      await promise;
      expect(service.confirmState()).toBeNull();
    });

    it('should complete the observable after resolution', async () => {
      let completed = false;
      service.confirm('Test').subscribe({ complete: () => { completed = true; } });
      service.confirmState()!.resolve(true);
      expect(completed).toBe(true);
    });
  });
});
