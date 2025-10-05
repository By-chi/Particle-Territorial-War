
using System.Numerics;
using Godot;

public partial class Database : Node
{
    public static Database Instance { get; private set; }
    public override void _Ready()
    {
        Instance = this;
    }
    public BigInteger BulletHistoryCount = 0;
    public BigInteger CurrentBulletCount = 0;

}