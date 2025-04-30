
export interface EmailFormData {
    name: string;
    email: string;
    message: string;
    cv_attachment?: string;
    cv_link?: string;

    [key: string]: any; // allow any extra keys for compatibility with EmailJS
  }
  