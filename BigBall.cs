using System.Text.RegularExpressions;
using Godot;

public partial class BigBall : RigidBody2D
{
    
    // 缓存常用变量，减少成员访问开销
    private int _mapWidth;
    private int _mapHeight;
    private Vector2 _mapGlobalPos;
    private System.Numerics.BigInteger Health;
    public System.Numerics.BigInteger id=0;
	
    
    // 带有 getter 和 setter 的属性
	public System.Numerics.BigInteger HealthDecorators
	{
		get => Health;
		set
		{
			meshInstance.Scale = Vector2.One * Mathf.Clamp((float)((double)value / 50000f), 3.0f, 25.0f);
			collisionShape.Scale = meshInstance.Scale;
			label.Text = value.ToString();
			Health = value;
			r = 8 * meshInstance.Scale.X;
			Mass = r;
		}
	}
	public new void BodyEntered(Node2D body)
	{
		if (body is BigBall bigBall && bigBall.Host != Host)
		{
			if (Health > bigBall.Health || (Health == bigBall.Health && id > bigBall.id))
			{
				Health -= bigBall.HealthDecorators / 2;
				bigBall.HealthDecorators /= 2;
				bigBall.HealthDecorators -= Health;
			}
		}
		else if (body.GetParent() is Battery battery&&battery!=Host)
        {
            Health -= battery.Health / 2;
            battery.Health /= 2;
            if (Health < 0)
                battery.Health -= Health;
        }
    }
	private float r = 0;
    public Battery Host;
    public Map map;
	[Export] MeshInstance2D meshInstance;
	[Export] CollisionShape2D collisionShape;
	[Export] Label label;
	public void Reset(System.Numerics.BigInteger health)
	{
		meshInstance.Modulate = Host.Color/2;
		HealthDecorators = health;
		if (map != null)
		{
			_mapWidth = map._textureWidth;
			_mapHeight = map._textureHeight;
			_mapGlobalPos = map.GlobalPosition;
		}
		id=Database.Instance.BulletHistoryCount;
	}
	
	
    public override void _PhysicsProcess(double delta)
	{
		if (HealthDecorators == 0)
			return;
		var currentWorldPosition = GlobalPosition;

		var mapLocalPosCurrent = map.ToLocal(currentWorldPosition);

		var endPixel = (Vector2I)mapLocalPosCurrent;
		var result = OptimizedCirclePoints.GetIntegerPointsInCircle(endPixel, r);

		// 用for循环替代foreach，减少迭代器开销
		foreach (Vector2I point in result)
		{
			// 合并边界检查（利用缓存的地图尺寸）
			if (point.X >= 0 && point.X < _mapWidth &&
				point.Y >= 0 && point.Y < _mapHeight)
			{
				// 减少像素读取次数（仅在必要时读取）
				if (map.ReadPixel(point.X, point.Y) != Host.Color)
				{
					map.UpdatePixel(point.X, point.Y, Host.Color);
					if (--HealthDecorators <= 0) // 合并判断和自减
					{
						QueueFree();
						break;
					}
				}
			}
		}
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
