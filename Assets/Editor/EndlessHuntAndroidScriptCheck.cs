using System.IO;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;
public static class EndlessHuntAndroidScriptCheck
{
    public static void Run()
    {
        Directory.CreateDirectory("Logs/AndroidScriptCheck");
        var result=PlayerBuildInterface.CompilePlayerScripts(new ScriptCompilationSettings{target=BuildTarget.Android,group=BuildTargetGroup.Android,options=ScriptCompilationOptions.DevelopmentBuild},"Logs/AndroidScriptCheck");
        if(result.assemblies==null||result.assemblies.Count==0)throw new System.Exception("Android script compilation produced no assemblies.");
        File.WriteAllText("Logs/android-script-check.txt","PASS: Android player C# compilation\n"+string.Join("\n",result.assemblies));Debug.Log("Android C# player compilation passed.");
    }
}
