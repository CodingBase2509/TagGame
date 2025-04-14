using TagGame.Api.Persistence;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Api.Tests;

public static class Utility
{
    public static Mock<IDataAccess> CreateDbMockForRooms(Mock<IDataAccess> dbMock = null)
    {
        dbMock ??= new Mock<IDataAccess>();
        var roomMock = new Mock<IDataSet<GameRoom>>();
        dbMock.Setup(x => x.Rooms)
            .Returns(() => roomMock.Object);
        
        var settingsMock = new Mock<IDataSet<GameSettings>>();
        dbMock.Setup(x => x.Settings)
            .Returns(() => settingsMock.Object);
        
        roomMock.Setup(x => x.Include(r => r.Settings))
            .Returns(() => roomMock.Object);

        roomMock.Setup(x => x.AddAsync(It.IsAny<GameRoom>()))
            .ReturnsAsync(true);
        settingsMock.Setup(x => x.AddAsync(It.IsAny<GameSettings>()))
            .ReturnsAsync(true);
        
        return dbMock;
    }
    
    public static Mock<IDataAccess> CreateDbMockForPlayers(Mock<IDataAccess> dbMock = null)
    {
        dbMock = CreateDbMockForRooms(dbMock);
        
        var userMock = new Mock<IDataSet<User>>();
        dbMock.Setup(x => x.Users)
            .Returns(() => userMock.Object);
        
        var playerMock = new Mock<IDataSet<Player>>();
        dbMock.Setup(x => x.Players)
            .Returns(() => playerMock.Object);
        
        userMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(() => new User());
        playerMock.Setup(x => x.AddAsync(It.IsAny<Player>()))
            .ReturnsAsync(true);
        
        return dbMock;
    }
}