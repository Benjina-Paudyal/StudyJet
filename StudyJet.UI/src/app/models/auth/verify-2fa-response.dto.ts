export interface Verify2FAResponse {
    token: string;
    roles: string[];
    username: string;
    userID: string;
    fullName?: string;
    email?: string;
    profilePictureUrl?: string;
  }