using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassShaderProperties
{
    public static readonly int _LocalToWorldPropertiesId = Shader.PropertyToID("_LocalToWorldMatri");
    public static readonly int _GrassInfos = Shader.PropertyToID("_GrassInfos");
    public static readonly int _PassCount = Shader.PropertyToID("_PassCount");
    public static readonly int _UVOffest = Shader.PropertyToID("_UVOffest");
    public static readonly int _ColorTex = Shader.PropertyToID("_ColorTex");
        
}
