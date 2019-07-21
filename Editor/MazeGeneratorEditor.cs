using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Hirame.Cybele.Editor
{
    [CustomEditor (typeof (GeneratorBase), true)]
    public class MazeGeneratorEditor : UnityEditor.Editor
    {
        private IGenerator generator;

        private EditorCoroutine genRoutine;
        
        private void OnEnable ()
        {
            generator = target as IGenerator;
        }

        private void OnDisable ()
        {
            if (genRoutine != null)
                EditorCoroutineUtility.StopCoroutine (genRoutine);
        }

        public override void OnInspectorGUI ()
        {
            if (generator == null)
            {
                OnEnable ();
                return;
            }

            using (var changed = new EditorGUI.ChangeCheckScope ())
            {
                DrawControlButtons ();
                
                DrawPropertiesExcluding (serializedObject, "m_Script");
                
                if (changed.changed)
                {
                    serializedObject.ApplyModifiedProperties ();
                }
            }
        }

        private void DrawControlButtons ()
        {
            if (GUILayout.Button ("Generate"))
            {
                if (genRoutine != null)
                    EditorCoroutineUtility.StopCoroutine (genRoutine);
                
                genRoutine = EditorCoroutineUtility.StartCoroutine (generator.Generate (), generator);
            }
        }
    }

}