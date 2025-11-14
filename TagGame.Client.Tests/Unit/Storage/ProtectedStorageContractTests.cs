using TagGame.Client.Core.Storage;

namespace TagGame.Client.Tests.Unit.Storage;

public class ProtectedStorageContractTests
{
    private static InMemoryProtectedStorage Create() => new();

    [Fact]
    public async Task Write_then_Read_returns_same_bytes()
    {
        var sut = Create();
        var name = "sample";
        ReadOnlyMemory<byte> bytes = new byte[] { 1, 2, 3, 4, 5 };

        await sut.WriteAsync(name, bytes);
        var read = await sut.ReadAsync(name);

        read.HasValue.Should().BeTrue();
        read!.Value.ToArray().Should().Equal(bytes.ToArray());
    }

    [Fact]
    public async Task Read_unknown_returns_null()
    {
        var sut = Create();
        var read = await sut.ReadAsync("does-not-exist");
        read.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_removes_entry()
    {
        var sut = Create();
        await sut.WriteAsync("x", "\t\t\t"u8.ToArray());
        await sut.DeleteAsync("x");
        var read = await sut.ReadAsync("x");
        read.HasValue.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Guard_name_required(string? name)
    {
        var sut = Create();
        var write = async () => await sut.WriteAsync(name!, new byte[] { 1 });
        var read = async () => await sut.ReadAsync(name!);
        var del = async () => await sut.DeleteAsync(name!);

        await write.Should().ThrowAsync<ArgumentException>();
        await read.Should().ThrowAsync<ArgumentException>();
        await del.Should().ThrowAsync<ArgumentException>();
    }

    private sealed class InMemoryProtectedStorage : IProtectedStorage
    {
        private readonly Dictionary<string, byte[]> _map = new(StringComparer.Ordinal);

        public Task<ReadOnlyMemory<byte>?> ReadAsync(string name, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return Task.FromResult(_map.TryGetValue(name, out var bytes)
                ? new ReadOnlyMemory<byte>?(bytes)
                : null);
        }

        public Task WriteAsync(string name, ReadOnlyMemory<byte> data, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            _map[name] = data.ToArray();
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string name, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            _map.Remove(name);
            return Task.CompletedTask;
        }
    }
}
