namespace IkanLogger2.Models
{
    public class User
    {
        public int Id { get; private set; }
        public string Username { get; private set; }

        public User(int id, string username)
        {
            Id = id;
            Username = username;
        }
    }
}
