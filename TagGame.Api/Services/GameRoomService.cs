using Microsoft.EntityFrameworkCore;
using TagGame.Api.Persistence;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Api.Services;

public class GameRoomService
{
    private readonly IDatabase _db;

    public GameRoomService(IDatabase dataSet)
    {
        _db = dataSet;
    }

    public async Task<GameRoom?> GetRoomAsync(Guid roomId)
    {
        if (Equals(roomId, Guid.Empty))
            return null;

        var room = await _db.Set<GameRoom>()
            .AsNoTracking()
            .Include(r => r.Settings)
            .Include(r => r.Players)
            .FirstOrDefaultAsync(r => Equals(r.Id, roomId));

        return room;
    }

    public async Task<GameRoom?> GetRoomAsync(string name, string accessCode)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(accessCode))
            return null;

        var room = await _db.Set<GameRoom>()
            .AsNoTracking()
            .Include(r => r.Players)
            .Where(r => Equals(r.Name, name) && Equals(r.AccessCode, accessCode))
            .FirstOrDefaultAsync();

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

        var settingsEntry = await _db.Set<GameSettings>().AddAsync(room.Settings);
        var roomEntity = await _db.Set<GameRoom>().AddAsync(room);

        if (settingsEntry.State != EntityState.Added &&
            roomEntity.State != EntityState.Added)
            return null;

        var changedEntites = await _db.SaveChangesAsync();
        if (changedEntites == 0)
            return null;

        return room;
    }

    public async Task<bool> DeleteRoomAsync(Guid roomId)
    {
        var room = await GetRoomAsync(roomId);
        if (room is null)
            return false;
        
        var entry = _db.Set<GameRoom>().Remove(room);
        if (entry.State != EntityState.Detached)
            return false;
        
        var changedEntites = await _db.SaveChangesAsync();
        return changedEntites > 0;
    }

    public async Task<bool> UpdateSettingsAsync(Guid roomId, GameSettings settings)
    {
        var room = await GetRoomAsync(roomId);
        if (room is null)
            return false;
        
        room.Settings = settings;
        
        var entry = _db.Set<GameSettings>().Update(room.Settings);
        if (entry.State != EntityState.Modified)
            return false;
        
        var changedEntites = await _db.SaveChangesAsync();
        return changedEntites > 0;
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
