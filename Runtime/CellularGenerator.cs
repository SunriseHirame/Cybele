
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Hirame.Cybele
{
    public class CellularGenerator : GeneratorBase, IGenerator
    {
        [SerializeField] private int width = 40;
        [SerializeField] private int height = 40;

        [Header ("Noise")]
        [SerializeField] private WhiteNoise baseNoise;

        [Header ("Automata")]
        [SerializeField] private CellularAutomata automata;


        [SerializeField] private Material white;
        [SerializeField] private Material black;
        [SerializeField] private Material blue;

                
        private int[,] cells;
        private GameObject[,] gos;
        
        public IEnumerator Generate ()
        {
            Clear ();

            CreateInitialObjects ();
            Debug.Log ("Starting Generation...");
            
            var timer = new Stopwatch ();
            timer.Start ();
            
            cells = baseNoise.GetNoise (width, height);
            automata.Apply (ref cells);
            
            timer.Stop ();
            Debug.Log ($"Generation finished in: {timer.Elapsed.TotalMilliseconds.ToString ()}");
            SetMaterials ();

            yield break;
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