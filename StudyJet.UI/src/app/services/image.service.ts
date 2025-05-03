import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ImageService {
  constructor() {}

  // Course Image
  getCourseImageUrl(imageFilename: string): string {
    if (!imageFilename) {
      console.warn('Image filename is undefined or null');
      return ''; 
    }
  
    // Check if the image is already a full URL
    if (imageFilename.startsWith('http') || imageFilename.startsWith('https')) {
      return imageFilename; 
    }
   
    const cleanedImageFilename = this.cleanImageFilename(imageFilename, 'courses');
    return `${environment.imageBaseUrl}/images/courses/${cleanedImageFilename}`;
  }
  

  // Profile Image
  getProfileImageUrl(imageFilename: string): string {

    if (!imageFilename) {
      console.warn('Profile image filename is undefined or null');
      return ''; 
    }

      // Check if the image is already a full URL
  if (imageFilename.startsWith('http') || imageFilename.startsWith('https')) {
    return imageFilename;
  }

    const cleanedImageFilename = this.cleanImageFilename(
      imageFilename,
      'profiles'
    );
    return `${environment.imageBaseUrl}/images/profiles/${cleanedImageFilename}`;
  }

  // Helper method 
  private cleanImageFilename(imageFilename: string | undefined, imageType: string): string {
    if (!imageFilename) {
      console.warn('Image filename is undefined or null');
      return '';  
    }
  
    if (imageType === 'courses' && imageFilename.startsWith('/images/courses/')) {
      return imageFilename.replace('/images/courses/', '');
    }
  
    if (imageType === 'profiles' && imageFilename.startsWith('/images/profiles/')) {
      return imageFilename.replace('/images/profiles/', '');
    }
  
    // If no cleaning is needed
    return imageFilename;
  }
  
}


