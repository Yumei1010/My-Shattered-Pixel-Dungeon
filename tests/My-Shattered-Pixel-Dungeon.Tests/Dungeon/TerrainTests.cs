using MyShatteredPixelDungeon.scripts.dungeon;

namespace MyShatteredPixelDungeon.Tests.Dungeon;

/// <summary>
///     地形常量与标志位测试
/// </summary>
public class TerrainTests
{
    [Fact]
    public void Wall_IsSolid_And_LosBlocking()
    {
        Assert.True(Terrain.IsSolid(Terrain.Wall));
        Assert.True(Terrain.IsLosBlocking(Terrain.Wall));
        Assert.False(Terrain.IsPassable(Terrain.Wall));
    }

    [Fact]
    public void Empty_IsPassable()
    {
        Assert.True(Terrain.IsPassable(Terrain.Empty));
        Assert.False(Terrain.IsSolid(Terrain.Empty));
        Assert.False(Terrain.IsLosBlocking(Terrain.Empty));
    }

    [Fact]
    public void Water_IsPassable_And_Liquid()
    {
        Assert.True(Terrain.IsPassable(Terrain.Water));
        Assert.True(Terrain.IsLiquid(Terrain.Water));
    }

    [Fact]
    public void Chasm_IsAvoid_And_Pit()
    {
        Assert.True(Terrain.IsAvoid(Terrain.Chasm));
        Assert.True(Terrain.IsPit(Terrain.Chasm));
        Assert.False(Terrain.IsPassable(Terrain.Chasm));
    }

    [Fact]
    public void Door_IsPassable_But_LosBlocking()
    {
        Assert.True(Terrain.IsPassable(Terrain.Door));
        Assert.True(Terrain.IsLosBlocking(Terrain.Door));
        Assert.True(Terrain.IsSolid(Terrain.Door));
    }

    [Fact]
    public void OpenDoor_IsPassable_Not_Solid()
    {
        Assert.True(Terrain.IsPassable(Terrain.OpenDoor));
        Assert.False(Terrain.IsSolid(Terrain.OpenDoor));
        Assert.False(Terrain.IsLosBlocking(Terrain.OpenDoor));
    }

    [Fact]
    public void HighGrass_IsLosBlocking_But_Passable()
    {
        Assert.True(Terrain.IsPassable(Terrain.HighGrass));
        Assert.True(Terrain.IsLosBlocking(Terrain.HighGrass));
        Assert.True(Terrain.IsFlammable(Terrain.HighGrass));
    }

    [Fact]
    public void SecretDoor_IsSecret_And_Solid()
    {
        Assert.True(Terrain.IsSecret(Terrain.SecretDoor));
        Assert.True(Terrain.IsSolid(Terrain.SecretDoor));
    }

    [Fact]
    public void Trap_IsAvoid_Not_Passable()
    {
        Assert.True(Terrain.IsAvoid(Terrain.Trap));
        Assert.False(Terrain.IsPassable(Terrain.Trap));
    }

    [Fact]
    public void Discover_Reveals_SecretDoor()
    {
        Assert.Equal(Terrain.Door, Terrain.Discover(Terrain.SecretDoor));
        Assert.Equal(Terrain.Trap, Terrain.Discover(Terrain.SecretTrap));
        Assert.Equal(Terrain.Empty, Terrain.Discover(Terrain.Empty));
    }

    [Fact]
    public void Flags_Array_Has_All_Terrain_Types_Defined()
    {
        // 验证所有主要地形都有标志定义（非 None）
        var terrains = new[]
        {
            Terrain.Chasm, Terrain.Empty, Terrain.Grass, Terrain.Water, Terrain.Wall,
            Terrain.Door, Terrain.OpenDoor, Terrain.Entrance, Terrain.Exit, Terrain.Embers,
            Terrain.LockedDoor, Terrain.CrystalDoor, Terrain.Pedestal, Terrain.Barricade,
            Terrain.HighGrass, Terrain.FurrowedGrass, Terrain.SecretDoor, Terrain.SecretTrap,
            Terrain.Trap, Terrain.InactiveTrap, Terrain.EmptyDeco, Terrain.LockedExit,
            Terrain.UnlockedExit, Terrain.Well, Terrain.Bookshelf, Terrain.Alchemy,
            Terrain.Statue, Terrain.RegionDeco, Terrain.MineCrystal, Terrain.MineBoulder,
        };

        foreach (var t in terrains)
        {
            Assert.NotEqual(TileFlags.None, Terrain.Flags[t]);
        }
    }
}