using UnityEngine;
using UnityEditor;

public class GundamAnimTester : EditorWindow
{
    [MenuItem("Tools/Gundam Animation Tester")]
    public static void ShowWindow()
    {
        GetWindow<GundamAnimTester>("Gundam Anim");
    }

    private string[] _anims = { "Action1", "Action2", "Appear", "Idle", "Static" };

    void OnGUI()
    {
        GUILayout.Label("Gundam Animation Tester", EditorStyles.boldLabel);
        GUILayout.Space(8);

        var gundam = GameObject.Find("extreme-application_onextreme_gundam");
        if (gundam == null) { EditorGUILayout.HelpBox("Gundam not found in scene!", MessageType.Error); return; }

        var anim = gundam.GetComponent<Animator>();
        if (anim == null) { EditorGUILayout.HelpBox("No Animator on Gundam!", MessageType.Error); return; }

        if (anim.runtimeAnimatorController == null)
        { EditorGUILayout.HelpBox("No AnimatorController assigned!", MessageType.Warning); return; }

        EditorGUILayout.HelpBox("Controller: " + anim.runtimeAnimatorController.name, MessageType.Info);
        GUILayout.Space(8);

        foreach (var name in _anims)
        {
            if (GUILayout.Button(name, GUILayout.Height(36)))
            {
                if (!Application.isPlaying)
                {
                    // Edit mode: dùng Play trực tiếp
                    anim.Play(name, 0, 0f);
                    anim.Update(0f);
                    Debug.Log("[GundamTester] Previewing: " + name);
                }
                else
                {
                    // Play mode: dùng Trigger
                    foreach (var p in _anims) anim.ResetTrigger(p);
                    anim.SetTrigger(name);
                    Debug.Log("[GundamTester] Triggered: " + name);
                }
            }
        }

        GUILayout.Space(12);
        GUILayout.Label("Runtime phím tắt (Play Mode):", EditorStyles.miniLabel);
        GUILayout.Label("1=Action1  2=Action2  3=Appear  4=Idle  5=Static", EditorStyles.miniLabel);
    }

    void OnInspectorUpdate() => Repaint();
}
