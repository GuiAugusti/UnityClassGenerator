using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ScriptGenerator
{
    public class ClassGenerator : EditorWindow
    {
        private static readonly string _templateWithoutInheritance =
@"using UnityEngine;

public class |SCRIPT|
{
    |OVERRIDEMETHODS|
}";

        private static readonly string _templateWithInheritance =
@"using UnityEngine;

public class |SCRIPT| : MonoBehaviour
{
    |OVERRIDEMETHODS|
}";

        private string _className = "";
        private bool _useMono = true;
        private bool _useOverride = false;
        private string _selectedFolderPath = "Assets/Scripts";
        private string _generatedScriptAssetPath;




        [MenuItem("HopeStudios/C# Class Generator", false)]
        public static void GenerateNewClass()
        {
            GetWindow<ClassGenerator>("C# Class Generator").Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Enter the name for the new C# class:");
            _className = EditorGUILayout.TextField(_className);

            GUIContent monoContent = new GUIContent("Use MonoBehaviour", "Enable to make the class inherit from MonoBehaviour");
            _useMono = EditorGUILayout.Toggle(monoContent, _useMono);

            GUIContent overrideContent = new GUIContent("Generate base methods", "Enable to automatically generate Awake, Start, and Update methods");
            _useOverride = EditorGUILayout.Toggle(overrideContent, _useOverride);

            GUILayout.Space(10);

            string relativeFolderPath = _selectedFolderPath.Replace(Application.dataPath, "Assets");
            GUILayout.Label("Generation Folder\n" + relativeFolderPath);

            if (GUILayout.Button("Select Folder"))
            {
                _selectedFolderPath = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Create"))
            {
                CreateScript();
            }

            if (GUILayout.Button("Find Generated Script"))
            {
                FindGeneratedScript();
            }          
            
            if (GUILayout.Button("Open Generated Script"))
            {
                OpenGeneratedScript();
            }      


        }


        private void FindGeneratedScript()
        {
            if (string.IsNullOrEmpty(_generatedScriptAssetPath))
            {
                Debug.Log("No script has been generated.");
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(_generatedScriptAssetPath);
            Object scriptAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            EditorGUIUtility.PingObject(scriptAsset);
            Selection.activeObject = scriptAsset;
        }

        private void OpenGeneratedScript()
        {
            if (string.IsNullOrEmpty(_generatedScriptAssetPath))
            {
                Debug.Log("No script has been generated.");
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(_generatedScriptAssetPath);
            Object scriptAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            AssetDatabase.OpenAsset(scriptAsset);
        }

        private void CreateScript()
        {
            if (string.IsNullOrEmpty(_className))
            {
                Debug.Log("Class name cannot be empty.");
                return;
            }

            if (string.IsNullOrEmpty(_selectedFolderPath))
            {
                Debug.Log("No folder selected.");
                return;
            }

            string template = _useMono ? _templateWithInheritance : _templateWithoutInheritance;
            string overrideMethods = _useOverride ? GenerateOverrideMethods() : "";

            string assetPathAndName = GenerateUniqueAssetPath(_selectedFolderPath, _className);

            template = template.Replace("|SCRIPT|", _className);
            template = template.Replace("|OVERRIDEMETHODS|", overrideMethods);

            WriteScriptToFile(template, assetPathAndName);
            AssetDatabase.Refresh();

            // Get the relative asset path
            string relativeAssetPath = assetPathAndName.Replace(Application.dataPath, "Assets");

            // Set the generatedScriptAssetPath
            _generatedScriptAssetPath = AssetDatabase.AssetPathToGUID(relativeAssetPath);
        }




        private string GenerateOverrideMethods()
        {
            return $@"
    private void Awake()
    {{
        
    }}

    private void Start()
    {{
        
    }}

    private void Update()
    {{
        
    }}";
        }

        private string GenerateUniqueAssetPath(string folderPath, string className)
        {
            string assetPathAndName = Path.Combine(folderPath, $"{className}.cs");
            return AssetDatabase.GenerateUniqueAssetPath(assetPathAndName);
        }

        private void WriteScriptToFile(string template, string assetPathAndName)
        {
            try
            {
                File.WriteAllText(assetPathAndName, template);
                Debug.Log($"Created script: {assetPathAndName}");
            }
            catch (IOException e)
            {
                Debug.LogError($"Failed to create script: {assetPathAndName}\nError: {e.Message}");
            }
        }
    }
}
