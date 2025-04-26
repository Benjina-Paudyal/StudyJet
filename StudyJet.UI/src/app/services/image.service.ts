import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ImageService {
  constructor() {}

  // Course Image
  getCourseImageUrl(imageFilename: string): string {
    const cleanedImageFilename = this.cleanImageFilename(
      imageFilename,
      'courses'
    );
    return `${environment.imageBaseUrl}/images/courses/${cleanedImageFilename}`;
  }

  // Profile Image
  getProfileImageUrl(imageFilename: string): string {
    const cleanedImageFilename = this.cleanImageFilename(
      imageFilename,
      'profiles'
    );
    return `${environment.imageBaseUrl}/images/profiles/${cleanedImageFilename}`;
  }

  // Helper method 
  private cleanImageFilename(imageFilename: string, imageType: string): string {
    if (
      imageType === 'courses' &&
      imageFilename.startsWith('/images/courses/')
    ) {
      return imageFilename.replace('/images/courses/', '');
    }

    if (
      imageType === 'profiles' &&
      imageFilename.startsWith('/images/profiles/')
    ) {
      return imageFilename.replace('/images/profiles/', '');
    }

    // If no cleaning is needed
    return imageFilename;
  }
}
