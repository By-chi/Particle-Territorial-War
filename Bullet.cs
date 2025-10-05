using System;
using System.Linq;
using Godot;

public partial class Bullet : Area2D
{
    public event Action<Bullet> OnBulletDespawn;
    
    // 缓存常用变量，减少成员访问开销
    private int _mapWidth;
    private int _mapHeight;
    private Vector2 _mapGlobalPos;
    private System.Numerics.BigInteger _health;
    public System.Numerics.BigInteger Health
    {
        get => _health;
        set
        {
            _health = value;
            if (Health <= 0)
            {
                CallDeferred("TriggerDespawn");
            }
        }
    }
    public Battery Host;
    public Map map;
    public Vector2 Move;
    public System.Numerics.BigInteger id=0;
    private Vector2 _lastWorldPosition;

    public new void AreaEntered(Area2D area)
    {
        if (IsInstanceValid(area)&&area is Bullet bullet && bullet.Host != Host)
        {
            if (Health > bullet.Health || (Health == bullet.Health && id > bullet.id))
            {
                Health -= bullet.Health;
                bullet.Health = 0;
            }
        }
    }
    public void Reset(System.Numerics.BigInteger health)
    {
        Visible = true;
        ProcessMode = ProcessModeEnum.Inherit; // 替代多个SetProcess调用
        Monitoring = true;
        _lastWorldPosition = GlobalPosition;
        Health = health;
        // 缓存地图边界数据，避免每帧访问
        if (map != null)
        {
            _mapWidth = map._textureWidth;
            _mapHeight = map._textureHeight;
            _mapGlobalPos = map.GlobalPosition;
        }
        id = Database.Instance.BulletHistoryCount;
    }
    public new void BodyEntered(Node2D body)
	{
        if (body is BigBall bigBall)
        {
            if (bigBall.Host == Host)
            {
                bigBall.HealthDecorators += Health;
            }
            else
            {
                bigBall.HealthDecorators -= Health;
            }
            bigBall.ApplyForce(Move * 0.05f,GlobalPosition);
            CallDeferred("TriggerDespawn");
            
		}
	}
    public void TriggerDespawn()
    {
        Visible = false;
        ProcessMode = ProcessModeEnum.Disabled;
        Monitoring = false;
        OnBulletDespawn?.Invoke(this);	
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Health == 0)
            return;

        var deltaMove = Move * (float)delta;
        GlobalPosition += deltaMove;
        var currentWorldPosition = GlobalPosition;

        var mapLocalPosLast = map.ToLocal(_lastWorldPosition);
        var mapLocalPosCurrent = map.ToLocal(currentWorldPosition);

        var startPixel = (Vector2I)mapLocalPosLast;
        var endPixel = (Vector2I)mapLocalPosCurrent;
        var result = PerformanceOptimizedSemicircleLine.GetAllPoints(startPixel, endPixel, 6);
        
        // 用for循环替代foreach，减少迭代器开销
        for (int i = 0; i < result.Points.Length; i++)
        {
            var point = result.Points[i];
            // 合并边界检查（利用缓存的地图尺寸）
            if (point.X >= 0 && point.X < _mapWidth && 
                point.Y >= 0 && point.Y < _mapHeight)
            {
                // 减少像素读取次数（仅在必要时读取）
                if (map.ReadPixel(point.X, point.Y) != Host.Color)
                {
                    map.UpdatePixel(point.X, point.Y, Host.Color);
                    Health--;
                }
            }
        }
        _lastWorldPosition = currentWorldPosition;
        if (Engine.GetPhysicsFrames() % 10 == 0)
        {
            var x = GlobalPosition.X;
            var y = GlobalPosition.Y;
            var mapX = _mapGlobalPos.X;
            var mapY = _mapGlobalPos.Y;
            
            if (x < mapX || x >= mapX + _mapWidth || 
                y < mapY || y >= mapY + _mapHeight)
            {
                ((Main)GetTree().CurrentScene).Rebound(this);
            }
        }
    }
}
