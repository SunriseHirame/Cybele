using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = Unity.Mathematics.Random;

namespace Hirame.Cybele
{
    public class CellularGenerator : GeneratorBase, IGenerator
    {
        [SerializeField] private int width = 40;
        [SerializeField] private int height = 40;

        [Header ("Seed")]
        [SerializeField] private bool randomSeed;
        [SerializeField] private uint seed;
        
        [Header ("Noise")]
        [SerializeField] private WhiteNoise baseNoise;

        [Header ("Automata")]
        [SerializeField] private CellularAutomata automata;

        [Header ("Cleaning")]
        [SerializeField] private int minAreaSize = 4;
        
        [Header ("Visualization")]
        [SerializeField] private Material white;
        [SerializeField] private Material black;
        [SerializeField] private Material blue;

                
        private int[,] cells;
        private GameObject[,] gos;
        
        public IEnumerator Generate ()
        {
            if (randomSeed || seed == 0)
                seed = (uint) (UnityEngine.Random.value * uint.MaxValue);
            var rng = new Random (seed);
            
            Clear ();

            CreateInitialObjects ();
            Debug.Log ("Starting Generation...");
            
            var timer = new Stopwatch ();
            timer.Start ();
            
            cells = baseNoise.GetNoise (width, height, rng);
            automata.Apply (ref cells);

            Clean ();
            
            timer.Stop ();
            Debug.Log ($"Generation finished in: {timer.ElapsedMilliseconds.ToString ()}");
            SetMaterials ();

            yield break;
        }

        private void Clean ()
        {
            var visited = new HashSet<Vector2Int> ();
            var floodFill = new Queue<Vector2Int> ();

            var neighbouringFloors = new List<Vector2Int> (4);
            var areaSize = 0;
            
            for (var x = 0; x < width - 1; x++)
            {
                for (var y = 0; y < height - 1; y++)
                {
                    var point = new Vector2Int(x, y);
                    if (cells[point.x, point.y] == 0 || visited.Contains (point))
                        continue;

                    var area = new List<Vector2Int> ();
                    area.Add (point);
                    
                    floodFill.Enqueue (point);
                    
                    while (floodFill.Count > 0)
                    {
                        point = floodFill.Dequeue ();
                        visited.Add (point);
                        area.Add (point);
                        
                        areaSize++;
                        
                        GetNeighbouringFloors (point, neighbouringFloors);
                        
                        foreach (var neighbourFloor in neighbouringFloors)
                        {
                            if (visited.Contains (neighbourFloor))
                                continue;
                            
                            floodFill.Enqueue (neighbourFloor);
                            visited.Add (neighbourFloor);
                        }
                        
                        neighbouringFloors.Clear ();
                    }

                    if (areaSize < minAreaSize)
                    {
                        // TODO: fill area
                        Debug.Log ($"Area is too small: {point} | {areaSize}");

                        foreach (var p in area)
                        {
                            SetMaterial (p, black);
                            cells[p.x, p.y] = 0;
                        }
                    }

                    areaSize = 0;
                }
            }
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
        
        private void Clear ()
        {
            while (transform.childCount > 0)
                DestroyImmediate (transform.GetChild (0).gameObject);
        }
        
        private void SetMaterial (in Vector2Int position, Material material)
        {
            gos[position.x, position.y].GetComponent<Renderer> ().sharedMaterial = material;
        }

        private void SetMaterials ()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    SetMaterial (new Vector2Int (x, y), cells[x, y] == 0 ? black : white);
                }
            }
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

                    SetMaterial (new Vector2Int (x, y), black);
                }
            }
        }
    }

}