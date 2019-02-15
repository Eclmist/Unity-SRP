using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedColor : MonoBehaviour
{
    [SerializeField]
    Color color = Color.white;

    static MaterialPropertyBlock propertyBlock;
	static int colorID = Shader.PropertyToID("_Color");

    protected void Awake()
    {
        OnValidate();
    }

    protected void OnValidate()
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        propertyBlock.SetColor(colorID, color);
        GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
    }
}
