namespace CMetalsWS.Data
{
    public class UserDialogResult
    {
        public ApplicationUser User { get; set; } = new();
        public string? Password { get; set; }
        public List<string> Roles { get; set; } = new();

        public UserDialogResult() { }

        public UserDialogResult(ApplicationUser user, string? password, IEnumerable<string> roles)
        {
            User = user;
            Password = password;
            Roles = roles?.ToList() ?? new List<string>();
        }
    }
}
