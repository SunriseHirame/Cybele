using System.Collections.Generic;
using UnityEngine;

namespace Hirame.Cybele
{
    public static class PoissonSampler
    {
        public static List<Vector2> GeneratePoints (float radius, Vector2 planeSize, int maxFails = 30,
            int maxPoints = int.MaxValue)
        {
            var cellSize = radius / Mathf.Sqrt (2);
            var xResolution = Mathf.CeilToInt (planeSize.x / cellSize);
            var yResolution = Mathf.CeilToInt (planeSize.y / cellSize);
            var resolution = new Vector2Int (xResolution, yResolution);
            var grid = new Node[xResolution, yResolution];

            var fails = 0;
            var resultPoints = new List<Vector2> ();

            while (fails < maxFails)
            {
                var point = RandomPointOnPlane (planeSize);

                if (!IsValid (point, resolution, radius, cellSize, grid))
                {
                    fails++;
                    continue;
                }

                var gridPoint = WorldToGrid (point, cellSize);
                grid[gridPoint.x, gridPoint.y] = new Node
                {
                    Occupied = true,
                    Position = point
                };
                resultPoints.Add (point);

                if (resultPoints.Count == maxPoints)
                    break;
            }

            return resultPoints;
        }

        private static Vector2 RandomPointOnPlane (in Vector2 plane)
        {
            return new Vector2 (
                Random.value * plane.x,
                Random.value * plane.y
            );
        }

        private static bool IsValid (
            in Vector2 point, in Vector2Int resolution, float radius,
            float cellSize, Node[,] grid)
        {
            var gridPoint = WorldToGrid (point, cellSize);
            var xStart = Mathf.Max (0, gridPoint.x - 3);
            var yStart = Mathf.Max (0, gridPoint.y - 3);
            var xEnd = Mathf.Min (resolution.x - 1, gridPoint.x + 3);
            var yEnd = Mathf.Min (resolution.y - 1, gridPoint.y + 3);

            for (var y = yStart; y <= yEnd; y++)
            {
                for (var x = xStart; x <= xEnd; x++)
                {
                    if (grid[x, y].Occupied
                        && Overlaps (grid[x, y].Position, point, radius))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool Overlaps (Vector2 a, Vector2 b, float radius)
        {
            var overlap = radius + radius;
            return (a - b).sqrMagnitude <= overlap * overlap;
            //return (a - b).magnitude < radius + radius;
        }

        private static Vector2Int WorldToGrid (Vector2 worldPoint, float cellSize)
        {
            var x = (int) (worldPoint.x / cellSize);
            var y = (int) (worldPoint.y / cellSize);
            return new Vector2Int (x, y);
        }

        private struct Node
        {
            public bool Occupied;
            public Vector2 Position;
        }
    }
}