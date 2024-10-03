using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ShaderPatcherWindow : EditorWindow
{
    private Shader shaderToPatch;

    // Menu item to show the shader patcher window
    [MenuItem("Bakery/BakeryAdapter/Patch Shader")]
    public static void ShowWindow()
    {
        GetWindow<ShaderPatcherWindow>("Patch Shader");
    }

    // GUI for the shader patcher window
    private void OnGUI()
    {
        GUILayout.Label("Patch Shader", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Shader To Patch:", GUILayout.Width(100));
        shaderToPatch = EditorGUILayout.ObjectField(shaderToPatch, typeof(Shader), false) as Shader;
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Patch") && shaderToPatch != null)
        {
            PatchShader();
        }
    }

    // Method to patch the shader
    private void PatchShader()
    {
        string shaderPath = AssetDatabase.GetAssetPath(shaderToPatch);

        // Text to search in the original shader and its corresponding replacement
        string[] searchTexts = {
            "_Volume0",
            "_Volume1",
            "_Volume2",
            "_Volume3",
            "_VolumeMask",
            "_GlobalVolumeMin",
            "_GlobalVolumeInvSize",
            "_GlobalVolumeMatrix"
        };

        string[] replacementTexts = {
            "_Udon_Volume0",
            "_Udon_Volume1",
            "_Udon_Volume2",
            "_Udon_Volume3",
            "_Udon_VolumeMask",
            "_Udon_GlobalVolumeMin",
            "_Udon_GlobalVolumeInvSize",
            "_Udon_GlobalVolumeMatrix"
        };

        if (string.IsNullOrEmpty(shaderPath))
        {
            Debug.LogError("Failed to get shader path.");
            return;
        }

        if (!File.Exists(shaderPath))
        {
            Debug.LogError("Shader file not found at path: " + shaderPath);
            return;
        }

        // Read the content of the original shader
        string shaderCode = File.ReadAllText(shaderPath);

        // Replace the texts
        for (int i = 0; i < searchTexts.Length; i++)
        {
            shaderCode = shaderCode.Replace(searchTexts[i], replacementTexts[i]);
        }

        // Search and include the referenced files in the includes
        string shaderDirectory = Path.GetDirectoryName(shaderPath);
        shaderCode = PatchIncludes(shaderCode, shaderDirectory);

        // Write the modified shader back to the file
        try
        {
            File.WriteAllText(shaderPath, shaderCode);
            Debug.Log("Shader patched successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to write patched shader to file: " + e.Message);
        }

        // Update the imported asset in the editor
        AssetDatabase.ImportAsset(shaderPath);
    }

    // Method to replace text and count occurrences
    private int ReplaceText(ref string input, string searchText, string replacementText)
    {
        int replacements = 0;
        int index = input.IndexOf(searchText);
        while (index != -1)
        {
            input = input.Remove(index, searchText.Length).Insert(index, replacementText);
            index = input.IndexOf(searchText, index + replacementText.Length);
            replacements++;
        }
        return replacements;
    }

    // Method to patch the includes
    private static string PatchIncludes(string shaderCode, string shaderDirectory)
    {
        // Pattern to search for includes
        Regex includePattern = new Regex(@"#include\s+[""'](.+?)[""']");

        // Find all matches of includes
        MatchCollection matches = includePattern.Matches(shaderCode);

        // Iterate through all matches
        foreach (Match match in matches)
        {
            // Get the name of the included file
            string includeFileName = match.Groups[1].Value;

            // Read the content of the included file if it exists
            string includeFilePath = Path.Combine(shaderDirectory, includeFileName);
            string includeFileContent = "";

            try
            {
                includeFileContent = File.ReadAllText(includeFilePath);
            }
            catch (FileNotFoundException)
            {
                Debug.LogWarning("Could not find included file: " + includeFilePath);
                continue; // Skip this file and go to the next include
            }

            // Replace in the content of the included file
            includeFileContent = PatchIncludes(includeFileContent, shaderDirectory);

            // Replace in the main shader
            shaderCode = shaderCode.Replace(match.Value, includeFileContent);
        }

        return shaderCode;
    }
}
