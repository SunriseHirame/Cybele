using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hirame.Cybele
{
    public class MazeGenerator : GeneratorBase, IGenerator
    {
        [SerializeField] private int width = 40;
        [SerializeField] private int height = 40;

        [SerializeField] private bool randomSeed = true;
        [SerializeField] private int seed = 423798;
        [Range (0, 1)]
        [SerializeField] private float turnChance = 0.4f;

        [Range (0, 1)]
        [SerializeField] private float turnDirectionFlip = 0.5f;
        
        [SerializeField] private Material white;
        [SerializeField] private Material black;
        [SerializeField] private Material blue;
        
        private int[,] cells;
        private GameObject[,] gos;

        public IEnumerator Generate ()
        {
            StopAllCoroutines ();
            
            Clear ();
            CreateInitialObjects ();

            var stack = new Stack<Vector2Int> ();
            var visited = new HashSet<Vector2Int> ();
            
            var position = new Vector2Int (1, 1);
            var direction = new Vector2Int (0, 1);
            
            do
            {
                yield return null;
                if (IsOnMap (in position) && CountNeighbours (in position) <= 1)
                {
                    SetAsFloor (in position, visited);
                    TrackNeighbouringWalls (in position, stack, visited);
                    
                    position = GetRandomNeighbour (in position, in direction);
                    
                    continue;
                }
                
                if (stack.Count > 0)
                {
                    position = stack.Pop ();
                }

            } while (stack.Count > 0);
            
            Debug.Log ("DONE");
        }

        private Vector2Int GetRandomNeighbour (in Vector2Int position, in Vector2Int direction)
        {
            // Does not turn
            if (Random.value > turnChance)
            {
                return position + direction;
            }
            // Turns
            return position + new Vector2Int (direction.y, direction.x) * (Random.value <= turnDirectionFlip ? -1 : 1);
        }

        private void TrackNeighbouringWalls (in Vector2Int position, Stack<Vector2Int> stack, HashSet<Vector2Int> visited)
        {
            var xLeft = new Vector2Int(position.x - 1, position.y);
            var xRight = new Vector2Int(position.x + 1, position.y);
            var yDown = new Vector2Int(position.x, position.y - 1);
            var yUp = new Vector2Int(position.x, position.y + 1);
            
            if (xLeft.x >= 0 && !visited.Contains (xLeft))
            {
                stack.Push (xLeft);
                visited.Add (xLeft);
                
                SetMaterial (xLeft, blue);
            }            
            if (xRight.x < width && !visited.Contains (xRight))
            {
                stack.Push (xRight);
                visited.Add (xRight);

                SetMaterial (xRight, blue);
            }            
            if (yDown.y >= 0 && !visited.Contains (yDown))
            {
                stack.Push (yDown);
                visited.Add (yDown);

                SetMaterial (yDown, blue);
            }            
            if (yUp.y < height && !visited.Contains (yUp))
            {
                stack.Push (yUp);
                visited.Add (yUp);

                SetMaterial (yUp, blue);
            }        
        }

        private bool IsOnMap (in Vector2Int position)
        {
            return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
        }

        private int CountNeighbours (in Vector2Int position)
        {
            var neighbours = 0;

            var xMin = position.x - 1;
            var xMax = position.x + 1;
            var yMin = position.y - 1;
            var yMax = position.y + 1;

            neighbours += xMin >= 0 ? cells[xMin, position.y] : 1;
            neighbours += xMax < width ? cells[xMax, position.y] : 1;
            neighbours += yMin >= 0 ? cells[position.x, yMin] : 1;
            neighbours += yMax < height ? cells[position.x, yMax] : 1;

            //print ($"{position} | {neighbours}");
            return neighbours;
        }

        private void SetAsFloor (in Vector2Int position, HashSet<Vector2Int> visited)
        {
            cells[position.x, position.y] = 1;
            visited.Add (position);
            
            SetMaterial (position, white);
        }

        private void SetMaterial (in Vector2Int position, Material material)
        {
            gos[position.x, position.y].GetComponent<Renderer> ().sharedMaterial = material;
        }

        private void Clear ()
        {
            while (transform.childCount > 0)
                DestroyImmediate (transform.GetChild (0).gameObject);
        }
        
        private void CreateInitialObjects ()
        {
            gos = new GameObject[width, height];
            cells = new int[width, height];
            
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    gos[x, y] = GameObject.CreatePrimitive (PrimitiveType.Cube);
                    gos[x, y].transform.SetParent (transform);
                    gos[x, y].transform.position = new Vector3(x + 0.5f, 0, y + 0.5f);

                    gos[x, y].GetComponent<MeshRenderer> ().sharedMaterial = black;
                }
            }
        }
    }
    
}
