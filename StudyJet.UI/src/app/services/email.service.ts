import { Injectable } from '@angular/core';
import emailjs, { EmailJSResponseStatus } from 'emailjs-com';
import { EmailFormData } from '../models/email/email-form-data.dto';

@Injectable({
    providedIn: 'root',
  })
  export class EmailService {
      private serviceID = 'service_ehed12j';  
      private templateID = 'template_rddmb4v'; 
      private userID = 'vQ2Wa6tRmkTppZMJS';         
  
      constructor() {}
  
      sendEmail(formData: EmailFormData): Promise<EmailJSResponseStatus> {
        return emailjs.send(this.serviceID, this.templateID, formData, this.userID);
      }
}
