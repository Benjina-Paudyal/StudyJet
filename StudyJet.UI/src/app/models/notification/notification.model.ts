export interface Notification {
    id: number;
    userId: string;
    message: string;
    dateCreated: Date;
    isRead: boolean;
    courseID? : number | null;
  }
 