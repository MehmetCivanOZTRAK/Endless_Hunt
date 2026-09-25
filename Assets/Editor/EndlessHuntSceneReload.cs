using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

// One-shot handoff for externally verified scene assets, only when the open scene has no unsaved edits.
[InitializeOnLoad]
public static class EndlessHuntSceneReload
{
    private const string Request = "Logs/MobilePolishReload.request";
    private static double _next;
    static EndlessHuntSceneReload() => EditorApplication.update += Poll;
    private static void Poll()
    {
        if(EditorApplication.timeSinceStartup < _next) return;
        _next=EditorApplication.timeSinceStartup+2;
        if(File.Exists("Logs/PreparedUIEditorState.request"))
        {
            var state=new System.Text.StringBuilder();
            state.AppendLine("playing="+EditorApplication.isPlayingOrWillChangePlaymode);
            state.AppendLine("compiling="+EditorApplication.isCompiling);
            state.AppendLine("updating="+EditorApplication.isUpdating);
            for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
            {
                var scene=UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                state.AppendLine("scene="+scene.path+";dirty="+scene.isDirty);
            }
            File.WriteAllText("Logs/PreparedUIEditorState.txt",state.ToString());
            File.Delete("Logs/PreparedUIEditorState.request");
        }
        if(!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
            if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) return;
        File.Delete(Request);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        File.WriteAllText("Logs/MobilePolishReload.done","Verified mobile menu loaded.");
    }
}
