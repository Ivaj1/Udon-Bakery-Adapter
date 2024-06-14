using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ShaderPatcherWindow : EditorWindow
{
    private Shader shaderToPatch;

    [MenuItem("Bakery/BakeryAdapter/Patch Shader")]
    public static void ShowWindow()
    {
        GetWindow<ShaderPatcherWindow>("Patch Shader");
    }

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

    private void PatchShader()
    {
        string shaderPath = AssetDatabase.GetAssetPath(shaderToPatch);

        // Texto a buscar en el shader original y su correspondiente reemplazo
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

        // Leer el contenido del shader original
        string shaderCode = File.ReadAllText(shaderPath);

        // Reemplazar los textos
        for (int i = 0; i < searchTexts.Length; i++)
        {
            int replacements = ReplaceText(ref shaderCode, searchTexts[i], replacementTexts[i]);
            Debug.Log("Replaced " + replacements + " occurrences of '" + searchTexts[i] + "'.");
        }

        // Buscar e incluir los archivos referenciados en los includes
        string shaderDirectory = Path.GetDirectoryName(shaderPath);
        shaderCode = PatchIncludes(shaderCode, shaderDirectory);

        // Escribir el shader modificado de vuelta al archivo
        try
        {
            File.WriteAllText(shaderPath, shaderCode);
            Debug.Log("Shader patched successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to write patched shader to file: " + e.Message);
        }

        // Actualizar el asset importado en el editor
        AssetDatabase.ImportAsset(shaderPath);
    }

    // Método para reemplazar texto y contar las ocurrencias
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

    // Método para parchear los includes
    private static string PatchIncludes(string shaderCode, string shaderDirectory)
    {
        // Patrón para buscar includes
        Regex includePattern = new Regex(@"#include\s+[""'](.+?)[""']");

        // Buscar todos los matches de includes
        MatchCollection matches = includePattern.Matches(shaderCode);

        // Recorrer todos los matches
        foreach (Match match in matches)
        {
            // Obtener el nombre del archivo incluido
            string includeFileName = match.Groups[1].Value;

            // Leer el contenido del archivo incluido si existe
            string includeFilePath = Path.Combine(shaderDirectory, includeFileName);
            string includeFileContent = "";

            try
            {
                includeFileContent = File.ReadAllText(includeFilePath);
            }
            catch (FileNotFoundException)
            {
                Debug.LogWarning("Could not find included file: " + includeFilePath);
                continue; // Saltar este archivo e ir al siguiente include
            }

            // Reemplazar en el contenido del archivo incluido
            includeFileContent = PatchIncludes(includeFileContent, shaderDirectory);

            // Reemplazar en el shader principal
            shaderCode = shaderCode.Replace(match.Value, includeFileContent);
        }

        return shaderCode;
    }
}
