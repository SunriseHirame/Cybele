using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hirame.Cybele.Editor
{
    [CustomEditor (typeof (NoiseDisplay))]
    public class NoiseDisplayEditor : UnityEditor.Editor
    {
        private NoiseDisplay display;
        private bool autoUpdate;

        private List<GameObject> displayObjects = new List<GameObject> ();
        
        private void OnEnable ()
        {
            display = target as NoiseDisplay;
            display?.GenerateNoise ();
            GenerateDisplay ();
        }

        public override void OnInspectorGUI ()
        {
            autoUpdate = GUILayout.Toggle (autoUpdate, "Auto Update");

            serializedObject.Update ();
            using (var changed = new EditorGUI.ChangeCheckScope ())
            {                
                DrawPropertiesExcluding (serializedObject, 
                    "m_Script", "Iterations");

                using (new GUILayout.HorizontalScope ())
                {
                    var iterationProp = serializedObject.FindProperty ("Iterations");
                    EditorGUILayout.IntSlider (
                        iterationProp, 0, NoiseDisplay.MaxIterations);
                    
                    if (GUILayout.Button ("+"))
                    {
                        display.Iterations = 
                            Mathf.Min (++display.Iterations, NoiseDisplay.MaxIterations);
                        iterationProp.intValue = display.Iterations;
                        EditorUtility.SetDirty (this);
                    }
                    if (GUILayout.Button ("-"))
                    {
                        display.Iterations = Mathf.Max (--display.Iterations, 0);
                        iterationProp.intValue = display.Iterations;
                        EditorUtility.SetDirty (this);
                    }
                }

                if (changed.changed)
                {
                    serializedObject.ApplyModifiedProperties ();
                   
                    if (autoUpdate)
                    {
                        display.GenerateNoise ();
                        GenerateDisplay ();
                        return;
                    }
                }             
            }

            if (GUILayout.Button ("Update"))
            {
                display.GenerateNoise ();
                GenerateDisplay ();
                return;
            }
        }

        private void OnDisable ()
        {
            ClearDisplay ();
        }

        private void GenerateDisplay ()
        {
            ClearDisplay ();
            
            var noise = display.Noise;
            var lx = noise.GetLength (0);
            var ly = noise.GetLength (1);

            var unlit = Shader.Find ("Unlit/Color");
            var offCellMat = new Material (unlit) {color = Color.white};
            var onCellMat = new Material (unlit) {color = Color.black};

            displayObjects.Capacity = lx * ly;
            
            for (var i = 0; i < lx; i++)
            {
                for (var j = 0; j < ly; j++)
                {
                    var go = GameObject.CreatePrimitive (PrimitiveType.Cube);
                    displayObjects.Add (go);
                    
                    go.hideFlags = HideFlags.DontSave;
                    go.name = $"{i}, {j}";
                    
                    go.transform.position = new Vector3(0.5f + i, 0.5f + j, 0);
                    go.transform.SetParent (display.transform);
                    
                    var renderer = go.GetComponent<MeshRenderer> ();
                    renderer.sharedMaterial = noise[i, j] == 1 ? onCellMat : offCellMat;                 
                }
            }
        }

        private void ClearDisplay ()
        {
            foreach (var obj in displayObjects)
            {
                if (EditorApplication.isPlaying)
                    Destroy (obj);
                else
                    DestroyImmediate (obj);
            }

            displayObjects.Clear ();
        }
    }

}