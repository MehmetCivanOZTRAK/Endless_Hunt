using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

namespace EndlessHunt
{
    public sealed class SurvivalGame : MonoBehaviour
    {
        [Header("Scene References")]
        public Camera gameplayCamera;
        public Transform worldRoot;
        public InfiniteMap materialWorld;
        public SurvivalBalance balance;
        public EnemyProjectile[] enemyProjectiles;
        public Transform[] mapChunks;
        public SpriteRenderer[] mapChunkRenderers;
        public SurvivalPlayer player;
        public Joystick movementJoystick;
        public WeaponDefinition[] weapons;

        [Header("HUD")]
        public Text timerText, waveText, scoreText, levelText;
        public Slider healthBar, xpBar;
        public Button pauseButton;
        public BattleFeedback feedback;
        public Image[] choiceIcons;
        public Sprite[] upgradeIcons;
        public Text[] choiceRanks;
        private readonly int[] _upgradeRanks = new int[6];

        [Header("Prepared Panels")]
        public GameObject levelUpPanel;
        public Text levelUpTitle;
        public Button[] choiceButtons;
        public Text[] choiceTexts;
        public GameObject pausePanel;
        public Button resumeButton, pauseMenuButton;
        public GameObject exitConfirmPanel;
        public Button exitConfirmButton, exitCancelButton;
        public GameObject gameOverPanel;
        public Text gameOverText;
        public Button retryButton, gameOverMenuButton;

        [Header("Prepared Pools (visible in Hierarchy)")]
        public SurvivalEnemy[] enemyPool;
        public ExperienceGem[] experiencePool;
        public Sprite[] enemySprites;
        public Sprite[] enemyWalkFrames;
        public float enemyVisibleHeight = .3f;
        public int forcedMapIndex = -1;
        public GoblinSet[] goblins;
        public RunArsenal arsenal;
        private int _rewardValue,_rewardNeeded,_crystals,_rewardsTaken;
        private float _nextRewardAt;
        private readonly EnemyCrowd _crowd = new();
        public int RewardsTaken => _rewardsTaken;
        public int RewardCost => _rewardNeeded;
        public int RewardValue => _rewardValue;
        private float _regenClock;
        private bool _recorded;
        public int WaveNumber=>_wave;
        public int RunLevel=>_level;
        public int Crystals=>_crystals;
        public float WaveRemaining=>Mathf.Max(0,_nextWave-_time);
        public float RewardNormalized=>Mathf.Clamp01((float)_rewardValue/_rewardNeeded);

        public readonly List<SurvivalEnemy> Enemies = new();
        private float _time, _spawnClock, _nextWave = 30f;
        private float _damageMultiplier = 1f, _moveSpeed = 3.8f, _pickupRadius = 1.4f;
        private int _wave = 1, _score, _kills, _level = 1, _map;
        private bool _ended, _choosing;
        private readonly int[] _shownChoices = new int[3];
        private WeaponDefinition _equippedWeapon;

        public SurvivalPlayer Player => player;
        public float MeleeDamageMultiplier { get => _damageMultiplier; set => _damageMultiplier = value; }
        public float Damage => (_equippedWeapon != null ? _equippedWeapon.DamageAtLevel(GameProfile.WeaponLevel(_equippedWeapon.id)) : 18f) * _damageMultiplier * (1+LocalPlayerStore.Talent(1)*.04f);
        public float PickupRadius => _pickupRadius * (1+LocalPlayerStore.Talent(3)*.05f);

