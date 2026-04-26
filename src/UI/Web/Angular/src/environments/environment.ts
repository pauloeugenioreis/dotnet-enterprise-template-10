interface AppEnvironment {
  production: boolean;
  apiUrl: string;
  demo: { email: string; password: string };
}

export const environment: AppEnvironment = {
  production: false,
  apiUrl: (window as unknown as Record<string, Record<string, string>>)['env']?.['apiUrl'] ?? '',
  demo: {
    email: 'admin@projecttemplate.com',
    password: 'Admin@2026!Secure'
  }
};
