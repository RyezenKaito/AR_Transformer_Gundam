using UnityEngine;
namespace WarehouseArena
{
    [ExecuteAlways,RequireComponent(typeof(TextMesh))]
    public sealed class WarehouseFontAtlas : MonoBehaviour
    {
        void OnEnable(){Font.textureRebuilt+=Refresh;Refresh(GetComponent<TextMesh>().font);}
        void OnDisable(){Font.textureRebuilt-=Refresh;}
        void Refresh(Font font){var t=GetComponent<TextMesh>();var r=GetComponent<Renderer>();if(font && t.font==font && r.sharedMaterial)r.sharedMaterial.mainTexture=font.material.mainTexture;}
    }
}
