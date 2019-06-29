using UnityEngine;

namespace Hirame.Cybele
{
    public class NoiseDisplay : MonoBehaviour
    {
        public const int MaxIterations = 20;
        
        public Vector2Int Size;

        public string Seed;
        
        public Vector2Int Limits;
        
        [Range (0, 1)]
        public float FillPercent;

        [Range(0, MaxIterations)]
        public int Iterations;

        [HideInInspector]
        public int[,] Noise;
        
        public void GenerateNoise ()
        {
            Noise = NoiseSampler.GetNoise (Size, Seed.GetHashCode (), FillPercent);
            Noise = CellularSampler.GetCellularNoise (
                Noise, Limits, Iterations);
        }
    }

}