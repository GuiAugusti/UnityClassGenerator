using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HopeStudios
{
    public class ClassGenerator : EditorWindow
    {
        private static readonly string TemplateWithoutInheritance =
@"using UnityEngine;

public class |SCRIPT|
{
    |OVERRIDEMETHODS|
}";

        private static readonly string TemplateWithInheritance =
@"using UnityEngine;

public class |SCRIPT| : MonoBehaviour
{
    |OVERRIDEMETHODS|
}";

        private string className = "";
        private bool useMono = true;
        private bool useOverride = false;
        private string selectedFolderPath = "Assets/Scripts";
        private string generatedScriptAssetPath;




        [MenuItem("HopeStudios/C# Class Generator", false)]
        public static void GenerateNewClass()
        {
            GetWindow<ClassGenerator>("C# Class Generator").Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Enter the name for the new C# class:");
            className = EditorGUILayout.TextField(className);

            GUIContent monoContent = new GUIContent("Use MonoBehaviour", "Enable to make the class inherit from MonoBehaviour");
            useMono = EditorGUILayout.Toggle(monoContent, useMono);

            GUIContent overrideContent = new GUIContent("Generate base methods", "Enable to automatically generate Awake, Start, and Update methods");
            useOverride = EditorGUILayout.Toggle(overrideContent, useOverride);

            GUILayout.Space(10);

            string relativeFolderPath = selectedFolderPath.Replace(Application.dataPath, "Assets");
            GUILayout.Label("Generation Folder\n" + relativeFolderPath);

            if (GUILayout.Button("Select Folder"))
            {
                selectedFolderPath = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");
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
            if (string.IsNullOrEmpty(generatedScriptAssetPath))
            {
                Debug.Log("No script has been generated.");
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(generatedScriptAssetPath);
            Object scriptAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            EditorGUIUtility.PingObject(scriptAsset);
            Selection.activeObject = scriptAsset;
        }

        private void OpenGeneratedScript()
        {
            if (string.IsNullOrEmpty(generatedScriptAssetPath))
            {
                Debug.Log("No script has been generated.");
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(generatedScriptAssetPath);
            Object scriptAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            AssetDatabase.OpenAsset(scriptAsset);
        }

        private void CreateScript()
        {
            if (string.IsNullOrEmpty(className))
            {
                Debug.Log("Class name cannot be empty.");
                return;
            }

            if (string.IsNullOrEmpty(selectedFolderPath))
            {
                Debug.Log("No folder selected.");
                return;
            }

            string template = useMono ? TemplateWithInheritance : TemplateWithoutInheritance;
            string overrideMethods = useOverride ? GenerateOverrideMethods() : "";

            string assetPathAndName = GenerateUniqueAssetPath(selectedFolderPath, className);

            template = template.Replace("|SCRIPT|", className);
            template = template.Replace("|OVERRIDEMETHODS|", overrideMethods);

            WriteScriptToFile(template, assetPathAndName);
            AssetDatabase.Refresh();

            // Get the relative asset path
            string relativeAssetPath = assetPathAndName.Replace(Application.dataPath, "Assets");

            // Set the generatedScriptAssetPath
            generatedScriptAssetPath = AssetDatabase.AssetPathToGUID(relativeAssetPath);
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
