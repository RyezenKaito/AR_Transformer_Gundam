using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static TurnBasedBattle.Editor.BattleAssetUtility;

namespace TurnBasedBattle.Editor
{
    /// <summary>Combines rigid pieces per parent/material, preserving each animated joint.</summary>
    public static class BattleMeshOptimizer
    {
        static string Signature(Material mat)
        {
            var s=new StringBuilder(mat.shader.name+"|"+mat.renderQueue+"|"+string.Join(",",mat.shaderKeywords.OrderBy(k=>k)));
            for(int i=0;i<mat.shader.GetPropertyCount();i++)
            {
                string p=mat.shader.GetPropertyName(i);s.Append('|').Append(p).Append(':');
                switch(mat.shader.GetPropertyType(i))
                {
                    case ShaderPropertyType.Color:s.Append(mat.GetColor(p).ToString("F5"));break;
                    case ShaderPropertyType.Vector:s.Append(mat.GetVector(p).ToString("F5"));break;
                    case ShaderPropertyType.Texture:s.Append(mat.GetTexture(p)?mat.GetTexture(p).GetEntityId().ToString():"none").Append(mat.GetTextureOffset(p)).Append(mat.GetTextureScale(p));break;
                    default:s.Append(mat.GetFloat(p).ToString("R",System.Globalization.CultureInfo.InvariantCulture));break;
                }
            }
            return s.ToString();
        }
        public static void Combine(Transform root,string assetPrefix,bool unifyMaterials=false)
        {
            var renderers=root.GetComponentsInChildren<MeshRenderer>(true);
            var canonical=new Dictionary<string,Material>();
            var replacements=new Dictionary<Material,Material>();
            foreach(var original in renderers.SelectMany(r=>r.sharedMaterials).Distinct())
            {
                if(original==null)continue;
                if(!unifyMaterials){replacements[original]=original;continue;}
                string key=Signature(original);
                if(original.name.StartsWith("UC_") && original.name.Contains("_RED_"))key+="|RedArmor";
                if(!canonical.TryGetValue(key,out var material))
                {
                    // Explicit source red armor labels retain a distinctive RX-0 accent.
                    if(!canonical.TryGetValue(key,out material))
                    {
                        material=new Material(original){name=assetPrefix+"_Palette_"+canonical.Count};
                        if(key.EndsWith("|RedArmor")){material.SetColor("_BaseColor",new Color(.63f,.025f,.035f));material.SetColor("_EmissionColor",new Color(.25f,.008f,.01f));material.EnableKeyword("_EMISSION");}
                        material=Save(material,Root+"/Materials/"+material.name+".mat");canonical[key]=material;
                    }
                }
                replacements[original]=material;
            }
            int groupIndex=0;
            foreach(var parentGroup in renderers.GroupBy(r=>r.transform.parent))
            {
                var parent=parentGroup.Key;if(parent==null)continue;
                var groups=new Dictionary<Material,List<CombineInstance>>();
                foreach(var renderer in parentGroup)
                {
                    var filter=renderer.GetComponent<MeshFilter>();if(filter==null || filter.sharedMesh==null)continue;
                    var mesh=filter.sharedMesh;var materials=renderer.sharedMaterials;
                    for(int sub=0;sub<mesh.subMeshCount;sub++)
                    {
                        var mat=replacements[materials[Mathf.Min(sub,materials.Length-1)]];
                        if(!groups.TryGetValue(mat,out var list)){list=new List<CombineInstance>();groups.Add(mat,list);}
                        list.Add(new CombineInstance{mesh=mesh,subMeshIndex=sub,transform=parent.worldToLocalMatrix*filter.transform.localToWorldMatrix});
                    }
                }
                int part=0;
                foreach(var pair in groups)
                {
                    string name=assetPrefix+"_"+groupIndex+"_"+part++;
                    var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray(),true,true);mesh.RecalculateBounds();
                    mesh=Save(mesh,Root+"/Meshes/"+name+".asset");
                    MeshObject("Combined_"+name,parent,mesh,pair.Key);
                }
                foreach(var renderer in parentGroup)
                {
                    if(renderer.transform.childCount==0)UnityEngine.Object.DestroyImmediate(renderer.gameObject);
                    else{UnityEngine.Object.DestroyImmediate(renderer.GetComponent<MeshFilter>());UnityEngine.Object.DestroyImmediate(renderer);}
                }
                groupIndex++;
            }
        }
    }
}
