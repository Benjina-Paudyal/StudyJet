export interface Notification {
    id: number;
    userId: string;
    message: string;
    dateCreated: Date;
    isRead: boolean;
    courseId? : number;
  }
 