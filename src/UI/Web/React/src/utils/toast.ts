import { toast } from 'sonner';

/**
 * Utilitário centralizado para notificações (Toasts).
 * Pode ser usado dentro e fora de componentes React.
 */
export const notify = {
  success: (message: string, description?: string) => {
    toast.success(message, { description });
  },
  error: (message: string, description?: string) => {
    toast.error(message, { description, duration: 5000 });
  },
  info: (message: string, description?: string) => {
    toast.info(message, { description });
  },
  warning: (message: string, description?: string) => {
    toast.warning(message, { description });
  },
  loading: (message: string) => {
    return toast.loading(message);
  },
  dismiss: (id?: string | number) => {
    toast.dismiss(id);
  }
};
