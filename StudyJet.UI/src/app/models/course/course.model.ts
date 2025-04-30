
export interface Course {
    courseID: number;
    title: string;
    description?: string;
    imageUrl: string;
    price: number;
    instructorID: string;
    categoryID: number;
    creationDate: Date;
    lastUpdatedDate: Date;
    videoUrl: string;
    isPopular: boolean;
    instructorName: string;
    categoryName: string;
    purchaseDate: Date;
    totalPrice: number;
    isApproved: boolean;
    status: "Pending" | "Approved" | "Rejected" | 0 | 1 | 2; 
    isUpdate?: boolean;  
    updateId?: number;
  }