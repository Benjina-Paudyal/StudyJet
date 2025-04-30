import { Injectable } from '@angular/core';
import emailjs from 'emailjs-com';
import { EmailFormData } from '../models/email/email-form-data-dto';

@Injectable({
    providedIn: 'root',
  })
  export class EmailService {
    
      private serviceID = 'service_ehed12j';  
      private templateID = 'template_rddmb4v'; 
      private userID = 'vQ2Wa6tRmkTppZMJS';         
  
      constructor() {}
  
      sendEmail(formData: EmailFormData): Promise<any>{
        return emailjs.send(this.serviceID, this.templateID, formData as any, this.userID);
      }

  }
