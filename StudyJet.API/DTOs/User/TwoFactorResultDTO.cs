namespace StudyJet.API.DTOs.User
{
    public class TwoFactorResultDTO
    {
        public bool Success { get; set; }
        public string Key { get; set; }
        public byte[] QrCodeImage { get; set; }
        public string ErrorMessage { get; set; }

    }
}
