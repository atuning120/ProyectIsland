using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class BKShaderStripper : IPreprocessShaders
{
    public int callbackOrder { get { return 0; } }

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (shader.name.StartsWith("BK/"))
        {
            int originalCount = data.Count;
            for (int i = data.Count - 1; i >= 0; --i)
            {
                var keywords = data[i].shaderKeywordSet;
                
                // Strip Decal variants (DBuffer)
                if (HasKeyword(keywords, "_DBUFFER_MRT1") || 
                    HasKeyword(keywords, "_DBUFFER_MRT2") || 
                    HasKeyword(keywords, "_DBUFFER_MRT3"))
                {
                    data.RemoveAt(i);
                    continue;
                }

                // Strip Light Cookies
                if (HasKeyword(keywords, "_LIGHT_COOKIES"))
                {
                    data.RemoveAt(i);
                    continue;
                }
                
                // Strip Light Layers
                if (HasKeyword(keywords, "_LIGHT_LAYERS"))
                {
                    data.RemoveAt(i);
                    continue;
                }

                // Strip Debug Display
                if (HasKeyword(keywords, "DEBUG_DISPLAY"))
                {
                    data.RemoveAt(i);
                    continue;
                }
            }
            // Debug.Log($"BKShaderStripper: Stripped {originalCount - data.Count} variants from {shader.name} (Pass {snippet.passType})");
        }
    }

    bool HasKeyword(ShaderKeywordSet set, string keywordName)
    {
        foreach(var kw in set.GetShaderKeywords())
        {
            if (kw.name == keywordName) return true;
        }
        return false;
    }
}
