using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hirame.Cybele
{
    public class MazeGenerator : GeneratorBase, IGenerator
    {
        [Header ("Size")]
        [SerializeField] private int width = 40;
        [SerializeField] private int height = 40;

        [SerializeField] private bool randomSeed = true;
        [SerializeField] private int seed = 423798;
        
        [Range (0, 1)]
        [SerializeField] private float turnChance = 0.4f;

        [Range (0, 1)]
        [SerializeField] private float turnDirectionFlip = 0.5f;

        [Header ("Wall kill")]
        [SerializeField] private int maxWallsToKill;
        
        
        [Header ("Colors")]
        [FormerlySerializedAs ("white")]
        [SerializeField] private Material floorMaterial;
        
        [FormerlySerializedAs ("black")] 
        [SerializeField] private Material wallMaterial;
        
        [FormerlySerializedAs ("blue")]
        [SerializeField] private Material visitedMaterial;

        [SerializeField] private Material killedWallMaterial;
        
        [Header ("Flood Fill")]
        [SerializeField] private Material[] areaColors;
        
        
        private int[,] cells;
        private GameObject[,] gos;

        private int floorTiles;

        public IEnumerator Generate ()
        {
            StopAllCoroutines ();
            
            Clear ();
            CreateInitialObjects ();

            if (randomSeed)
                seed = Random.Range (int.MinValue, int.MaxValue);

            floorTiles = 0;
            
            var stack = new Stack<Vector2Int> ();
            var visited = new HashSet<Vector2Int> ();
            
            var startPosition = new Vector2Int (Random.Range (1, width - 1), Random.Range (1, height - 1));
            var position = startPosition;
            var direction = Random.value > 0.5 
                ? new Vector2Int (Random.value > 0.5 ? -1 : 1, 0)
                : new Vector2Int (0, Random.value > 0.5 ? -1 : 1);
            
            do
            {
                print (direction);
                yield return null;
                if (IsOnMap (in position) && CountNeighbours (in position) <= 1)
                {
                    SetAsFloor (in position, visited);
                    TrackNeighbouringWalls (in position, stack, visited);
                    
                    position = GetRandomNeighbour (in position, ref direction);
                    
                    continue;
                }
                
                if (stack.Count > 0)
                {
                    position = stack.Pop ();
                }

            } while (stack.Count > 0);


            for (var i = 0; i < maxWallsToKill; i++)
            {
                var x = Random.Range (1, width - 1);
                var y = Random.Range (1, height - 1);

                if (cells[x, y] == 1)
                    continue;
                
                floorTiles++;
                cells[x, y] = 1;
                SetMaterial (new Vector2Int (x, y), killedWallMaterial);
                yield return null;
            }
            
            visited.Clear ();
            var floodQueue = new Queue<Vector2Int> ();
            
            position = startPosition;
            floodQueue.Enqueue (position);
            visited.Add (position);

            var neighbourList = new List<Vector2Int> (4);
            var tilesMapped = 0;
            
            while (floodQueue.Count > 0)
            {
                yield return null;
                
                position = floodQueue.Dequeue ();
                tilesMapped++;

                var fillIndex = (int) (tilesMapped / (float) floorTiles * areaColors.Length);
                SetMaterial (position, areaColors[fillIndex]);
                
                GetNeighbouringFloors (in position, neighbourList);

                foreach (var pos in neighbourList)
                {
                    if (visited.Contains (pos))
                        continue;
                    
                    floodQueue.Enqueue (pos);
                    visited.Add (pos);
                }
                
                neighbourList.Clear ();
            }
            
            Debug.Log ("DONE");
        }

        private Vector2Int GetRandomNeighbour (in Vector2Int position, ref Vector2Int direction)
        {
            // Does not turn
            if (Random.value > turnChance)
            {
                return position + direction;
            }

            direction = new Vector2Int (direction.y, direction.x) * (Random.value <= turnDirectionFlip ? -1 : 1);
            // Turns
            return position + direction;
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
                
                SetMaterial (xLeft, visitedMaterial);
            }            
            if (xRight.x < width && !visited.Contains (xRight))
            {
                stack.Push (xRight);
                visited.Add (xRight);

                SetMaterial (xRight, visitedMaterial);
            }            
            if (yDown.y >= 0 && !visited.Contains (yDown))
            {
                stack.Push (yDown);
                visited.Add (yDown);

                SetMaterial (yDown, visitedMaterial);
            }            
            if (yUp.y < height && !visited.Contains (yUp))
            {
                stack.Push (yUp);
                visited.Add (yUp);

                SetMaterial (yUp, visitedMaterial);
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

        private void GetNeighbouringFloors (in Vector2Int position, List<Vector2Int> neighbours)
        {
            var xMin = position.x - 1;
            var xMax = position.x + 1;
            var yMin = position.y - 1;
            var yMax = position.y + 1;
            
            if (cells[xMin, position.y] == 1)
                neighbours.Add (new Vector2Int(xMin, position.y));
            
            if (cells[xMax, position.y] == 1)
                neighbours.Add (new Vector2Int(xMax, position.y));
            
            if (cells[position.x, yMin] == 1)
                neighbours.Add (new Vector2Int(position.x, yMin));
            
            if (cells[position.x, yMax] == 1)
                neighbours.Add (new Vector2Int(position.x, yMax));
        }

        private void SetAsFloor (in Vector2Int position, HashSet<Vector2Int> visited)
        {
            floorTiles++;
            
            cells[position.x, position.y] = 1;
            visited.Add (position);
            
            SetMaterial (position, floorMaterial);
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

                    gos[x, y].GetComponent<MeshRenderer> ().sharedMaterial = wallMaterial;
                }
            }
        }
    }
    
}
