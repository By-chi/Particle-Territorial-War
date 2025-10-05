using Godot;
using System;
using System.Collections.Generic;

// 预定义结构体存储结果，避免频繁创建哈希集
public struct LinePointsResult
{
    public Vector2I[] Points;
    public int Count;
}
public static class PerformanceOptimizedSemicircleLine
{
    // 重用的临时数组池，避免频繁分配内存
    private static readonly Stack<Vector2I[]> _arrayPool = new Stack<Vector2I[]>();
    private static readonly int _maxCachedArraySize = 1000;

    /// <summary>
    /// 高性能版本：计算带有半圆端点的直线所穿过的所有整数点
    /// </summary>
    public static LinePointsResult GetAllPoints(Vector2 start, Vector2 end, float radius)
    {
        // 转换为整数坐标计算，减少浮点运算
        int startX = Mathf.RoundToInt(start.X);
        int startY = Mathf.RoundToInt(start.Y);
        int endX = Mathf.RoundToInt(end.X);
        int endY = Mathf.RoundToInt(end.Y);
        int r = Mathf.CeilToInt(radius);
        int rSquared = (int)(radius * radius + 1.0f); // 预计算半径平方
        
        // 估算所需最大点数，避免动态扩容
        int maxPointsEstimate = EstimateMaxPoints(start, end, radius);
        Vector2I[] points = GetArrayFromPool(maxPointsEstimate);
        int count = 0;
        
        // 计算直线方向向量（整数形式）
        int dirX = endX - startX;
        int dirY = endY - startY;
        int lengthSquared = dirX * dirX + dirY * dirY;
        
        // 处理点重合的特殊情况
        if (lengthSquared < 4) // 非常接近
        {
            count = AddCirclePoints(points, 0, startX, startY, r, rSquared);
            return new LinePointsResult { Points = points, Count = count };
        }
        
        // 计算直线部分的点（使用整数Bresenham算法的扩展）
        count = AddThickLinePoints(points, 0, startX, startY, endX, endY, r, rSquared, dirX, dirY, lengthSquared);
        
        // 计算起点半圆的点
        count = AddEndpointSemicirclePoints(
            points, count, 
            startX, startY, endX, endY, 
            r, rSquared, 
            true, dirX, dirY, lengthSquared);
        
        // 计算终点半圆的点
        count = AddEndpointSemicirclePoints(
            points, count, 
            endX, endY, startX, startY, 
            r, rSquared, 
            false, dirX, dirY, lengthSquared);
        
        return new LinePointsResult { Points = points, Count = count };
    }
    
    /// <summary>
    /// 从对象池获取数组，避免频繁内存分配
    /// </summary>
    private static Vector2I[] GetArrayFromPool(int minSize)
    {
        lock (_arrayPool)
        {
            while (_arrayPool.Count > 0)
            {
                var arr = _arrayPool.Pop();
                if (arr.Length >= minSize)
                {
                    return arr;
                }
            }
            return new Vector2I[minSize];
        }
    }
    
    /// <summary>
    /// 释放数组回对象池
    /// </summary>
    public static void ReleaseArray(Vector2I[] array)
    {
        if (array == null) return;
        
        lock (_arrayPool)
        {
            if (_arrayPool.Count < _maxCachedArraySize)
            {
                _arrayPool.Push(array);
            }
        }
    }
    
    /// <summary>
    /// 估算最大点数，减少内存分配
    /// </summary>
    private static int EstimateMaxPoints(Vector2 start, Vector2 end, float radius)
    {
        float length = start.DistanceTo(end);
        int linePoints = (int)(length * radius * 4 + 10);
        int circlePoints = (int)(Mathf.Pi * radius * radius * 2 + 10);
        return linePoints + circlePoints;
    }
    
    /// <summary>
    /// 添加带宽度的直线点（使用整数运算优化）
    /// </summary>
    private static int AddThickLinePoints(
        Vector2I[] buffer, int startIndex,
        int x0, int y0, int x1, int y1,
        int r, int rSquared,
        int dirX, int dirY, int lengthSquared)
    {
        int count = startIndex;
        int w = x1 - x0;
        int h = y1 - y0;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        
        // 确定步进方向
        if (w < 0) dx1 = -1;
        else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1;
        else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1;
        else if (w > 0) dx2 = 1;
        
        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);
        
