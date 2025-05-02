export interface AuthResponse {
    token: string;         
    roles: string[];       
    username: string;     
    userID: string;        
    fullName?: string;
    email?: string;
    profilePictureUrl?: string;
    requires2FA?: boolean;
    requiresPasswordChange?: boolean;
  }