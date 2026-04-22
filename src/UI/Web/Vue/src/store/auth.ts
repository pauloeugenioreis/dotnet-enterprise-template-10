import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<any | null>(null);
  const token = ref<string | null>(localStorage.getItem('auth_token'));

  function setAuth(newUser: any, newToken: string) {
    user.value = newUser;
    token.value = newToken;
    localStorage.setItem('auth_token', newToken);
  }

  function logout() {
    user.value = null;
    token.value = null;
    localStorage.removeItem('auth_token');
  }

  return { user, token, setAuth, logout };
});
