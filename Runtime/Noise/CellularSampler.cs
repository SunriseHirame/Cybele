using System;
using UnityEngine;

namespace Hirame.Cybele
{
    public static class CellularSampler
    {
        public static int[,] GetCellularNoise (int[,] noise, int threshold, int iterations)
        {
            var grid = noise;

            for (var i = 0; i < iterations; i++)
            {
                CellularAutomata (ref grid, new Vector2Int(threshold, threshold));
            }
            
            return grid;
        }
        
        public static int[,] GetCellularNoise (int[,] noise, Vector2Int threshold, int iterations)
        {
            var grid = noise;

            for (var i = 0; i < iterations; i++)
            {
                CellularAutomata (ref grid, threshold);
            }
            
            return grid;
        }
        
        public static int GetNeighbours (int x, int y, int[,] grid, int radius = 1)
        {
            var lx = grid.GetLength (0);
            var ly = grid.GetLength (1);
            
            var sx = Math.Max (x - radius, 0);
            var sy = Math.Max (y - radius, 0);
            var ex = Math.Min (x + radius, lx - 1);
            var ey = Math.Min (y + radius, ly - 1);

            var neighbours = -grid[x, y];
            
            for (var i = sx; i <= ex; i++)
            {
                for (var j = sy; j <= ey; j++)
                {
                    if (grid[i, j] == 1)
                        neighbours++;
                }
            }

            return neighbours;
        }
        
        private static void CellularAutomata (ref int[,] grid, in Vector2Int limits)
        {
            var lx = grid.GetLength (0);
            var ly = grid.GetLength (1);

            var source = (int[,]) grid.Clone ();

            for (var i = 0; i < lx; i++)
            {
                for (var j = 0; j < ly; j++)
                {
                    var nn = GetNeighbours (i, j, source);

                    if (nn < limits.x)
                    {
                        grid[i, j] = 0;
                    }

                    if (nn > limits.y)
                    {
                        grid[i, j] = 1;
                    }
                }
            }
        }
        
    }

}