using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EndlessHunt
{
    [Serializable] public sealed class GearCardView { public Button button; public Image icon, selectedFrame; public Text title, status; }
    [Serializable] public sealed class WeaponCardView { public Button button; public Image icon, selectedFrame; public Text level, price; }
    [Serializable] public sealed class UpgradeRowView { public Text title, level, cost; public Button button; }

    public sealed class SurvivorMenu : MonoBehaviour
    {
        [Header("Canvas Character Previews")]
        public SkeletonGraphic battlePreview;
        public SkeletonGraphic equipmentPreview;
        public SkeletonGraphic storePreview;
        public WeaponDefinition[] weapons;

        [Header("Top Player Bar")]
        public Text playerNameText, playerLevelText, coinText, gemText, energyText, energyTimerText;

        [Header("Pages")]
        public GameObject homePage, equipmentPage, upgradePage, storePage;

        [Header("Five Button Navigation")]
        public Button homeTab, equipmentTab, upgradeTab, storeTab, arsenalTab;
        public Image[] tabHighlights;

        [Header("Battle Home")]
        public Text stageTitle, bestScoreText;
        public Button previousStageButton, nextStageButton, playButton;

        [Header("Survivor Style Equipment")]
        public Image[] equippedSlotIcons;
        public Text[] equippedSlotLevels;
        public Button[] equippedSlotButtons;
        public Text attackText, healthText;
        public GearCardView[] equipmentCards;
        public WeaponCardView[] inventoryWeaponCards;

        [Header("Weapon Store")]
        public WeaponCardView[] weaponCards;
        public Image weaponDetailIcon;
        public Text weaponDetailTitle, weaponDetailStats, weaponActionText, weaponUpgradeText;
        public Button weaponActionButton, weaponUpgradeButton;

        [Header("Upgrades")]
        public UpgradeRowView[] upgradeRows;

        [Header("Purchase Confirmation")]
        public GameObject confirmationPanel;
        public Image confirmationIcon;
        public Text confirmationTitle, confirmationBody, confirmationPrice;
        public Button confirmationAccept, confirmationCancel;

        [Header("Prepared Toast")]
        public GameObject toastPanel;
        public Text toastText;

        private Coroutine _toastRoutine;
        private int _selectedWeapon;
        private WeaponDefinition _pendingPurchase;

        private void Start()
        {
            homeTab.onClick.AddListener(() => ShowPage(0, 2));
            equipmentTab.onClick.AddListener(() => ShowPage(1, 1));
            upgradeTab.onClick.AddListener(() => ShowPage(2, 3));
            storeTab.onClick.AddListener(() => ShowPage(3, 0));
            arsenalTab.onClick.AddListener(() => ShowPage(3, 4));
            previousStageButton.onClick.AddListener(() => CycleStage(-1));
            nextStageButton.onClick.AddListener(() => CycleStage(1));
            playButton.onClick.AddListener(StartGame);

            for (int i = 0; i < equipmentCards.Length && i < GameCatalog.Gear.Length; i++)
            {
                int captured = i;
                equipmentCards[i].button.onClick.AddListener(() => SelectGear(GameCatalog.Gear[captured]));
            }
            for (int i = 0; i < weaponCards.Length && i < weapons.Length; i++)
            {
                int captured = i;
                weaponCards[i].button.onClick.AddListener(() => SelectStoreWeapon(captured));
            }
            for (int i = 0; i < inventoryWeaponCards.Length && i < weapons.Length; i++)
            {
                int captured = i;
                inventoryWeaponCards[i].button.onClick.AddListener(() => SelectInventoryWeapon(captured));
            }
            for (int i = 0; i < upgradeRows.Length; i++)
            {
                int captured = i;
                upgradeRows[i].button.onClick.AddListener(() => Upgrade(captured));
            }
            for (int i = 0; i < equippedSlotButtons.Length; i++)
            {
                int captured = i;
                equippedSlotButtons[i].onClick.AddListener(() => EquippedSlotPressed(captured));
            }

            weaponActionButton.onClick.AddListener(WeaponAction);
            weaponUpgradeButton.onClick.AddListener(UpgradeSelectedWeapon);
            confirmationAccept.onClick.AddListener(ConfirmPurchase);
            confirmationCancel.onClick.AddListener(CloseConfirmation);
            toastPanel.SetActive(false);
            confirmationPanel.SetActive(false);
            _selectedWeapon = Mathf.Max(0, Array.FindIndex(weapons, item => item != null && item.id == GameProfile.EquippedWeaponId));
            RefreshCharacters();
            ShowPage(0, 2);
        }

        private void Update()
        {
            coinText.text = GameProfile.Coins.ToString("N0");
            gemText.text = GameProfile.Gems.ToString("N0");
            energyText.text = GameProfile.Energy + "/30";
            energyTimerText.text = GameProfile.EnergyTimer();
            playerNameText.text = "Player " + (69252009 + GameProfile.BestScore);
            playerLevelText.text = "LV." + Mathf.Max(1, PlayerPrefs.GetInt("EH_PlayerLevel", 20));
        }

        private void ShowPage(int page, int navigationIndex)
        {
            homePage.SetActive(page == 0);
            equipmentPage.SetActive(page == 1);
            upgradePage.SetActive(page == 2);
            storePage.SetActive(page == 3);
            for (int i = 0; i < tabHighlights.Length; i++) tabHighlights[i].enabled = i == navigationIndex;
            if (page == 0) RefreshHome();
            else if (page == 1) RefreshEquipment();
            else if (page == 2) RefreshUpgrades();
            else RefreshWeapons();
        }

        private void RefreshCharacters()
        {
            RefreshCharacter(battlePreview);
            RefreshCharacter(equipmentPreview);
            RefreshCharacter(storePreview);
        }

        private void RefreshCharacter(SkeletonGraphic preview)
        {
            CharacterSkin.Apply(preview, weapons);
            SkeletonAnimation animation = preview.Animation as SkeletonAnimation;
            if (animation != null) animation.AnimationState.SetAnimation(0, "Idle", true);
        }

        private void RefreshHome()
        {
            int selected = GameProfile.SelectedStage;
            stageTitle.text = $"{selected + 1}. {GameCatalog.MapNames[selected]}";
            bestScoreText.text = $"En Uzun Hayatta Kalma:  {GameProfile.BestScore / 60:00}m {GameProfile.BestScore % 60:00}s";
        }

        private void CycleStage(int delta)
        {
            int max = Mathf.Min(GameProfile.HighestStage, GameCatalog.MapNames.Length);
            GameProfile.SelectedStage = (GameProfile.SelectedStage + delta + max) % max;
            RefreshHome();
        }

        private void StartGame()
        {
            if (!GameProfile.SpendEnergy(5)) { FlashMessage("YETERLİ ENERJİ YOK"); return; }
            SceneManager.LoadScene("GameScene");
        }

        private void RefreshEquipment()
        {
            RefreshGearCards();
            RefreshInventoryWeapons();
            WeaponDefinition weapon = CharacterSkin.FindWeapon(weapons, GameProfile.EquippedWeaponId);
            int weaponLevel = weapon != null ? GameProfile.WeaponLevel(weapon.id) : 1;
            equippedSlotIcons[0].sprite = weapon != null ? weapon.icon : null;
            equippedSlotLevels[0].text = "LV." + weaponLevel;
            GearSlot[] slots = { GearSlot.Top, GearSlot.Helmet, GearSlot.Bottom, GearSlot.Hair, GearSlot.Boots };
            for (int i = 0; i < slots.Length; i++)
            {
                GearDefinition gear = GameProfile.FindEquipped(slots[i]);
                equippedSlotIcons[i + 1].sprite = GearIcon(gear);
                equippedSlotLevels[i + 1].text = "LV." + PlayerPrefs.GetInt("EH_GearLevel_" + (gear?.id ?? "none"), 1);
            }
            float attack = weapon != null ? weapon.DamageAtLevel(weaponLevel) : 18f;
            int powerLevel = PlayerPrefs.GetInt("EH2_Upgrade_1", 1);
            int healthLevel = PlayerPrefs.GetInt("EH2_Upgrade_0", 1);
            attackText.text = "ATK   " + Mathf.RoundToInt(attack * (1f + (powerLevel - 1) * .05f));
            healthText.text = "HP   " + Mathf.RoundToInt(100f * (1f + (healthLevel - 1) * .05f));
        }

        private void RefreshGearCards()
        {
            for (int i = 0; i < equipmentCards.Length; i++)
            {
                GearDefinition gear = GameCatalog.Gear[i];
                bool owns = GameProfile.Owns(gear.id);
                bool equipped = GameProfile.Equipped(gear.slot) == gear.id;
                equipmentCards[i].selectedFrame.enabled = equipped;
                equipmentCards[i].status.text = owns ? (equipped ? "LV.1" : "GİY") : gear.price + "♦";
                equipmentCards[i].icon.color = owns ? Color.white : new Color(.42f,.45f,.55f,1f);
            }
        }

        private void RefreshInventoryWeapons()
        {
            for (int i = 0; i < inventoryWeaponCards.Length; i++)
            {
                WeaponDefinition weapon = weapons[i];
                bool owns = GameProfile.Owns(weapon.id);
                bool equipped = GameProfile.EquippedWeaponId == weapon.id;
                inventoryWeaponCards[i].selectedFrame.enabled = equipped;
                inventoryWeaponCards[i].level.text = owns ? "LV." + GameProfile.WeaponLevel(weapon.id) : "";
                inventoryWeaponCards[i].price.text = owns ? "" : weapon.price + "♦";
                inventoryWeaponCards[i].icon.color = owns ? Color.white : new Color(.38f,.4f,.48f,1f);
            }
        }

        private void SelectGear(GearDefinition gear)
        {
            if (!GameProfile.Buy(gear)) { FlashMessage("YETERLİ ELMAS YOK"); return; }
            RefreshCharacters(); RefreshEquipment(); FlashMessage(gear.title + " GİYİLDİ");
        }

        private void SelectInventoryWeapon(int index)
        {
            WeaponDefinition weapon = weapons[index];
            if (!GameProfile.Owns(weapon.id))
            {
                _selectedWeapon = index;
                ShowPage(3, 4);
                WeaponAction();
                return;
            }
            GameProfile.EquippedWeaponId = weapon.id;
            RefreshCharacters(); RefreshEquipment(); FlashMessage(weapon.title + " KUŞANILDI");
        }

        private void EquippedSlotPressed(int index)
        {
            if (index == 0) ShowPage(3, 4);
            else FlashMessage("Aşağıdaki envanterden yeni ekipman seçebilirsin");
        }

        private void SelectStoreWeapon(int index) { _selectedWeapon = Mathf.Clamp(index, 0, weapons.Length - 1); RefreshWeapons(); }

        private void RefreshWeapons()
        {
            for (int i = 0; i < weaponCards.Length; i++)
            {
                WeaponDefinition weapon = weapons[i];
                bool owns = GameProfile.Owns(weapon.id);
                bool equipped = GameProfile.EquippedWeaponId == weapon.id;
                weaponCards[i].selectedFrame.enabled = i == _selectedWeapon || equipped;
                weaponCards[i].level.text = owns ? "LV." + GameProfile.WeaponLevel(weapon.id) : "KİLİTLİ";
                weaponCards[i].price.text = owns ? (equipped ? "KUŞANILDI" : "SEÇ") : weapon.price + "♦";
                weaponCards[i].icon.color = owns ? Color.white : new Color(.42f,.45f,.55f,1f);
            }
            WeaponDefinition selected = weapons[_selectedWeapon];
            int level = GameProfile.WeaponLevel(selected.id);
            bool has = GameProfile.Owns(selected.id);
            weaponDetailIcon.sprite = selected.icon;
            weaponDetailTitle.text = selected.title;
            weaponDetailStats.text = $"ATK  {selected.DamageAtLevel(level):0}     MENZİL  {selected.attackRange:0.0}\nHIZ  {1f / selected.attackCooldown:0.0}/sn     LV.{level}";
            weaponActionText.text = has ? (GameProfile.EquippedWeaponId == selected.id ? "KUŞANILDI" : "KUŞAN") : selected.price + " ELMAS";
            weaponUpgradeText.text = GameProfile.WeaponUpgradeCost(selected) + " ALTIN";
            weaponUpgradeButton.interactable = has;
        }

        private void WeaponAction()
        {
            WeaponDefinition selected = weapons[_selectedWeapon];
            if (GameProfile.Owns(selected.id))
            {
                GameProfile.EquippedWeaponId = selected.id;
                RefreshCharacters(); RefreshWeapons(); FlashMessage(selected.title + " KUŞANILDI");
                return;
            }
            _pendingPurchase = selected;
            confirmationIcon.sprite = selected.icon;
            confirmationTitle.text = "SATIN AL";
            confirmationBody.text = selected.title + " silahını satın almak istiyor musun?\n\nATK: " + selected.baseDamage.ToString("0");
            confirmationPrice.text = selected.price + " ELMAS";
            confirmationPanel.SetActive(true);
        }

        private void ConfirmPurchase()
        {
            if (_pendingPurchase == null) return;
            if (!GameProfile.BuyWeapon(_pendingPurchase)) { CloseConfirmation(); FlashMessage("YETERLİ ELMAS YOK"); return; }
            string title = _pendingPurchase.title;
            CloseConfirmation(); RefreshCharacters(); RefreshWeapons(); FlashMessage(title + " SATIN ALINDI");
        }

        private void CloseConfirmation() { confirmationPanel.SetActive(false); _pendingPurchase = null; }

        private void UpgradeSelectedWeapon()
        {
            WeaponDefinition selected = weapons[_selectedWeapon];
            if (!GameProfile.UpgradeWeapon(selected)) { FlashMessage("YETERLİ ALTIN YOK"); return; }
            RefreshCharacters(); RefreshWeapons(); FlashMessage(selected.title + " GELİŞTİRİLDİ");
        }

        private void RefreshUpgrades()
        {
            string[] names = { "CAN", "HASAR", "HAREKET", "TOPLAMA ALANI" };
            for (int i = 0; i < upgradeRows.Length; i++)
            {
                int level = PlayerPrefs.GetInt("EH2_Upgrade_" + i, 1);
                upgradeRows[i].title.text = names[i];
                upgradeRows[i].level.text = $"LV.{level}   +%{(level - 1) * 5}";
                upgradeRows[i].cost.text = (150 * level) + " ALTIN";
            }
        }

        private void Upgrade(int index)
        {
            int level = PlayerPrefs.GetInt("EH2_Upgrade_" + index, 1);
            int cost = 150 * level;
            if (GameProfile.Coins < cost) { FlashMessage("YETERLİ ALTIN YOK"); return; }
            GameProfile.Coins -= cost;
            PlayerPrefs.SetInt("EH2_Upgrade_" + index, level + 1);
            PlayerPrefs.Save(); RefreshUpgrades();
        }

        private Sprite GearIcon(GearDefinition gear)
        {
            if (gear == null) return null;
            foreach (GearCardView card in equipmentCards)
                if (card.title != null && card.title.text == gear.title) return card.icon.sprite;
            return equipmentCards.Length > 0 ? equipmentCards[0].icon.sprite : null;
        }

        private void FlashMessage(string message)
        {
            if (_toastRoutine != null) StopCoroutine(_toastRoutine);
            _toastRoutine = StartCoroutine(ShowToast(message));
        }

        private IEnumerator ShowToast(string message)
        {
            toastText.text = message; toastPanel.SetActive(true);
            yield return new WaitForSecondsRealtime(1.4f);
            toastPanel.SetActive(false);
        }
    }
}
