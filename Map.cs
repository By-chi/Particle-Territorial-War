using Godot;
using System.Collections.Generic;

public partial class Map : Node2D
{
    [Export] public int _textureWidth = 256;
    [Export] public int _textureHeight = 256;
    [Export] private int _maxBatchSize = 1000; // 每帧最大处理的像素更新数

    private Image _image;
    private ImageTexture _imageTexture;
    private Color[,] _pixelCache; // CPU端像素缓存，用于快速读取
    private readonly Queue<(int X, int Y, Color Color)> _updateQueue = new();
    private ShaderMaterial _shaderMaterial;

    public override void _Ready()
    {
        // 使用新的API创建空图像（替换过时的Image.Create()）
        _image = Image.CreateEmpty(_textureWidth, _textureHeight, false, Image.Format.Rgba8);
        _imageTexture = ImageTexture.CreateFromImage(_image);
        
        // 初始化CPU缓存
        _pixelCache = new Color[_textureWidth, _textureHeight];
        for (int y = 0; y < _textureHeight; y++)
        {
            for (int x = 0; x < _textureWidth; x++)
            {
                _pixelCache[x, y] = Colors.Black;
            }
        }

        // 设置着色器材质
        _shaderMaterial = new ShaderMaterial
        {
            Shader = GD.Load<Shader>("res://pixel_shader.gdshader")
        };
        _shaderMaterial.SetShaderParameter("pixel_texture", _imageTexture);
        
        // 创建一个Sprite2D用于显示纹理
        var displaySprite = new Sprite2D();
        displaySprite.Material = _shaderMaterial;
        displaySprite.Texture = _imageTexture;
		displaySprite.GlobalPosition =new Vector2I(_textureWidth,_textureHeight)/2;
		displaySprite.ShowBehindParent = true;
		AddChild(displaySprite);

        // // 启动测试：每秒更新5000个随机像素
        // StartTestUpdates();
    }

    public override void _Process(double delta)
    {
        // 批量处理更新队列，控制每帧处理数量
        int processed = 0;
        while (_updateQueue.Count > 0 && processed < _maxBatchSize)
        {
            var update = _updateQueue.Dequeue();
            UpdatePixelInternal(update.X, update.Y, update.Color);
            processed++;
        }

        // 只有当有更新时才上传数据到GPU
        if (processed > 0)
        {
            _imageTexture.Update(_image);
        }
    }

    // 添加像素更新请求到队列
    public void UpdatePixel(int x, int y, Color color)
    {
        if (IsValidCoordinate(x, y))
        {
            _updateQueue.Enqueue((x, y, color));
        }
    }

    public Color ReadPixel(int x, int y)
    {
        if (IsValidCoordinate(x, y))
        {
            return _pixelCache[x, y];
        }
        return Colors.Transparent;
    }

    // 内部更新方法：同时更新CPU缓存和Image
    private void UpdatePixelInternal(int x, int y, Color color)
    {
        _pixelCache[x, y] = color;
        _image.SetPixel(x, y, color);
    }

    // 检查坐标是否有效
    public bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < _textureWidth && y >= 0 && y < _textureHeight;
    }
}
