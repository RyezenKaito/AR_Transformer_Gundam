using UnityEngine;
namespace BiogearLab
{
    [ExecuteAlways,RequireComponent(typeof(TextMesh))]
    public sealed class BiogearFontAtlas : MonoBehaviour
    {
        void OnEnable(){Font.textureRebuilt+=Refresh;Refresh(GetComponent<TextMesh>().font);}
        void OnDisable(){Font.textureRebuilt-=Refresh;}
        void Refresh(Font font){var text=GetComponent<TextMesh>();if(!font||font!=text.font)return;var r=GetComponent<Renderer>();if(r.sharedMaterial)r.sharedMaterial.mainTexture=font.material.mainTexture;}
    }
}
