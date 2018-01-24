using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderLib : MonoBehaviour
{
    public bool useShaderReplace;
    public Shader[] shaderGroup;
    public static Dictionary<string, Shader> lib;

    bool hasRecorded;

    void Start()
    {
        if (useShaderReplace&&!hasRecorded)
        {
            hasRecorded = true;
            lib = new Dictionary<string, Shader>();

            for (int i = 0; i < shaderGroup.Length; i++)
            {
                lib.Add(shaderGroup[i].name, shaderGroup[i]);
            }
        }
    }

}
