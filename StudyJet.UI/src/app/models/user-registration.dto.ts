
export interface UserRegistration
{
    UserName: string;
    Email: string;
    Password: string;
    ConfirmPassword : string;
    ProfilePicture?: File;
    FullName: string;
}