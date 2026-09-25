using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using EndlessHunt;
public static class EndlessHuntDemoInspect
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var m=Object.FindFirstObjectByType<SurvivorMenu>(); var s=new StringBuilder();
        foreach(var f in typeof(SurvivorMenu).GetFields()) if(typeof(Object).IsAssignableFrom(f.FieldType)) {var o=f.GetValue(m) as Object; s.AppendLine(f.Name+" = "+(o?o.name:"NULL"));}
        foreach(var t in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(t.GetComponentsInParent<Transform>(true).Length>7) continue;
            var i=t.GetComponent<Image>(); var txt=t.GetComponent<Text>();
            s.AppendLine(PathOf(t)+" | "+t.anchorMin+" "+t.anchorMax+" pos="+t.anchoredPosition+" size="+t.sizeDelta+" | "+(i?AssetDatabase.GetAssetPath(i.sprite):"")+" | "+(txt?txt.text.Replace("\n"," "):"")+" | "+string.Join(",",t.GetComponents<Component>().Select(x=>x?x.GetType().Name:"MISSING")));
        }
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/demo-inspect.txt",s.ToString());
    }
    static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
}

