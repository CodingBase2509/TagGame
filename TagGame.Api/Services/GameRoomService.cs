using TagGame.Api.Persistence;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Services;

public class GameRoomService(IDataAccess db)
{
    public async Task<GameRoom?> GetRoomAsync(Guid roomId)
    {
        if (Equals(roomId, Guid.Empty))
            return null;

        var room = await db.Rooms.GetByIdAsync(roomId, false);

        return room;
    }

    public async Task<GameRoom?> GetRoomAsync(string name, string accessCode)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(accessCode))
            return null;

        var room = db.Rooms
            .Where(r => Equals(r.Name, name) && Equals(r.AccessCode, accessCode))
            .FirstOrDefault();

        return room;
    }

    public async Task<GameRoom?> CreateAsync(Guid userId, string roomName)
    {
        var room = new GameRoom()
        {
            Id = Guid.NewGuid(),
            CreatorId = userId,
            Name = roomName,
            Players = [],
            State = GameState.Lobby,
            AccessCode = GenerateAccessCode(), 
        };

        room.Settings = new()
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            HideTimeout = GameOptions.DefaultHideTimeout,
            IsPingEnabled = GameOptions.DefaultPingEnabled,
            PingInterval = GameOptions.DefaultPingInterval,
            Area = new GameArea()
            {
                Shape = GameOptions.DefaultGameArea,
                Boundary = []
            }
        };

        var settingsSuccess = await db.Settings.AddAsync(room.Settings);
        var roomSuccess = await db.Rooms.AddAsync(room);

        if (!settingsSuccess && !roomSuccess)
            return null;

        var saveSuccess = await db.SaveChangesAsync();
        return saveSuccess ? room : null;
    }

    public async Task<bool> DeleteRoomAsync(Guid roomId)
    {
        var room = await GetRoomAsync(roomId);
        if (room is null)
            return false;
        
        var success = await db.Rooms.DeleteAsync(room);
        if (!success)
            return false;
        
        return await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateSettingsAsync(Guid roomId, GameSettings settings)
    {
        var room = await GetRoomAsync(roomId);
        if (room is null)
            return false;
        
        room.Settings = settings;
        
        var success = await db.Settings.UpdateAsync(room.Settings);
        if (!success)
            return false;
        
        return await db.SaveChangesAsync();
    }

    private static string GenerateAccessCode()
    {
        string accessCode = string.Empty;
        
        int[][] ranges = [[48, 57], [97, 122]];
        var numRange = ranges[0];
        var charRange = ranges[1];

        for (int i = 0; i < MaxLengthOptions.AccessCodeLenght; i++)
        {
            var number = Random.Shared.Next(numRange[0], numRange[1]);
            var c = Random.Shared.Next(charRange[0], charRange[1]);

            var random = Random.Shared.Next(0, 1);

            if (random == 0)
                accessCode += number;
            else if (random == 1)
                accessCode += c;
            else
                accessCode += string.Empty;
        } 

        return accessCode;
    }
}