        // 交换长轴短轴
        if (longest < shortest)
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0) dy2 = -1;
            else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        
        int numerator = longest >> 1;
        int x = x0, y = y0;
        
        // 沿直线步进
        for (int i = 0; i <= longest; i++)
        {
            // 添加当前点周围的点（线宽）
            count = AddPointsInRadius(buffer, count, x, y, r, rSquared, dirX, dirY, lengthSquared, false);
            
            numerator += shortest;
            if (numerator > longest)
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
        
        return count;
    }
    
    /// <summary>
    /// 添加端点半圆的点（整数运算优化）
    /// </summary>
    private static int AddEndpointSemicirclePoints(
        Vector2I[] buffer, int startIndex,
        int centerX, int centerY, int otherX, int otherY,
        int r, int rSquared,
        bool isStart,
        int dirX, int dirY, int lengthSquared)
    {
        int count = startIndex;
        
        // 计算半圆方向（使用整数点积）
        int semiDirX = centerX - otherX;
        int semiDirY = centerY - otherY;
        
        // 遍历可能在半圆内的点
        for (int dx = -r; dx <= r; dx++)
        {
            int x = centerX + dx;
            int dxSquared = dx * dx;
            
            // 提前计算y方向的最大可能范围
            int maxYDistance = (int)Mathf.Sqrt(rSquared - dxSquared);
            
            for (int dy = -maxYDistance; dy <= maxYDistance; dy++)
            {
                int y = centerY + dy;
                
                // 检查是否在半圆方向上（点积 >= 0）
                int dotProduct = dx * semiDirX + dy * semiDirY;
                if (dotProduct < 0)
                    continue;
                
                // 检查是否在圆内
                if (dxSquared + dy * dy <= rSquared)
                {
                    if (count < buffer.Length)
                    {
                        buffer[count++] = new Vector2I(x, y);
                    }
                }
            }
        }
        
        return count;
    }
    
    /// <summary>
    /// 添加圆形区域内的点
    /// </summary>
    private static int AddCirclePoints(
        Vector2I[] buffer, int startIndex,
        int centerX, int centerY, int r, int rSquared)
    {
        int count = startIndex;
        
        for (int dx = -r; dx <= r; dx++)
        {
            int x = centerX + dx;
            int dxSquared = dx * dx;
            int maxYDistance = (int)Mathf.Sqrt(rSquared - dxSquared);
            
            for (int dy = -maxYDistance; dy <= maxYDistance; dy++)
            {
                if (dxSquared + dy * dy <= rSquared)
                {
                    if (count < buffer.Length)
                    {
                        buffer[count++] = new Vector2I(x + dx, centerY + dy);
                    }
                }
            }
        }
        
        return count;
    }
    
    /// <summary>
    /// 添加半径范围内的点（用于线宽）
    /// </summary>
    private static int AddPointsInRadius(
        Vector2I[] buffer, int startIndex,
        int x, int y, int r, int rSquared,
        int dirX, int dirY, int lengthSquared, bool checkDirection)
    {
        int count = startIndex;
        
        for (int dx = -r; dx <= r; dx++)
        {
            int currentX = x + dx;
            int dxSquared = dx * dx;
            int maxYDistance = (int)Mathf.Sqrt(rSquared - dxSquared);
            
            for (int dy = -maxYDistance; dy <= maxYDistance; dy++)
            {
                if (dxSquared + dy * dy <= rSquared)
                {
                    // 对于线宽部分，可能需要检查是否在直线范围内
                    if (checkDirection && lengthSquared > 0)
                    {
                        int dot = dx * dirX + dy * dirY;
                        if (dot < 0 || dot > lengthSquared)
                            continue;
                    }
                    
                    if (count < buffer.Length)
                    {
                        buffer[count++] = new Vector2I(currentX, y + dy);
                    }
                }
            }
        }
        
        return count;
    }
}
