export interface DecodedToken {
    sub: string;
    userId: string;
    email: string;
    role: string;
    exp: number;
    iat: number;
    jti: string;
    nbf: number;
    iss: string;
    aud: string;
  }
  
  export function decodeToken(token: string): DecodedToken | null {
    const parts = token.split('.');
    if (parts.length === 3) {
      const payload = atob(parts[1]);
      return JSON.parse(payload);
    }
    return null;
  }