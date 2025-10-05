using Godot;

public partial class Main : Node2D
{
	[Export] public Node BulletPool { get; set; }
	[Export] public Vector2I TerritorySize { get; set; } = new Vector2I(1063, 646);
	[Export] public Map Map { get; set; }
	[Export] public Marker2D BirthPos { get; set; }
	[Export] public StaticBody2D BallMap { get; set; }

	public void Rebound(Node2D area)
	{
		if (area is Bullet bullet)
		{
			// 计算地图的实际边界（全局坐标）
			float mapLeft = Map.GlobalPosition.X;
			float mapRight = Map.GlobalPosition.X + TerritorySize.X;
			float mapTop = Map.GlobalPosition.Y;
			float mapBottom = Map.GlobalPosition.Y + TerritorySize.Y;

			// 子弹的全局位置
			Vector2 bulletPos = bullet.GlobalPosition;

			// X方向边界判断（左右反弹）
			if (bulletPos.X <= mapLeft || bulletPos.X >= mapRight)
			{
				// 反转X方向速度
				bullet.Move = new Vector2(-bullet.Move.X, bullet.Move.Y);
				// 修正位置，避免子弹卡在边界外反复反弹
				bullet.GlobalPosition = new Vector2(
					Mathf.Clamp(bulletPos.X, mapLeft, mapRight),  // 限制X在边界内
					bulletPos.Y
				);
			}

			// Y方向边界判断（上下反弹）
			if (bulletPos.Y <= mapTop || bulletPos.Y >= mapBottom)
			{
				// 反转Y方向速度
				bullet.Move = new Vector2(bullet.Move.X, -bullet.Move.Y);
				// 修正位置，避免子弹卡在边界外反复反弹
				bullet.GlobalPosition = new Vector2(
					bulletPos.X,
					Mathf.Clamp(bulletPos.Y, mapTop, mapBottom)  // 限制Y在边界内
				);
			}
		}
	}

	public void _on_scatter_body_entered(Node2D body)
	{
		if (body is Ball ball)
		{
			ball.Host.LaunchShotgunPellets(ball.value);
			ball.Host.AddBall(10);
			ball.QueueFree();
			
		}
	}
	public void _on_machine_body_entered(Node2D body)
	{
		if (body is Ball ball)
		{
			ball.Host.AddBullets(ball.value);
			ball.Host.AddBall(10);
			ball.QueueFree();
		}
	}
	public void _on_shield_body_entered(Node2D body)
	{
		if (body is Ball ball)
		{
			ball.Host.AddHealth(ball.value);
			ball.Host.AddBall(10);
			ball.QueueFree();
		}
	}
	public void _on_big_body_entered(Node2D body)
	{
		if (body is Ball ball)
		{
			ball.Host.AddBigBall(ball.value);
			ball.Host.AddBall(10);
			ball.QueueFree();
		}
	}

	public void _on_snipe_body_entered(Node2D body)
	{
		if (body is Ball ball)
		{
			ball.Host.AddSnipeBullet(ball.value);
			ball.Host.AddBall(10);
			ball.QueueFree();
		}
	}
	public void _on_x_4_body_entered(Node2D body)
	{
		if (body is Ball ball)
		{
			ball.Host.AddBall(ball.value*4);
			ball.QueueFree();
		}
	}
	public void _on_x_2_body_entered(Node2D body)
	{
		if (body is Ball ball)
		{
			ball.Host.AddBall(ball.value*2);
			ball.QueueFree();
		}
	}
	public void _on_x_8_body_entered(Node2D body)
	{
		if (body is Ball ball)
		{
			ball.Host.AddBall(ball.value*8);
			ball.QueueFree();
		}
	}
	
}
    