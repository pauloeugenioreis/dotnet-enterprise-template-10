import { TestBed } from '@angular/core/testing';
import { StorageService } from './storage.service';

describe('StorageService', () => {
  let service: StorageService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(StorageService);
  });

  afterEach(() => localStorage.clear());

  describe('get / set', () => {
    it('should store and retrieve a string value', () => {
      service.set('key', 'hello');
      expect(service.get('key')).toBe('hello');
    });

    it('should return null for a key that does not exist', () => {
      expect(service.get('missing')).toBeNull();
    });

    it('should overwrite an existing value', () => {
      service.set('key', 'first');
      service.set('key', 'second');
      expect(service.get('key')).toBe('second');
    });
  });

  describe('remove', () => {
    it('should remove a stored key', () => {
      service.set('key', 'value');
      service.remove('key');
      expect(service.get('key')).toBeNull();
    });

    it('should not throw when removing a non-existent key', () => {
      expect(() => service.remove('ghost')).not.toThrow();
    });
  });

  describe('getObject / setObject', () => {
    it('should serialize and deserialize an object', () => {
      const obj = { id: 42, name: 'Test' };
      service.setObject('obj', obj);
      expect(service.getObject('obj')).toEqual(obj);
    });

    it('should return null for a missing key', () => {
      expect(service.getObject('missing')).toBeNull();
    });

    it('should return null when stored value is invalid JSON', () => {
      localStorage.setItem('bad', '{invalid_json}');
      expect(service.getObject('bad')).toBeNull();
    });

    it('should handle nested objects correctly', () => {
      const nested = { user: { id: '1', roles: ['admin'] } };
      service.setObject('nested', nested);
      expect(service.getObject('nested')).toEqual(nested);
    });
  });
});
