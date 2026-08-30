export type UserRole = 'Employee' | 'Agent' | 'Admin';
export type UserStatus = 'Active' | 'Inactive';

export interface User {
  id: number;
  name: string;
  email: string;
  role: UserRole;
  status: UserStatus;
  departmentId: number | null;
  departmentName: string | null;
  createdAt: string;
}
