using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace EndlessHunt.UI
{
    [DefaultExecutionOrder(60)]
    public sealed class DemoBattle : MonoBehaviour
    {
        public SurvivalGame game;
        public Transform safeArea;
        [Header("Prepared HUD references")]
        public Slider rewardBar;
        public TMP_Text rewardText, waveNotice;
        public CanvasGroup notice;
        public Image[] weaponIcons;
        public TMP_Text[] weaponRanks;
        int _lastWave;
        float _lastValue=-1;
        Tween _barTween;
        void Update()
        {
            if(!rewardBar)return;float v=game.RewardNormalized;
            if(!Mathf.Approximately(v,_lastValue)){_lastValue=v;_barTween?.Kill();_barTween=rewardBar.DOValue(v,.18f).SetUpdate(true);}
            rewardText.text="YETENEK "+(game.RewardsTaken+1)+"  •  "+game.RewardValue+" / "+game.RewardCost+" ELMAS";
            for(int i=0;i<weaponIcons.Length;i++){int rank=game.arsenal.ranks[i];weaponIcons[i].enabled=rank>0;weaponRanks[i].text=rank>0?"SV."+rank:"";}
            if(game.WaveNumber!=_lastWave)
            {
                _lastWave=game.WaveNumber;notice.DOKill();notice.alpha=1;
                waveNotice.text="DALGA "+_lastWave+(_lastWave==game.balance.firstRangedWave?" • TAŞ ATICILAR":_lastWave%5==0?" • ŞEF GOBLİN":" • HAZIR OL");
                notice.DOFade(0,.4f).SetDelay(2.2f);
            }
            if(game.WaveRemaining<=5){notice.DOKill();notice.alpha=1;waveNotice.text="DALGA "+(game.WaveNumber+1)+" YAKLAŞIYOR • "+Mathf.CeilToInt(game.WaveRemaining);}
        }
        void OnDestroy(){_barTween?.Kill();if(notice)notice.DOKill();}
    }
}
