using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Battery : Area2D
{
    [Export] public Color Color { get; set; }
    [Export] public Label RemainingAmmunitionQuantityLabel { get; set; }
    [Export] public Label HealthLabel { get; set; }
    [Export] public Marker2D ShootMarker { get; set; }
    [Export] public Sprite2D Sprite { get; set; }


    private int _quantityGenerated = 3;
    private float _dispersion = 0.1f;

    private System.Numerics.BigInteger _remainingAmmunitionQuantity = 5000;
    public System.Numerics.BigInteger RemainingAmmunitionQuantity
    {
        get => _remainingAmmunitionQuantity;
        set
        {
            _remainingAmmunitionQuantity = value;
            RemainingAmmunitionQuantityLabel.Text = value.ToString();
        }
    }
    public void body_entered(Node2D body)
    {
        if (body is BigBall bigBall && bigBall.Host != this)
        {
            Aim.Add(body);

            AimId = bigBall.id;
            // GetNode<Label>("Label").Text = "new BigBall";

        }
    }
    public void area_entered(Area2D area)
    {
        if (area is Bullet bullet && bullet.Host != this)
        {
            Aim.Add(area);
            // GetNode<Label>("Label").Text = "new Bullet";
            AimId = bullet.id;
        }
    }
    public readonly List<Node2D> Aim=[];
    public System.Numerics.BigInteger AimId;
    private System.Numerics.BigInteger _health = 1000000;
    public System.Numerics.BigInteger Health
    {
        get => _health;
        set
        {
            _health = value;
            HealthLabel.Text = value.ToString();
            if (_health <= 0)
            {
                GlobalPosition = new Vector2(114514, 114514);
                ProcessMode = ProcessModeEnum.Disabled;
            }
        }
    }
    private int BallCount = 3;
    public override void _Ready()
    {
        Sprite.SelfModulate = Color;
        // 初始化标签显示
        RemainingAmmunitionQuantityLabel.Text = RemainingAmmunitionQuantity.ToString();
        HealthLabel.Text = Health.ToString();
        for (int i = 0; i < BallCount; i++)
            AddBall(10);


    }
    public new void AreaEntered(Area2D area)
    {
        if (area is Bullet bullet&&bullet.Host!=this)
        {
            Health -= bullet.Health;
            bullet.TriggerDespawn();
        }
    }
    public void AddBall(System.Numerics.BigInteger value)
    {
        if (ProcessMode == ProcessModeEnum.Disabled) {
            return;
        }
        Ball ball = GD.Load<PackedScene>("res://ball.tscn").Instantiate<Ball>();
        ball.GlobalPosition = ((Main)GetTree().CurrentScene).BirthPos.GlobalPosition;
        ball.value = value;
        ball.Host = this;
        ((Main)GetTree().CurrentScene).BallMap.AddChild(ball);

    }
    public override void _Process(double delta)
    {
        
        
        for (int i = 0; i < _quantityGenerated; i++)
        {
            if (RemainingAmmunitionQuantity > 0)
            {
                RemainingAmmunitionQuantity--;
                if (Aim.Count == 0)
                    Sprite.Rotation += (float)delta * 2;
                else if (Aim[0] != null &&
                IsInstanceValid(Aim[0])&&
                ((Aim[0] is Bullet bullet && bullet.id == AimId)||(Aim[0] is BigBall bigBall && bigBall.id == AimId)))
                {
                    Sprite.LookAt(Aim[0].GlobalPosition);
                    Sprite.RotationDegrees += 90;
                }
                else
                {
                    Aim.RemoveAt(0);
                    Sprite.Rotation += (float)delta * 2;
                }
                Shoot(ShootMarker.GlobalPosition, Sprite.Rotation + (float)GD.RandRange(-_dispersion, _dispersion), 1000);
            }
            else
                break;
        }

    }
    
    private void Shoot(Vector2 pos,float rota,System.Numerics.BigInteger health,int speed=400)
    {
        if (ProcessMode == ProcessModeEnum.Disabled) {
            return;
        }
        Bullet bullet = ((BulletPool)((Main)GetTree().CurrentScene).BulletPool).GetBullet();
        // 设置子弹属性
        bullet.GlobalPosition = pos;
        bullet.Move = Vector2.Up.Rotated(rota) * speed;
        // bullet.Modulate = Color;
        bullet.Host = this;
        bullet.map = ((Main)GetTree().CurrentScene).Map;
        bullet.Reset(health);
    }

    public void AddBullets(System.Numerics.BigInteger value)
    {
        RemainingAmmunitionQuantity += value;
    }
    public void LaunchShotgunPellets(System.Numerics.BigInteger value)
    {
        int count;
        System.Numerics.BigInteger BulletHealth = 200;
        if (value > 100)
        {
            BulletHealth = 100+(value-100)/20;
            count = 100;
        }
        else
            count = (int)value;
        for (int i = 0; i < count;i++)
        {
            Shoot(ShootMarker.GlobalPosition, Sprite.Rotation+(float)GD.RandRange(-0.15,0.15), BulletHealth,600);
        }
    }
    public void AddHealth(System.Numerics.BigInteger value)
    {
        Health += value*10;
    }
    public void AddBigBall(System.Numerics.BigInteger value)
    {
        if (ProcessMode == ProcessModeEnum.Disabled)
        {
            return;
        }
        BigBall bigBall = GD.Load<PackedScene>("res://BigBall.tscn").Instantiate<BigBall>();
        bigBall.GlobalPosition = GlobalPosition;
        bigBall.LinearVelocity = new Vector2(GD.RandRange(-100, 100), GD.RandRange(-100, 100));
        bigBall.Host = this;
        bigBall.map = ((Main)GetTree().CurrentScene).Map;
        bigBall.Reset(value*100);
        ((Main)GetTree().CurrentScene).AddChild(bigBall);
    }
    public void AddSnipeBullet(System.Numerics.BigInteger value)
    {
        int count;
        System.Numerics.BigInteger BulletHealth = 100;
        if (value > 100)
        {
            BulletHealth = 100+(value-100)/20;
            count = 100;
        }
        else
            count = (int)value;
        for (int i = 0; i < count;i++)
        {
            Shoot(ShootMarker.GlobalPosition+new Vector2(GD.RandRange(-4,4),GD.RandRange(-4,4)), Sprite.Rotation, BulletHealth,600);
        }
    }

}
