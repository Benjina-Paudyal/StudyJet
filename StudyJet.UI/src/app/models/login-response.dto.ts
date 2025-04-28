
export interface LoginResponse {
    requires2FA?: boolean;
    tempToken?: string;
    requiresPasswordChange?: boolean;
    resetToken?: string;
    token?: string;
    roles?: string[];
    username?: string;
    profilePictureUrl?: string;
    fullName?: string;
    userId?: string;
  }
  