        private void Start()
        {
            Time.timeScale = 1f;
            if (!balance) { Debug.LogError("Survival Balance is not assigned.", this); enabled = false; return; }
            _moveSpeed = balance.playerSpeed; _nextWave = balance.waveSeconds;
            _rewardNeeded = balance.RewardCost(0, 0);
            gameplayCamera.orthographicSize = 5.4f / Mathf.Max(.35f, gameplayCamera.aspect);
            _map = forcedMapIndex >= 0 ? forcedMapIndex : GameProfile.SelectedStage;
            _equippedWeapon = CharacterSkin.FindWeapon(weapons, GameProfile.EquippedWeaponId);
            Enemies.Clear();
            foreach (SurvivalEnemy enemy in enemyPool) enemy.Bind(this);
            foreach (ExperienceGem gem in experiencePool) gem.Bind(this);
            player.Setup(this, movementJoystick, 100f + GameProfile.GearHealthBonus + LocalPlayerStore.Talent(0)*10, _moveSpeed * (1+LocalPlayerStore.Talent(2)*.02f) * (1f + GameProfile.GearSpeedBonus), weapons);
            player.GetComponent<MeleeWeapon>().Setup(this, player, _equippedWeapon);
            pauseButton.onClick.AddListener(TogglePause);
            resumeButton.onClick.AddListener(Resume);
            pauseMenuButton.onClick.AddListener(ShowExitConfirmation);
            exitConfirmButton.onClick.AddListener(ReturnToMenu);
            exitCancelButton.onClick.AddListener(HideExitConfirmation);
            retryButton.onClick.AddListener(Retry);
            gameOverMenuButton.onClick.AddListener(ReturnToMenu);
            for (int i = 0; i < choiceButtons.Length; i++) { int c = i; choiceButtons[i].onClick.AddListener(() => ApplyUpgrade(_shownChoices[c])); }
            levelUpPanel.SetActive(false); pausePanel.SetActive(false); exitConfirmPanel.SetActive(false); gameOverPanel.SetActive(false);
            if (materialWorld) materialWorld.SelectMap(_map);
            UpdateHud();
        }

        private void Update()
        {
            if (_ended || _choosing || Time.timeScale == 0f) return;
            _time += Time.deltaTime; _spawnClock -= Time.deltaTime;
            _regenClock+=Time.deltaTime;if(_regenClock>=10){_regenClock=0;player.Heal(LocalPlayerStore.Talent(5));}
            if (_spawnClock <= 0f) { SpawnFormation(); _spawnClock = balance.SpawnInterval(_wave); }
            if (_time >= _nextWave) { _wave++; _nextWave += balance.waveSeconds; SpawnBurst(_wave % 5 == 0 ? 10 : 5, _wave % 5 == 0); }
            TryReward(); FollowCamera(); RepositionDistantEnemy(); UpdateHud();
        }

        private void LateUpdate()
        {
            if (!balance || _ended || _choosing || Time.timeScale == 0 || player.IsDead) return;
            _crowd.Resolve(Enemies, player.transform.position, balance.bodyRadius, Time.deltaTime);
        }

