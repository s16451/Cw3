namespace APBD
{
    public interface IPasswordHashingService
    {
        public string GenerateHash(string value, string salt);
        
        public string GenerateSalt();
        
        public bool Validate(string value, string salt, string hash);
    }
}