using System.Drawing;

namespace TagGame.Shared.DTOs.Users;

public class CreateUser
{
    public class CreateUserRequest
    {
        public string Name { get; set; }
        
        public Color AvatarColor { get; set; }
    }
}