using TagGame.Shared.DTOs.Common;

namespace TagGame.Shared.DTOs.Users;

public class CreateUser
{
    public class CreateUserRequest
    {
        public string Name { get; set; }
        
        public ColorDTO AvatarColor { get; set; }
    }
}