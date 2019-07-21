using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Hirame.Cybele
{
    [System.Serializable]
    public class WhiteNoise
    {
        [Range (0, 1)]
        [SerializeField] private float onChance = 0.5f;
        
        [SerializeField] private bool useNoiseCurves;
        [SerializeField] private AnimationCurve xNoiseCurve = AnimationCurve.Constant (0, 1, 1);
        [SerializeField] private AnimationCurve yNoiseCurve = AnimationCurve.Constant (0, 1, 1);

        public int[,] GetNoise (int width, int height)
        {
            var timer = new Stopwatch ();
            timer.Start ();
            
            var noise = new int[width, height];
            
            for (var x = 1; x < width - 1; x++)
            {
                for (var y = 1; y < height - 1; y++)
                {
                    var chance = useNoiseCurves
                        ? onChance * GetCurveNoise (x, y, width, height)
                        : onChance;
                    
                    if (Random.value > chance)
                        continue;
                        
                    noise[x, y] = 1;
                }
            }

            timer.Stop ();
            Debug.Log ($"WhiteNoise done in: {timer.Elapsed.TotalMilliseconds.ToString()}");
            return noise;
        }

        private float GetCurveNoise (int x, int y, int width, int height)
        {
            var xNoise = xNoiseCurve.Evaluate (x / (float) width);
            var yNoise = yNoiseCurve.Evaluate (y / (float) height);

            return xNoise * yNoise;
        }
    }

}