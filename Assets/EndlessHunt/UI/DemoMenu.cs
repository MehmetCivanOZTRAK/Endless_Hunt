using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace EndlessHunt.UI
{
    [Serializable] public sealed class PreparedPack { public Button buy; public int category, amount; }
    [Serializable] public sealed class PreparedDay { public Button button; public Image frame; public TMP_Text label; public GameObject claimed; }
    [DefaultExecutionOrder(50)]
    public sealed class DemoMenu : MonoBehaviour
    {
        public SurvivorMenu menu;
        public Transform overlayRoot, topBar;
        public Image avatarImage;
        [Header("Prepared pages")]
        public GameObject shopPage;
        public GameObject[] shopCategories;
        public Button[] categoryButtons;
        public PreparedPack[] packs;
        public Button[] talentButtons;
        public Image[] talentFrames;
        public TMP_Text[] talentLabels;
        [Header("Existing home and top-bar hit targets")]
        public Button optionsButton, leaderboardButton, weeklyButton, nameButton, avatarButton;
        public Button energyButton, goldButton, gemButton;
        [Header("Prepared dialogs")]
        public PreparedDialog weeklyDialog, rewardsDialog, optionsDialog, settingsDialog, profileDialog, leaderboardDialog, exitDialog, messageDialog;
        public PreparedDay[] rewardDays;
        public GameObject[] rewardGroups;
        public TMP_Text[] rewardAmounts;
        public Button[] optionActions;
        public Slider volumeSlider;
        public Button vibrationButton, damageNumbersButton, frameRateButton;
        public TMP_Text vibrationLabel, damageNumbersLabel, frameRateLabel;
        public TMP_InputField nameInput;
        public Button[] avatarButtons, profileButtons;
        public TMP_Text[] profileLabels, rankingLabels;
        public GameObject[] rankingRows;
        public RectTransform profileContent, rankingContent;
        public Button saveProfileButton, newProfileButton, quitButton;
        public TMP_Text messageTitle, messageBody;
        int _category;
        float _refresh;
        DemoTheme T => DemoTheme.Current;
        void Start()
        {
            menu.PageChanged += Page;
            optionsButton.onClick.AddListener(Options); leaderboardButton.onClick.AddListener(Leaderboard); weeklyButton.onClick.AddListener(Weekly);
            nameButton.onClick.AddListener(Profile); avatarButton.onClick.AddListener(Profile);
            energyButton.onClick.AddListener(()=>Shop(2)); goldButton.onClick.AddListener(()=>Shop(1)); gemButton.onClick.AddListener(()=>Shop(0));
            for(int i=0;i<categoryButtons.Length;i++){int n=i;categoryButtons[i].onClick.AddListener(()=>SelectCategory(n));}
            foreach(var pack in packs) { var p=pack; p.buy.onClick.AddListener(()=>Buy(p)); }
            for(int i=0;i<talentButtons.Length;i++){int n=i;talentButtons[i].onClick.AddListener(()=>Talent(n));}
            for(int i=0;i<rewardDays.Length;i++){int n=i;rewardDays[i].button.onClick.AddListener(()=>Claim(n));}
            Action[] actions={Settings,Messages,Profile,Weekly,Exit};
            for(int i=0;i<optionActions.Length;i++){int n=i;optionActions[i].onClick.AddListener(()=>{optionsDialog.Close();actions[n]();});}
            volumeSlider.onValueChanged.AddListener(v=>{LocalPlayerStore.Data.volume=v;SaveSettings();});
            vibrationButton.onClick.AddListener(()=>{LocalPlayerStore.Data.vibration=!LocalPlayerStore.Data.vibration;SaveSettings();});
            damageNumbersButton.onClick.AddListener(()=>{LocalPlayerStore.Data.damageNumbers=!LocalPlayerStore.Data.damageNumbers;SaveSettings();});
            frameRateButton.onClick.AddListener(()=>{LocalPlayerStore.Data.frameRate=LocalPlayerStore.Data.frameRate==60?30:60;SaveSettings();});
            for(int i=0;i<avatarButtons.Length;i++){int n=i;avatarButtons[i].onClick.AddListener(()=>{LocalPlayerStore.Current.avatar=n;LocalPlayerStore.Save();RefreshProfile();});}
            for(int i=0;i<profileButtons.Length;i++){int n=i;profileButtons[i].onClick.AddListener(()=>{if(n>=LocalPlayerStore.Data.hunters.Count)return;LocalPlayerStore.Select(n);SceneManager.LoadScene("MainMenu");});}
            saveProfileButton.onClick.AddListener(SaveProfile); newProfileButton.onClick.AddListener(NewProfile); quitButton.onClick.AddListener(()=>Application.Quit());
            SelectCategory(0); Page(0,2); RefreshProfile();
        }
        void Update(){_refresh-=Time.unscaledDeltaTime;if(_refresh<=0){_refresh=1;RefreshProfile();}}
        void RefreshProfile(){menu.RefreshProfile();if(avatarImage&&T.avatars.Length>0)avatarImage.sprite=T.avatars[LocalPlayerStore.Current.avatar%T.avatars.Length];}
        void Page(int page,int nav)
        {
            bool shop=page==3&&nav==0;shopPage.SetActive(shop);if(shop)menu.storePage.SetActive(false);
            if(page==2)RefreshTree();
        }
        public void Shop(int category){SelectCategory(category);menu.Navigate(3,0);}
        void SelectCategory(int category){_category=Mathf.Clamp(category,0,shopCategories.Length-1);for(int i=0;i<shopCategories.Length;i++)shopCategories[i].SetActive(i==_category);}
        void Buy(PreparedPack p)
        {
            if(p.category==0)GameProfile.Gems+=p.amount;else if(p.category==1)GameProfile.Coins+=p.amount;else GameProfile.AddEnergy(p.amount);
            RefreshProfile();Rewards(p.category==1?p.amount:0,p.category==0?p.amount:0,p.category==2?p.amount:0);
        }
        void RefreshTree()
        {
            for(int i=0;i<talentFrames.Length;i++)
            {
                bool owned=LocalPlayerStore.Current.talents.Contains(i),ready=LocalPlayerStore.CanUnlock(i);
                talentFrames[i].sprite=T.slots[owned?1:ready?2:0];talentFrames[i].color=owned||ready?Color.white:new Color(.48f,.5f,.56f);
                talentLabels[i].color=owned?new Color(.55f,1,.60f):Color.white;
            }
        }
        void Talent(int id)
        {
            if(LocalPlayerStore.Unlock(id)){RefreshTree();talentButtons[id].transform.DOPunchScale(Vector3.one*.08f,.2f,1).SetUpdate(true);return;}
            Message(LocalPlayerStore.TalentNames[id%6],LocalPlayerStore.Current.talents.Contains(id)?LocalPlayerStore.TalentDetails[id%6]+"\nBu yetenek kalıcı olarak açık.":"Bu daldaki önceki düğümü açmalısın.");
        }
        public void Weekly()
        {
            int current=LocalPlayerStore.Current.rewardDay%7;
            for(int i=0;i<rewardDays.Length;i++)
            {
                var d=rewardDays[i];d.frame.sprite=i==6?T.rewardWide:i==current&&LocalPlayerStore.RewardReady?T.rewardAvailable:T.rewardCard;
                d.label.text=i<current?"ALINDI":i==current&&!LocalPlayerStore.RewardReady?"YARIN":i==6?"1000 ALTIN + 300 ELMAS":(50*(i+1))+" ALTIN\n+"+(i%2==0?"5 ENERJİ":"50 ELMAS");
                d.claimed.SetActive(i<current);
            }
            weeklyDialog.Open();
        }
        void Claim(int day)
        {
            if(day!=LocalPlayerStore.Current.rewardDay%7||!LocalPlayerStore.RewardReady)return;
            if(LocalPlayerStore.ClaimDaily(out int coin,out int gem,out int energy)){weeklyDialog.Close();Rewards(coin,gem,energy);}
        }
        public void Rewards(int coins,int gems,int energy)
        {
            int[] values={coins,gems,energy};
            for(int i=0;i<3;i++){rewardGroups[i].SetActive(values[i]>0);rewardAmounts[i].text="x"+values[i].ToString("N0");}
            rewardsDialog.Open();
        }
        public void Options()=>optionsDialog.Open();
        public void Settings(){RefreshSettings();settingsDialog.Open();}
        void SaveSettings(){LocalPlayerStore.ApplySettings();LocalPlayerStore.Save();RefreshSettings();}
        void RefreshSettings()
        {
            volumeSlider.SetValueWithoutNotify(LocalPlayerStore.Data.volume);
            vibrationLabel.text="TİTREŞİM: "+(LocalPlayerStore.Data.vibration?"AÇIK":"KAPALI");
            damageNumbersLabel.text="HASAR SAYILARI: "+(LocalPlayerStore.Data.damageNumbers?"AÇIK":"KAPALI");
            frameRateLabel.text="AKICILIK: "+LocalPlayerStore.Data.frameRate+" FPS";
        }
        void Messages(){LocalPlayerStore.Data.messagesRead=true;LocalPlayerStore.Save();Message("MESAJLAR","AVCI KAMPINA HOŞ GELDİN\n\nTüm ekipmanlar denemeye açık.\nİlk haritada 3 dakika hayatta kalarak\nyeni bölgenin kilidini aç.\n\nHaftalık ödülünü her gün almayı unutma!");}
        public void Profile()
        {
            nameInput.SetTextWithoutNotify(LocalPlayerStore.Current.name);
            for(int i=0;i<profileButtons.Length;i++){bool exists=i<LocalPlayerStore.Data.hunters.Count;profileButtons[i].gameObject.SetActive(exists);if(exists)profileLabels[i].text=LocalPlayerStore.Data.hunters[i].name+(i==LocalPlayerStore.Data.active?" • AKTİF":"");}
            profileContent.sizeDelta=new Vector2(profileContent.sizeDelta.x,LocalPlayerStore.Data.hunters.Count*88);
            profileDialog.Open();
        }
        void SaveProfile()
        {
            string name=nameInput.text.Trim();if(name.Length<2){Message("İSİM","En az 2 karakter yaz.");return;}
            LocalPlayerStore.Current.name=name;LocalPlayerStore.Save();RefreshProfile();profileDialog.Close();
        }
        void NewProfile(){if(!LocalPlayerStore.Create(nameInput.text)){Message("KAYIT","2–16 karakter bir isim gir. Bu cihazda en fazla 8 profil saklanır.");return;}SceneManager.LoadScene("MainMenu");}
        public void Leaderboard()
        {
            var ranking=LocalPlayerStore.Ranking();
            for(int i=0;i<rankingRows.Length;i++){bool exists=i<ranking.Count;rankingRows[i].SetActive(exists);if(exists){var h=ranking[i];rankingLabels[i].text=(i+1)+".  "+h.name+"    "+h.bestScore.ToString("N0")+"   •   D"+h.bestWave;}}
            rankingContent.sizeDelta=new Vector2(rankingContent.sizeDelta.x,ranking.Count*98);leaderboardDialog.Open();
        }
        public void Exit()=>exitDialog.Open();
        public void Message(string title,string body){messageTitle.text=title;messageBody.text=body;messageDialog.Open();}
        void OnDestroy(){if(menu)menu.PageChanged-=Page;}
    }
}
