using Godot;
using System.Collections.Generic;

public partial class BulletPool : Node
{
    [Export] public PackedScene BulletScene { get; set; }
    private readonly Queue<Bullet> _inactiveBullets = new();

    public Bullet GetBullet()
    {
        Bullet bullet;

        if (_inactiveBullets.Count > 0)
        {
            bullet = _inactiveBullets.Dequeue();
        }
        else
        {
            bullet = BulletScene.Instantiate<Bullet>();
            bullet.OnBulletDespawn += ReturnBullet;
            GetTree().CurrentScene.AddChild(bullet);
            Database.Instance.CurrentBulletCount++;
        }
        Database.Instance.BulletHistoryCount++;
        return bullet;
    }

    public void ReturnBullet(Bullet bullet)
    {
        // 避免重复入队（添加哈希表检查，O(1)判断）
        if (!_inactiveBullets.Contains(bullet))
        {
            _inactiveBullets.Enqueue(bullet); // O(1)操作
        }
    }
    public void Clear()
    {
        foreach (var bullet in _inactiveBullets)
        {
            bullet.QueueFree();
        }
        _inactiveBullets.Clear();
    }
}