        private void ApplyMapPalette()
        {
            Color ground = GameCatalog.MapGround[Mathf.Clamp(_map, 0, GameCatalog.MapGround.Length - 1)];
            gameplayCamera.backgroundColor = ground;
            for (int i = 0; i < mapChunkRenderers.Length; i++) mapChunkRenderers[i].color = Color.Lerp(ground, Color.white, (i & 3) * .025f);
        }
        private void SpawnFormation() { int count = Mathf.Min(4, 1 + (_wave - 1) / 4); for (int i = 0; i < count; i++) SpawnEnemy(SpawnPoint(i, count), false); }
        private Vector2 SpawnPoint(int index, int count)
        {
            float halfY = gameplayCamera.orthographicSize + balance.spawnMargin;
            float halfX = gameplayCamera.orthographicSize * gameplayCamera.aspect + balance.spawnMargin;
            int side = (UnityEngine.Random.Range(0, 4) + index) % 4;
            var offset = side < 2 ? new Vector2(side == 0 ? -halfX : halfX, UnityEngine.Random.Range(-halfY, halfY)) :
                new Vector2(UnityEngine.Random.Range(-halfX, halfX), side == 2 ? -halfY : halfY);
            return (Vector2)gameplayCamera.transform.position + offset;
        }
        private void SpawnBurst(int count, bool elite) { for (int i = 0; i < count; i++) SpawnEnemy(SpawnPoint(i, count), elite && i == 0); }
        private void SpawnEnemy(Vector2 position, bool elite)
        {
            if (Enemies.Count >= Mathf.Min(balance.Population(_wave), enemyPool.Length)) return;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                bool occupied = false;
                foreach (var existing in Enemies) if (((Vector2)existing.transform.position - position).sqrMagnitude < 1.5f) { occupied = true; break; }
                if (!occupied) break;
                if (attempt == 7) return;
                position = SpawnPoint(attempt, 1);
            }
            SurvivalEnemy enemy = Array.Find(enemyPool, item => !item.gameObject.activeSelf); if (enemy == null) return;
            Sprite sprite = enemySprites != null && enemySprites.Length > 0 ? enemySprites[UnityEngine.Random.Range(0, enemySprites.Length)] : null;
            int type=elite?2:_wave>=5?UnityEngine.Random.Range(0,3):_wave>=3?UnityEngine.Random.Range(0,2):0;
            if(goblins!=null&&goblins.Length==3) enemy.SetSkin(goblins[type]);
            enemy.Activate(position, sprite, (22 + _wave * 7) * (elite ? 5 : 1), balance.EnemySpeed(_wave, type), elite ? 4 : 1, elite);
            int ranged = 0; foreach (var other in Enemies) if (other.IsRanged) ranged++;
            enemy.SetRanged(ranged < balance.RangedPopulation(_wave));
            Enemies.Add(enemy);
        }
        public bool ThrowAtPlayer(SurvivalEnemy enemy)
        {
            if (enemyProjectiles == null) return false;
            foreach (var shot in enemyProjectiles) if (!shot.gameObject.activeSelf) { shot.Launch(this, enemy); return true; }
            return false;
        }
        private void RepositionDistantEnemy()
        {
            float farX = gameplayCamera.orthographicSize * gameplayCamera.aspect * 3 + 10;
            float farY = gameplayCamera.orthographicSize * 2 + 10;
            foreach (var enemy in Enemies)
            {
                Vector3 delta = enemy.transform.position - gameplayCamera.transform.position;
                if (Mathf.Abs(delta.x) <= farX && Mathf.Abs(delta.y) <= farY) continue;
                for (int attempt = 0; attempt < 8; attempt++)
                {
                    var candidate = SpawnPoint(attempt, 1); bool occupied = false;
                    foreach (var other in Enemies) if (other != enemy && ((Vector2)other.transform.position - candidate).sqrMagnitude < 1.5f) { occupied = true; break; }
                    if (occupied) continue;
                    // Keep health and status: leaving the screen never awards XP or creates another enemy.
                    enemy.transform.position = candidate; return;
                }
                return;
            }
        }
        public void EnemyKilled(SurvivalEnemy enemy, int xp) { Enemies.Remove(enemy); _kills++; _score += 10 * xp; DropExperience(enemy.transform.position, xp); }
        private void DropExperience(Vector3 position, int amount)
        {
            ExperienceGem gem=Array.Find(experiencePool,item=>!item.gameObject.activeSelf);if(gem){gem.Activate(position,amount);return;}
            // Preserve experience when a long run fills the visual pool.
            ExperienceGem nearest=null;float best=float.MaxValue;foreach(var item in experiencePool){float d=(item.transform.position-position).sqrMagnitude;if(d<best){best=d;nearest=item;}}if(nearest)nearest.Accumulate(amount);
        }
        public void AddExperience(int amount)
        {
            if (amount < 0 || _ended) return;
            _rewardValue += amount; _crystals += amount;
            TryReward();
        }
        private void TryReward()
        {
            if (_rewardValue < _rewardNeeded || _choosing || _ended || player.IsDead || _time < _nextRewardAt) return;
            _rewardValue -= _rewardNeeded;
            ShowUpgradeChoices();
        }
        private void ShowUpgradeChoices()
        {
            _choosing = true; Time.timeScale = 0f;
            string[] choices = { "KESKİN DARBE\nHasar +%20", "HIZLI SALDIRI\nSaldırı hızı +%16", "GENİŞ MENZİL\nYakın dövüş +%18", "SÜRAT\nHareket +%10", "MANYETİK ALAN\nToplama +%30", "İLK YARDIM\nCan +25" };
            int[] pool = {0,1,2,3,4,5};
            for (int i = 0; i < 3; i++)
            {
                int pick = UnityEngine.Random.Range(i, pool.Length);
                (pool[i], pool[pick]) = (pool[pick], pool[i]);
                _shownChoices[i] = pool[i]; choiceTexts[i].text = choices[pool[i]];
                choiceIcons[i].transform.localScale=Vector3.one;
                if (choiceIcons != null && choiceIcons.Length > i && upgradeIcons.Length > pool[i]) choiceIcons[i].sprite = upgradeIcons[pool[i]];
                if (choiceRanks != null && choiceRanks.Length > i) choiceRanks[i].text = _upgradeRanks[pool[i]] == 0 ? "YENİ" : "SV. " + (_upgradeRanks[pool[i]] + 1);
            }
            int weapon=arsenal?arsenal.NextOffer(_level+1):-1;
            if(weapon>=0){_shownChoices[0]=6+weapon;choiceTexts[0].text=RunArsenal.Names[weapon]+"\n"+RunArsenal.Details[weapon]+"\n"+arsenal.Cooldown(weapon).ToString("0.0")+" sn";choiceIcons[0].sprite=arsenal.icons[weapon];choiceIcons[0].transform.localScale=Vector3.one*1.6f;choiceRanks[0].text=arsenal.ranks[weapon]==0?"YENİ SİLAH":"SV. "+(arsenal.ranks[weapon]+1);}
            UpdateHud();
            levelUpTitle.text = "YETENEK SEÇ"; levelUpPanel.SetActive(true);
        }
        private void ApplyUpgrade(int choice)
        {
            if (!_choosing || _ended) return;
            switch (choice)
            {
                case 0: _damageMultiplier *= 1.2f; break;
                case 1: player.GetComponent<MeleeWeapon>().MultiplyAttackSpeed(1.16f); break;
                case 2: player.GetComponent<MeleeWeapon>().MultiplyRange(1.18f); break;
                case 3: player.Speed = Mathf.Min(player.Speed * 1.1f, balance.playerSpeed * 2); break;
                case 4: _pickupRadius *= 1.3f; break;
                case 5: player.IncreaseMaxHealth(25f); break;
            }
            if(choice>=6)arsenal.Grant(choice-6);else _upgradeRanks[choice]++;
            _rewardsTaken++; _level = _rewardsTaken + 1;
            int unlocked = 0; if (arsenal) foreach (int rank in arsenal.ranks) if (rank > 0) unlocked++;
            _rewardNeeded = balance.RewardCost(_rewardsTaken, unlocked);
            _nextRewardAt = _time + balance.minimumRewardInterval;
            levelUpPanel.SetActive(false); Time.timeScale = 1f; _choosing = false;
            UpdateHud();
        }
        public void GameOver()
        {
            if (_ended) return; _ended = true; Time.timeScale = 0f;
            RecordRun();
            GameProfile.BestScore = _score; GameProfile.Coins += _score / 8;
            if (_time >= 180f && _map + 1 >= GameProfile.HighestStage) GameProfile.HighestStage = Mathf.Min(GameCatalog.MapNames.Length, _map + 2);
            gameOverText.text = $"AV SONA ERDİ\n\nSKOR  {_score:N0}\nSÜRE  {FormatTime(_time)}\nREKOR  {GameProfile.BestScore:N0}\n\n+{_score / 8} ALTIN"; gameOverPanel.SetActive(true);
        }
        private void TogglePause() { if (_ended || _choosing) return; bool open = !pausePanel.activeSelf; pausePanel.SetActive(open); Time.timeScale = open ? 0f : 1f; }
        private void Resume() { exitConfirmPanel.SetActive(false); pausePanel.SetActive(false); Time.timeScale = 1f; }
        private void ShowExitConfirmation() => exitConfirmPanel.SetActive(true);
        private void HideExitConfirmation() => exitConfirmPanel.SetActive(false);
        private void Retry() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        private void ReturnToMenu() { RecordRun();Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
        private void RecordRun(){if(_recorded)return;_recorded=true;LocalPlayerStore.RecordRun(_score,_wave);GameProfile.BestScore=_score;GameProfile.Gems+=_crystals/5;}
        private void FollowCamera() { Vector3 p = player.transform.position; gameplayCamera.transform.position = Vector3.Lerp(gameplayCamera.transform.position, new Vector3(p.x, p.y, -10f), Time.deltaTime * 7f); }
        public void PlayerDamaged() { if (feedback != null) feedback.Damage(player.HealthNormalized); }
        public void ShowImpact(Vector3 position, float damage) { if (feedback != null) feedback.Impact(position, damage, gameplayCamera); }
        private void UpdateHud() { timerText.text = FormatTime(_time); waveText.text = $"DALGA {_wave}"; scoreText.text = $"SKOR {_score:N0}"; levelText.text = $"SV. {_level}"; if (feedback != null) feedback.SetHealth(player.HealthNormalized); else healthBar.value = player.HealthNormalized; xpBar.value = RewardNormalized; }
        private static string FormatTime(float value) => $"{Mathf.FloorToInt(value / 60f):00}:{Mathf.FloorToInt(value % 60f):00}";
        private void OnApplicationPause(bool paused) {if(paused&&!_ended&&!_choosing&&pausePanel){pausePanel.SetActive(true);Time.timeScale=0;}}
        private static float UpgradeMultiplier(int index) => 1f + (PlayerPrefs.GetInt("EH2_Upgrade_" + index, 1) - 1) * .05f;
    }
}
