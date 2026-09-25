using UnityEngine;
namespace EndlessHunt
{
    public sealed class GoblinSet : ScriptableObject
    {
        public string title;
        public Sprite[] run,attack,hurt,death;
        public float visibleHeight=1.1f;
        public Sprite Frame(Vector2 direction,float time,bool attacking=false,bool hurting=false)
        {
            int row=Mathf.Abs(direction.y)>Mathf.Abs(direction.x)?(direction.y>0?1:0):(direction.x>0?3:2);
            Sprite[] frames=hurting?hurt:attacking?attack:run;int count=frames.Length/4;
            return frames[row*count+(int)(time*(attacking?10:12))%count];
        }
    }
}
