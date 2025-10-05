using Godot;
using System.Collections.Generic;

public static class OptimizedCirclePoints
{
    /// <summary>
    /// 高效获取圆形区域内的所有整数点
    /// 利用圆的对称性和数学特性减少计算量
    /// </summary>
    /// <param name="center">圆心坐标</param>
    /// <param name="radius">半径</param>
    /// <returns>圆形区域内所有整数点的集合</returns>
    public static HashSet<Vector2I> GetIntegerPointsInCircle(Vector2 center, float radius)
    {
        var result = new HashSet<Vector2I>();
        
        // 将圆心转换为整数坐标
        int centerX = Mathf.RoundToInt(center.X);
        int centerY = Mathf.RoundToInt(center.Y);
        
        // 使用半径的平方进行比较，避免开方运算
        float radiusSquared = radius * radius;
        
        // 计算需要检查的x范围
        int radiusInt = Mathf.CeilToInt(radius);
        int minX = centerX - radiusInt;
        int maxX = centerX + radiusInt;
        
        for (int x = minX; x <= maxX; x++)
        {
            // 计算当前x到圆心x的距离的平方
            float dx = x - center.X;
            float dxSquared = dx * dx;
            
            // 如果x方向的距离已经超过半径，无需计算y
            if (dxSquared > radiusSquared)
                continue;
            
            // 计算y方向允许的最大距离的平方
            float maxYSquared = radiusSquared - dxSquared;
            float maxYDistance = Mathf.Sqrt(maxYSquared);
            
            // 计算y的范围
            int minY = Mathf.FloorToInt(center.Y - maxYDistance);
            int maxY = Mathf.CeilToInt(center.Y + maxYDistance);
            
            // 添加当前x对应的所有y值
            for (int y = minY; y <= maxY; y++)
            {
                result.Add(new Vector2I(x, y));
            }
        }
        
        return result;
    }

    // 更优版本：利用圆的对称性进一步减少计算量
    public static HashSet<Vector2I> GetIntegerPointsInCircleSymmetric(Vector2 center, float radius)
    {
        var result = new HashSet<Vector2I>();
        
        int centerX = Mathf.RoundToInt(center.X);
        int centerY = Mathf.RoundToInt(center.Y);
        float radiusSquared = radius * radius;
        int radiusInt = Mathf.CeilToInt(radius);
        
        // 只计算右上四分之一圆，然后通过对称得到其他点
        for (int x = 0; x <= radiusInt; x++)
        {
            float dx = x;
            float dxSquared = dx * dx;
            
            if (dxSquared > radiusSquared)
                break;
                
            float maxYSquared = radiusSquared - dxSquared;
            int maxY = Mathf.FloorToInt(Mathf.Sqrt(maxYSquared));
            
            for (int y = 0; y <= maxY; y++)
            {
                // 通过对称性添加8个方向的点（除了轴上的点避免重复）
                AddSymmetricPoints(result, centerX, centerY, x, y);
            }
        }
        
        return result;
    }
    
    // 辅助方法：添加对称点
    private static void AddSymmetricPoints(HashSet<Vector2I> points, int centerX, int centerY, int x, int y)
    {
        // 处理x=0或y=0的情况，避免重复添加
        if (x == 0 && y == 0)
        {
            points.Add(new Vector2I(centerX, centerY));
            return;
        }
        
        if (x == 0)
        {
            points.Add(new Vector2I(centerX, centerY + y));
            points.Add(new Vector2I(centerX, centerY - y));
            return;
        }
        
        if (y == 0)
        {
            points.Add(new Vector2I(centerX + x, centerY));
            points.Add(new Vector2I(centerX - x, centerY));
            return;
        }
        
        // 一般情况，添加8个对称点
        points.Add(new Vector2I(centerX + x, centerY + y));
        points.Add(new Vector2I(centerX + x, centerY - y));
        points.Add(new Vector2I(centerX - x, centerY + y));
        points.Add(new Vector2I(centerX - x, centerY - y));
        points.Add(new Vector2I(centerX + y, centerY + x));
        points.Add(new Vector2I(centerX + y, centerY - x));
        points.Add(new Vector2I(centerX - y, centerY + x));
        points.Add(new Vector2I(centerX - y, centerY - x));
    }
}
