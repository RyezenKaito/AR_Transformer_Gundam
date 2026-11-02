using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GundamLobby.Editor
{
    [InitializeOnLoad]
    public static class LobbyBuildRequest
    {
        const string Request = "Assets/GundamLobby/Build.request";
        static LobbyBuildRequest() { EditorApplication.delayCall += Check; }
        static void Check()
        {
            if (!File.Exists(Request)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Check; return; }
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            try
            {
                File.Delete(Request);
                GundamLobbyBuilder.Build();
                File.WriteAllText("Assets/GundamLobby/BuildStatus.txt", "SUCCESS " + DateTime.Now);
            }
            catch (Exception e)
            {
                File.WriteAllText("Assets/GundamLobby/BuildStatus.txt", "ERROR " + e);
                Debug.LogException(e);
            }
        }
    }
}
