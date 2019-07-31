using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Hirame.Cybele
{
    [System.Serializable]
    public class CellularAutomata
    {
        [Range (0, 8)]
        [SerializeField] private int flipThreshold;
        
        [Range (1, 16)]
        [SerializeField] private int passesToApply;

        public void Apply (ref int[,] grid)
        {
            var timer = new Stopwatch ();
            timer.Start ();
            
            for (var i = 0; i < passesToApply; i++)
            {
                ResolvePass (ref grid);
            }
            timer.Stop ();
            Debug.Log ($"CellularAutomata finished in: {timer.Elapsed.TotalMilliseconds.ToString()}");
        }

        private void ResolvePass (ref int[,] grid)
        {
            var gridWidth = grid.GetLength (0);
            var gridHeight = grid.GetLength (1);
            
            var tempGrid = new int[gridWidth, gridHeight];

            for (var x = 0; x < gridWidth; x++)
            {
                for (var y = 0; y < gridHeight; y++)
                {
                    var nc = CountNeighbours (x, y, grid);
                    tempGrid[x, y] = nc > flipThreshold ? 1 : 0;
                }
            }
            
            grid = tempGrid;
        }

        private static int CountNeighbours (int posX, int posY, int[,] grid)
        {
            var neighbours = 0;

            var xMin = math.max (posX - 1, 0);
            var xMax = math.min (posX + 1, grid.GetLength (0) - 1);
            var yMin = math.max (posY - 1, 0);
            var yMax = math.min (posY + 1, grid.GetLength (1) - 1);
            
            for (var x = xMin; x <= xMax; x++)
            {
                for (var y = yMin; y <= yMax; y++)
                {
                    neighbours += grid[x, y];
                }
            }

            return neighbours;
        }
    }

}
