using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Hirame.Cybele
{
    
    public static class NoiseSampler
    {
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static int[,] GetNoise (int width, int height, int seed, float fillPercent)
        {
            var prng = new System.Random (seed);
            var grid = new int[width, height];

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    grid[i, j] = prng.NextDouble () < fillPercent ? 1 : 0;
                }
            }

            return grid;
        }
        
        public static int[,] GetNoise (Vector2Int size, int seed, float fillPercent)
        {
            return GetNoise (size.x, size.y, seed, fillPercent);
        }
    }
    
}
