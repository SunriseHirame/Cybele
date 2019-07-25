using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = Unity.Mathematics.Random;

namespace Hirame.Cybele
{
    [System.Serializable]
    public class WhiteNoise
    {
        [Range (0, 1)]
        [SerializeField] private float onChance = 0.5f;
        
        [Header ("Axial Noise")]
        [SerializeField] private bool useNoiseCurves;
        [SerializeField] private AnimationCurve noiseCurveX = AnimationCurve.Constant (0, 1, 1);
        [SerializeField] private AnimationCurve noiseCurveY = AnimationCurve.Constant (0, 1, 1);

        [Header ("Radial Noise")]
        [SerializeField] private bool useRadialNoise;
        [Range (0, 1)] [SerializeField] private float centerX = 0.5f;
        [Range (0, 1)] [SerializeField] private float centerY = 0.5f;
        [SerializeField] private AnimationCurve radianNoise = AnimationCurve.Constant (0, 1, 1);

        public int[,] GetNoise (int width, int height, Random random = new Random ())
        {
            var timer = new Stopwatch ();
            timer.Start ();
            
            var noise = new int[width, height];
            
            for (var x = 1; x < width - 1; x++)
            {
                for (var y = 1; y < height - 1; y++)
                {
                    var chance = onChance * GetCurveNoise (x, y, width, height);
                    
                    if (random.NextFloat() > chance)
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
            var noise = 1f;

            if (useNoiseCurves)
            {
                var xNoise = noiseCurveX.Evaluate (x / (float) width);
                var yNoise = noiseCurveY.Evaluate (y / (float) height);

                noise *= xNoise * yNoise;
            }


            if (useRadialNoise)
            {
                var dx = (x / (float) width - centerX) * 2;
                var dy = (y / (float) height - centerY) * 2;
                
                var distance = math.sqrt (dx * dx + dy * dy);
                noise *= radianNoise.Evaluate (distance);
            }
            
            return noise;
        }
    }

}