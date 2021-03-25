using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(AllIn1Shader)), CanEditMultipleObjects]
public class AllIn1ShaderScriptEditor : Editor
{
    enum ShaderTypes
    {
        Default,
        ScaledTime,
        MaskedUI,
        Invalid
        
    }
    ShaderTypes shaderTypes = ShaderTypes.Invalid;

    public override void OnInspectorGUI()
    {
        Texture2D imageInspector = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/AllIn1SpriteShader/Textures/CustomEditorImage.png", typeof(Texture2D));

        if (imageInspector)
        {
            Rect rect;
            float imageHeight = imageInspector.height;
            float imageWidth = imageInspector.width;
            float aspectRatio = imageHeight / imageWidth;
            rect = GUILayoutUtility.GetRect(imageHeight, aspectRatio * Screen.width);
            EditorGUI.DrawTextureTransparent(rect, imageInspector);
        }

        AllIn1Shader myScript = (AllIn1Shader)target;
        if (shaderTypes == ShaderTypes.Invalid) SetCurrentShaderType(myScript);

        if (GUILayout.Button("Deactivate All Effects"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                (targets[i] as AllIn1Shader).ClearAllKeywords();
            }
        }


        if (GUILayout.Button("New Clean Material"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                (targets[i] as AllIn1Shader).TryCreateNew();
            }
        }


        if (GUILayout.Button("Create New Material With Same Properties (SEE DOC)"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                (targets[i] as AllIn1Shader).MakeCopy();
            }
        }

        if (GUILayout.Button("Save Material To Folder (SEE DOC)"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                (targets[i] as AllIn1Shader).SaveMaterial();
            }
        }

        if (GUILayout.Button("Apply Material To All Children"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                (targets[i] as AllIn1Shader).ApplyMaterialToHierarchy();
            }
        }

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Change Shader Type:");
            int previousShaderType = (int)shaderTypes;
            shaderTypes = (ShaderTypes)EditorGUILayout.EnumPopup(shaderTypes);
            if (previousShaderType != (int)shaderTypes)
            {
                Debug.Log(myScript.gameObject.name + " shader has been changed to: " + shaderTypes);
                myScript.SetSceneDirty();

                SpriteRenderer sr = myScript.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Renderer r = myScript.GetComponent<Renderer>();
                    if (r.sharedMaterial != null)
                    {
                        if(shaderTypes == ShaderTypes.Default) r.sharedMaterial.shader = Resources.Load("AllIn1SpriteShader", typeof(Shader)) as Shader;
                        else if(shaderTypes == ShaderTypes.ScaledTime) r.sharedMaterial.shader = Resources.Load("AllIn1SpriteShaderScaledTime", typeof(Shader)) as Shader;
                        else if(shaderTypes == ShaderTypes.MaskedUI) r.sharedMaterial.shader = Resources.Load("AllIn1SpriteShaderUiMask", typeof(Shader)) as Shader;
                        else SetCurrentShaderType(myScript);
                    }
                }
                else
                {
                    Image img = myScript.GetComponent<Image>();
                    if (img.material != null)
                    {
                        if (shaderTypes == ShaderTypes.Default) img.material.shader = Resources.Load("AllIn1SpriteShader", typeof(Shader)) as Shader;
                        else if (shaderTypes == ShaderTypes.ScaledTime) img.material.shader = Resources.Load("AllIn1SpriteShaderScaledTime", typeof(Shader)) as Shader;
                        else if (shaderTypes == ShaderTypes.MaskedUI) img.material.shader = Resources.Load("AllIn1SpriteShaderUiMask", typeof(Shader)) as Shader;
                        else SetCurrentShaderType(myScript);
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Sprite Atlas Auto Setup"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                (targets[i] as AllIn1Shader).ToggleSetAtlasUvs(true);
            }
        }
        if (GUILayout.Button("Remove Sprite Atlas Configuration"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                (targets[i] as AllIn1Shader).ToggleSetAtlasUvs(false);
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("REMOVE COMPONENT AND MATERIAL"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                (targets[i] as AllIn1Shader).CleanMaterial();
            }
            for (int i = targets.Length - 1; i >= 0; i--)
            {
                DestroyImmediate(targets[i] as AllIn1Shader);
            }
        }
    }

    private void SetCurrentShaderType(AllIn1Shader myScript)
    {
        string shaderName = "";
        SpriteRenderer sr = myScript.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Renderer r = myScript.GetComponent<Renderer>();
            if (r.sharedMaterial != null) shaderName = r.sharedMaterial.shader.name;
        }
        else
        {
            Image img = myScript.GetComponent<Image>();
            if (img != null) shaderName = img.material.shader.name;
        }
        shaderName = shaderName.Replace("AllIn1SpriteShader/", "");

        if (shaderName.Equals("AllIn1SpriteShader")) shaderTypes = ShaderTypes.Default;
        else if (shaderName.Equals("AllIn1SpriteShaderScaledTime")) shaderTypes = ShaderTypes.ScaledTime;
        else if (shaderName.Equals("AllIn1SpriteShaderUiMask")) shaderTypes = ShaderTypes.MaskedUI;
    }
}