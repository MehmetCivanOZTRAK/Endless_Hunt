using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EndlessHunt
{
    public sealed class SurvivalGame : MonoBehaviour
    {
        [Header("Scene References")]
        public Camera gameplayCamera;
        public Transform worldRoot;
        public Transform[] mapChunks;
        public SpriteRenderer[] mapChunkRenderers;
        public SurvivalPlayer player;
        public Joystick movementJoystick;
        public WeaponDefinition[] weapons;

        [Header("HUD")]
        public Text timerText, waveText, scoreText, levelText;
        public Slider healthBar, xpBar;
        public Button pauseButton;

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
        public int forcedMapIndex = -1;

        public readonly List<SurvivalEnemy> Enemies = new();
        private float _time, _spawnClock, _nextWave = 30f;
        private float _damageMultiplier = 1f, _moveSpeed = 4.4f, _pickupRadius = 1.4f;
        private int _wave = 1, _score, _kills, _level = 1, _xpValue, _xpNeeded = 8, _map;
        private bool _ended, _choosing;
        private readonly int[] _shownChoices = new int[3];
        private WeaponDefinition _equippedWeapon;

        public SurvivalPlayer Player => player;
        public float MeleeDamageMultiplier { get => _damageMultiplier; set => _damageMultiplier = value; }
        public float Damage => (_equippedWeapon != null ? _equippedWeapon.DamageAtLevel(GameProfile.WeaponLevel(_equippedWeapon.id)) : 18f) * _damageMultiplier * UpgradeMultiplier(1);
        public float PickupRadius => _pickupRadius * UpgradeMultiplier(3);

        private void Start()
        {
            Time.timeScale = 1f;
            _map = forcedMapIndex >= 0 ? forcedMapIndex : GameProfile.SelectedStage;
            _equippedWeapon = CharacterSkin.FindWeapon(weapons, GameProfile.EquippedWeaponId);
            Enemies.Clear();
            foreach (SurvivalEnemy enemy in enemyPool) enemy.Bind(this);
            foreach (ExperienceGem gem in experiencePool) gem.Bind(this);
            player.Setup(this, movementJoystick, 100f * UpgradeMultiplier(0), _moveSpeed * UpgradeMultiplier(2), weapons);
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
            ApplyMapPalette(); UpdateHud();
        }

        private void Update()
        {
            if (_ended || _choosing || Time.timeScale == 0f) return;
            _time += Time.deltaTime; _spawnClock -= Time.deltaTime;
            if (_spawnClock <= 0f) { SpawnFormation(); _spawnClock = Mathf.Max(.28f, 1.35f - _wave * .08f); }
            if (_time >= _nextWave) { _wave++; _nextWave += 30f; SpawnBurst(_wave % 5 == 0 ? 16 : 8, _wave % 5 == 0); }
            FollowCamera(); RecycleTiles(); UpdateHud();
        }

        private void ApplyMapPalette()
        {
            Color ground = GameCatalog.MapGround[Mathf.Clamp(_map, 0, GameCatalog.MapGround.Length - 1)];
            gameplayCamera.backgroundColor = ground;
            for (int i = 0; i < mapChunkRenderers.Length; i++) mapChunkRenderers[i].color = Color.Lerp(ground, Color.white, (i & 3) * .025f);
        }
        private void SpawnFormation() { int count = (_wave + _kills / 15) % 4 == 2 ? 3 : 1; for (int i = 0; i < count; i++) SpawnEnemy(SpawnPoint(i, count), false); }
        private Vector2 SpawnPoint(int index, int count)
        {
            float angle = UnityEngine.Random.value * Mathf.PI * 2f + index * Mathf.PI * 2f / count;
            return (Vector2)player.transform.position + new Vector2(Mathf.Cos(angle) * (gameplayCamera.orthographicSize * gameplayCamera.aspect + 2.5f), Mathf.Sin(angle) * (gameplayCamera.orthographicSize + 2.5f));
        }
        private void SpawnBurst(int count, bool elite) { for (int i = 0; i < count; i++) SpawnEnemy(SpawnPoint(i, count), elite && i == 0); }
        private void SpawnEnemy(Vector2 position, bool elite)
        {
            SurvivalEnemy enemy = Array.Find(enemyPool, item => !item.gameObject.activeSelf); if (enemy == null) return;
            Sprite sprite = enemySprites != null && enemySprites.Length > 0 ? enemySprites[UnityEngine.Random.Range(0, enemySprites.Length)] : null;
            enemy.Activate(position, sprite, (22 + _wave * 7) * (elite ? 7 : 1), 1.65f + _wave * .035f, elite ? 4 : 1, elite); Enemies.Add(enemy);
        }
        public void EnemyKilled(SurvivalEnemy enemy, int xp) { Enemies.Remove(enemy); _kills++; _score += 10 * xp; DropExperience(enemy.transform.position, xp); }
        private void DropExperience(Vector3 position, int amount) { ExperienceGem gem = Array.Find(experiencePool, item => !item.gameObject.activeSelf); if (gem != null) gem.Activate(position, amount); }
        public void AddExperience(int amount)
        {
            _xpValue += amount; if (_xpValue < _xpNeeded) return;
            _xpValue -= _xpNeeded; _level++; _xpNeeded = Mathf.RoundToInt(_xpNeeded * 1.28f + 3); ShowUpgradeChoices();
        }
        private void ShowUpgradeChoices()
        {
            _choosing = true; Time.timeScale = 0f;
            string[] choices = { "KESKİN DARBE\nHasar +%20", "HIZLI SALDIRI\nSaldırı hızı +%16", "GENİŞ MENZİL\nYakın dövüş +%18", "SÜRAT\nHareket +%12", "MANYETİK ALAN\nToplama +%30", "İLK YARDIM\nCan +25" };
            int seed = (_level * 3) % choices.Length;
            for (int i = 0; i < 3; i++) { _shownChoices[i] = (seed + i * 2) % choices.Length; choiceTexts[i].text = choices[_shownChoices[i]]; }
            levelUpTitle.text = "SEVİYE ATLADIN!\nBİR GELİŞTİRME SEÇ"; levelUpPanel.SetActive(true);
        }
        private void ApplyUpgrade(int choice)
        {
            switch (choice)
            {
                case 0: _damageMultiplier *= 1.2f; break;
                case 1: player.GetComponent<MeleeWeapon>().MultiplyAttackSpeed(1.16f); break;
                case 2: _damageMultiplier *= 1.08f; player.Speed *= 1.05f; break;
                case 3: player.Speed *= 1.12f; break;
                case 4: _pickupRadius *= 1.3f; break;
                case 5: player.Heal(25f); break;
            }
            levelUpPanel.SetActive(false); Time.timeScale = 1f; _choosing = false;
        }
        public void GameOver()
        {
            if (_ended) return; _ended = true; Time.timeScale = 0f;
            GameProfile.BestScore = _score; GameProfile.Coins += _score / 8;
            if (_time >= 180f && _map + 1 >= GameProfile.HighestStage) GameProfile.HighestStage = Mathf.Min(GameCatalog.MapNames.Length, _map + 2);
            gameOverText.text = $"AV SONA ERDİ\n\nSKOR  {_score:N0}\nSÜRE  {FormatTime(_time)}\nREKOR  {GameProfile.BestScore:N0}\n\n+{_score / 8} ALTIN"; gameOverPanel.SetActive(true);
        }
        private void TogglePause() { if (_ended || _choosing) return; bool open = !pausePanel.activeSelf; pausePanel.SetActive(open); Time.timeScale = open ? 0f : 1f; }
        private void Resume() { exitConfirmPanel.SetActive(false); pausePanel.SetActive(false); Time.timeScale = 1f; }
        private void ShowExitConfirmation() => exitConfirmPanel.SetActive(true);
        private void HideExitConfirmation() => exitConfirmPanel.SetActive(false);
        private void Retry() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        private void ReturnToMenu() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
        private void FollowCamera() { Vector3 p = player.transform.position; gameplayCamera.transform.position = Vector3.Lerp(gameplayCamera.transform.position, new Vector3(p.x, p.y, -10f), Time.deltaTime * 7f); }
        private void RecycleTiles()
        {
            Vector2 center = new(Mathf.Round(player.transform.position.x / 12f) * 12f, Mathf.Round(player.transform.position.y / 12f) * 12f);
            foreach (Transform tile in mapChunks)
            {
                Vector2 p = tile.position; if (p.x < center.x - 24f) p.x += 60f; else if (p.x > center.x + 24f) p.x -= 60f;
                if (p.y < center.y - 36f) p.y += 84f; else if (p.y > center.y + 36f) p.y -= 84f; tile.position = new Vector3(p.x, p.y, 2f);
            }
        }
        private void UpdateHud() { timerText.text = FormatTime(_time); waveText.text = $"DALGA {_wave}"; scoreText.text = $"SKOR {_score:N0}"; levelText.text = $"SV. {_level}"; healthBar.value = player.HealthNormalized; xpBar.value = Mathf.Clamp01((float)_xpValue / _xpNeeded); }
        private static string FormatTime(float value) => $"{Mathf.FloorToInt(value / 60f):00}:{Mathf.FloorToInt(value % 60f):00}";
        private static float UpgradeMultiplier(int index) => 1f + (PlayerPrefs.GetInt("EH2_Upgrade_" + index, 1) - 1) * .05f;
    }
}
