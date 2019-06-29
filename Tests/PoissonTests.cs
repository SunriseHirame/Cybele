using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hirame.Cybele.Tests
{
    public class PoissonTests
    {
        [Test]
        public void CreatesPoints ()
        {
            var points = PoissonSampler.GeneratePoints (
                1, new Vector2 (10, 10));
            Assert.Greater (points.Count, 0);
        }

        [Test]
        public void MaxPoints ()
        {
            const int maxPoints = 10;
            var points = PoissonSampler.GeneratePoints (
                1, new Vector2 (10, 10), 30, maxPoints);
            Assert.LessOrEqual (points.Count, maxPoints);
        }

        [UnityTest]
        public IEnumerator NoOverlap ()
        {
            const float radius = 0.5f;
            var points = PoissonSampler.GeneratePoints (
                radius, new Vector2 (40, 40));
            var nonOverlapping = 0;
            
            var light = new GameObject("Directional Light").AddComponent<Light> ();
            light.type = LightType.Directional;
            
            for (var i = 0; i < points.Count; i++)
            {
                var go = GameObject.CreatePrimitive (PrimitiveType.Sphere);
                go.transform.position = points[i];          
                var earlier = i - 1;
                
                for (; earlier >= 0; earlier--)
                {
                    if (!Overlaps (points[i], points[earlier], radius))
                        continue;
                        
                    go.GetComponent<MeshRenderer> ().material.color = Color.red;
                    nonOverlapping--;
                    break;
                }         
                
                nonOverlapping++;         
                //yield return null;
            }

            yield return new WaitForSeconds (1);

            Assert.True (points.Count == nonOverlapping);
        }

        private static bool Overlaps (Vector2 a, Vector2 b, float radius)
        {
            return (a - b).magnitude < radius + radius;
        }
    }
}
