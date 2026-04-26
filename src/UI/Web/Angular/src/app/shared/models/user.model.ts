export interface User {
  id: string | number;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  profileImageUrl: string | null;
  roles: string[];
  createdAt: string;
}
