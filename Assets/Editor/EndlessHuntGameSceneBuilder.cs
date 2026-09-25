using EndlessHunt;
using EndlessHunt.UI;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

internal static class EndlessHuntGameSceneBuilder
{
    internal static void Build(GameObject startPrefab, WeaponDefinition[] weapons, Sprite[] enemySprites, int forcedMap)
    {
        Camera camera = EndlessHuntSceneFactory.Camera("Main Camera", new Color(.16f, .33f, .24f), 9.6f);
        GameObject root = new("Endless Hunt Survival (Scene References)");
        SurvivalGame game = root.AddComponent<SurvivalGame>();
        game.gameplayCamera = camera;
        game.forcedMapIndex = forcedMap;
        game.enemySprites = enemySprites;
        game.weapons = weapons;

        BuildWorld(game);
        BuildPlayer(game, startPrefab);
        BuildPools(game, enemySprites);
        BuildHud(game);
        EndlessHuntSceneFactory.EventSystem();
    }

    private static void BuildWorld(SurvivalGame game)
    {
        GameObject world = new("Infinite LayerLab World (35 Prepared Chunks)");
        game.worldRoot = world.transform;
        const string mapRoot = "Assets/Layer Lab/2D Art Maker/AMCasual Character/Demo/Map/";
        Sprite mapSprite = EndlessHuntSceneFactory.Sprite(mapRoot + "background.png");
        string[] decorationNames = { "grass1.png", "grass2.png", "grass3.png", "flower1.png", "flower2.png", "tree1.png", "tree2.png", "tree3.png", "object1.png", "object2.png", "object4.png", "farm1.png", "campfire.png", "tent.png" };
        Sprite[] decorations = new Sprite[decorationNames.Length];
        for (int i = 0; i < decorations.Length; i++) decorations[i] = EndlessHuntSceneFactory.Sprite(mapRoot + decorationNames[i]);
        game.mapChunks = new Transform[35];
        game.mapChunkRenderers = new SpriteRenderer[35];
        int index = 0;
        for (int y = -3; y <= 3; y++)
        {
            for (int x = -2; x <= 2; x++)
            {
                GameObject chunk = new($"Map Chunk {x:+0;-0;0}, {y:+0;-0;0} (Persistent)");
                chunk.transform.SetParent(world.transform);
                chunk.transform.position = new Vector3(x * 12f, y * 12f, 2f);
                GameObject ground = new("LayerLab Background", typeof(SpriteRenderer));
                ground.transform.SetParent(chunk.transform, false);
                SpriteRenderer renderer = ground.GetComponent<SpriteRenderer>();
                renderer.sprite = mapSprite;
                renderer.sortingOrder = -20;
                if (mapSprite != null)
                {
                    Vector2 size = mapSprite.bounds.size;
                    ground.transform.localScale = new Vector3(12.15f / Mathf.Max(.01f, size.x),
                        12.15f / Mathf.Max(.01f, size.y), 1f);
                }
                GameObject decoRoot = new("LayerLab Decorations (Prepared)");
                decoRoot.transform.SetParent(chunk.transform, false);
                for (int d = 0; d < 5; d++)
                {
                    int spriteIndex = Mathf.Abs(index * 7 + d * 3) % decorations.Length;
                    GameObject deco = new($"Decoration {d + 1:00} - {decorationNames[spriteIndex]}", typeof(SpriteRenderer));
                    deco.transform.SetParent(decoRoot.transform, false);
                    deco.transform.localPosition = new Vector3(-4.5f + ((index * 31 + d * 17) % 90) / 10f, -4.5f + ((index * 13 + d * 29) % 90) / 10f, -.2f);
                    deco.transform.localScale = Vector3.one * (.55f + ((index + d) % 4) * .12f);
                    SpriteRenderer decoRenderer = deco.GetComponent<SpriteRenderer>();
                    decoRenderer.sprite = decorations[spriteIndex];
                    decoRenderer.sortingOrder = -5 + d;
                }
                game.mapChunks[index] = chunk.transform;
                game.mapChunkRenderers[index] = renderer;
                index++;
            }
        }
    }

    private static void BuildPlayer(SurvivalGame game, GameObject startPrefab)
    {
        GameObject playerObject = new("LayerLab Player (Scene Object)", typeof(Rigidbody2D),
            typeof(CircleCollider2D), typeof(SurvivalPlayer), typeof(MeleeWeapon));
        playerObject.tag = "Player";
        Rigidbody2D body = playerObject.GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CircleCollider2D collider = playerObject.GetComponent<CircleCollider2D>();
        collider.radius = .46f;

        SkeletonAnimation view = EndlessHuntSceneFactory.PrefabCharacter(
            startPrefab, "Character_20260827_211359 (Starting Look)", playerObject.transform, new Vector3(0f, -.55f, 0f), 1f);
        SurvivalPlayer player = playerObject.GetComponent<SurvivalPlayer>();
        player.body = body;
        player.characterView = view;
        game.player = player;
    }

    private static void BuildPools(SurvivalGame game, Sprite[] enemySprites)
    {
        Sprite gemSprite = EndlessHuntSceneFactory.Ui("Icons/128px/gem_icon_128px.png");

        GameObject enemyRoot = new("Enemy Pool (120 Prepared Objects)");
        enemyRoot.transform.SetParent(game.worldRoot);
        game.enemyPool = new SurvivalEnemy[120];
        for (int i = 0; i < game.enemyPool.Length; i++)
        {
            GameObject go = new($"Enemy Pool Item {i + 1:000}", typeof(SpriteRenderer),
                typeof(CircleCollider2D), typeof(SurvivalEnemy));
            go.transform.SetParent(enemyRoot.transform);
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = enemySprites != null && enemySprites.Length > 0 ? enemySprites[i % enemySprites.Length] : null;
            renderer.sortingOrder = 5;
            CircleCollider2D collider = go.GetComponent<CircleCollider2D>();
            collider.radius = .38f;
            collider.isTrigger = true;
            SurvivalEnemy enemy = go.GetComponent<SurvivalEnemy>();
            enemy.spriteRenderer = renderer;
            game.enemyPool[i] = enemy;
            go.SetActive(false);
        }

        GameObject experienceRoot = new("Experience Pool (100 Prepared Objects)");
        experienceRoot.transform.SetParent(game.worldRoot);
        game.experiencePool = new ExperienceGem[100];
        for (int i = 0; i < game.experiencePool.Length; i++)
        {
            GameObject go = new($"Experience Pool Item {i + 1:000}", typeof(SpriteRenderer),
                typeof(CircleCollider2D), typeof(ExperienceGem));
            go.transform.SetParent(experienceRoot.transform);
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = gemSprite;
            renderer.sortingOrder = 4;
            CircleCollider2D collider = go.GetComponent<CircleCollider2D>();
            collider.radius = .32f;
            collider.isTrigger = true;
            ExperienceGem gem = go.GetComponent<ExperienceGem>();
            gem.spriteRenderer = renderer;
            game.experiencePool[i] = gem;
            go.SetActive(false);
        }
    }

    internal static void BuildHud(SurvivalGame game)
    {
        Canvas canvas = EndlessHuntSceneFactory.Canvas("Battle Canvas");
        RectTransform safe = EndlessHuntSceneFactory.Rect("Safe Area", canvas.transform, Vector2.zero, Vector2.one,
            new Vector2(22f, 28f), new Vector2(-22f, -28f));
        safe.gameObject.AddComponent<SafeAreaController>();
        Image header = EndlessHuntSceneFactory.Image("Battle Header", safe,
            EndlessHuntSceneFactory.Ui("Panels/panel_black.png"), new Vector2(0f, 1f), Vector2.one,
            new Vector2(0f, -205f), Vector2.zero, Color.white);
        game.timerText = EndlessHuntSceneFactory.Text("Timer", header.transform, "00:00", 42, Color.white,
            TextAnchor.UpperCenter, new Vector2(.33f, .48f), new Vector2(.67f, .96f), Vector2.zero, Vector2.zero,
            FontStyle.Bold);
        game.waveText = EndlessHuntSceneFactory.Text("Wave", header.transform, "DALGA 1", 25,
            new Color(.25f, .9f, 1f), TextAnchor.UpperLeft, new Vector2(.03f, .5f), new Vector2(.34f, .94f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        game.scoreText = EndlessHuntSceneFactory.Text("Score", header.transform, "SKOR 0", 25,
            new Color(1f, .8f, .24f), TextAnchor.UpperRight, new Vector2(.66f, .5f), new Vector2(.97f, .94f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        game.healthBar = EndlessHuntSceneFactory.Bar("Health Bar", header.transform,
            EndlessHuntSceneFactory.Ui("ProgressBars/progressBarBase_black.png"),
            EndlessHuntSceneFactory.Ui("ProgressBars/progressBar_red.png"), Color.white,
            new Vector2(.035f, .13f), new Vector2(.47f, .38f), Vector2.zero, Vector2.zero);
        game.xpBar = EndlessHuntSceneFactory.Bar("Experience Bar", header.transform,
            EndlessHuntSceneFactory.Ui("ProgressBars/progressBarBase_black.png"),
            EndlessHuntSceneFactory.Ui("ProgressBars/progressBar_cyan.png"), Color.white,
            new Vector2(.53f, .13f), new Vector2(.965f, .38f), Vector2.zero, Vector2.zero);
        game.levelText = EndlessHuntSceneFactory.Text("Level", header.transform, "SV. 1", 20, Color.white,
            TextAnchor.LowerCenter, new Vector2(.4f, 0f), new Vector2(.6f, .28f), Vector2.zero, Vector2.zero,
            FontStyle.Bold);

        game.pauseButton = EndlessHuntSceneFactory.Button("Pause", safe,
            EndlessHuntSceneFactory.Ui("Buttons/buttonSquare_blue.png"), "Ⅱ", 32,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-150f, -350f), new Vector2(-25f, -225f));

        GameObject joystickPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Joystick Pack/Prefabs/Fixed Joystick.prefab");
        GameObject joystickObject = (GameObject)PrefabUtility.InstantiatePrefab(joystickPrefab, safe.transform);
        joystickObject.name = "Fixed Joystick (Joystick Pack)";
        RectTransform joystickRect = joystickObject.GetComponent<RectTransform>();
        joystickRect.anchorMin = Vector2.zero;
        joystickRect.anchorMax = Vector2.zero;
        joystickRect.pivot = Vector2.zero;
        joystickRect.anchoredPosition = new Vector2(55f, 70f);
        joystickRect.sizeDelta = new Vector2(300f, 300f);
        game.movementJoystick = joystickObject.GetComponent<Joystick>();

        BuildFeedback(game, canvas, safe);
        BuildLevelUpPanel(game, safe);
        BuildPausePanel(game, safe);
        BuildExitConfirmation(game, safe);
        BuildGameOverPanel(game, safe);
    }

    private static void BuildFeedback(SurvivalGame game, Canvas canvas, RectTransform safe)
    {
        BattleFeedback feedback = canvas.gameObject.AddComponent<BattleFeedback>();
        game.feedback = feedback; feedback.health = game.healthBar;
        GameObject trail = Object.Instantiate(game.healthBar.gameObject, game.healthBar.transform.parent);
        trail.name = "Health Damage Trail";
        trail.transform.SetSiblingIndex(game.healthBar.transform.GetSiblingIndex());
        feedback.delayedHealth = trail.GetComponent<Slider>();
        feedback.delayedHealth.fillRect.GetComponent<Image>().color = new Color(1f,.72f,.25f);
        game.healthBar.GetComponent<Image>().color = Color.clear;
        feedback.damageOverlay = EndlessHuntSceneFactory.Image("Damage Screen Tint",canvas.transform,null,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,new Color(.8f,.02f,.03f,0f));
        feedback.damageOverlay.transform.SetAsFirstSibling();
        feedback.healthLabel = EndlessHuntSceneFactory.Text("Health Percentage",game.healthBar.transform,"100%",20,Color.white,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,FontStyle.Bold);
        RectTransform layer = EndlessHuntSceneFactory.Rect("Damage Number Pool",safe,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        feedback.damageLabels = new Text[20];
        for(int i=0;i<20;i++)
        {
            Text number = EndlessHuntSceneFactory.Text("Damage "+i,layer,"",32,Color.clear,TextAnchor.MiddleCenter,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-65f,-30f),new Vector2(65f,30f),FontStyle.Bold);
            var outline = number.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(.08f,.06f,.05f); outline.effectDistance = new Vector2(2f,-2f);
            feedback.damageLabels[i] = number;
        }
    }

    private static void BuildLevelUpPanel(SurvivalGame game, Transform parent)
    {
        Image panel = EndlessHuntSceneFactory.Image("Level Up Panel (Prepared)", parent,
            null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(.035f,.045f,.07f,.88f), true);
        panel.gameObject.AddComponent<PopupMotion>();
        game.levelUpPanel = panel.gameObject;
        Image ribbon = EndlessHuntSceneFactory.Image("Choice Ribbon",panel.transform,EndlessHuntSceneFactory.Ui("Headers/header_yellow.png"),new Vector2(.05f,.73f),new Vector2(.95f,.86f),Vector2.zero,Vector2.zero,Color.white);
        game.levelUpTitle = EndlessHuntSceneFactory.Text("Title", ribbon.transform, "YETENEK SEÇ", 46,
            Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(20f,10f),new Vector2(-20f,-10f),FontStyle.Bold);
        game.choiceButtons = new Button[3]; game.choiceTexts = new Text[3]; game.choiceIcons = new Image[3]; game.choiceRanks = new Text[3];
        string[] icons = { "sword", "energy", "adventure", "energy", "gem", "armor" };
        game.upgradeIcons = new Sprite[6];
        for(int i=0;i<6;i++) game.upgradeIcons[i] = EndlessHuntSceneFactory.Ui("Icons/128px/"+icons[i]+"_icon_128px.png");
        for (int i = 0; i < 3; i++)
        {
            float left = .025f + i * .325f;
            Button button = EndlessHuntSceneFactory.Button($"Upgrade Choice {i + 1}", panel.transform,
                EndlessHuntSceneFactory.Ui("Panels/panel_black.png"), "", 28,
                new Vector2(left, .24f), new Vector2(left + .30f, .68f), Vector2.zero, Vector2.zero);
            Image cap = EndlessHuntSceneFactory.Image("Card Header",button.transform,EndlessHuntSceneFactory.Ui("Headers/header_yellow.png"),new Vector2(0f,.85f),Vector2.one,Vector2.zero,Vector2.zero,Color.white);
            game.choiceRanks[i] = EndlessHuntSceneFactory.Text("Rank",cap.transform,"YENİ",28,Color.white,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,FontStyle.Bold);
            game.choiceIcons[i] = EndlessHuntSceneFactory.Image("Skill Icon",button.transform,game.upgradeIcons[i],new Vector2(.15f,.57f),new Vector2(.85f,.81f),Vector2.zero,Vector2.zero,Color.white);
            game.choiceIcons[i].preserveAspect = true;
            game.choiceTexts[i] = EndlessHuntSceneFactory.Text("Choice Text",button.transform,"GELİŞTİRME",30,
                Color.white, TextAnchor.UpperCenter, new Vector2(.06f,.14f),new Vector2(.94f,.53f),new Vector2(6f,8f),new Vector2(-6f,-8f),FontStyle.Bold);
            EndlessHuntSceneFactory.Text("Choose",button.transform,"SEÇ",26,new Color(1f,.82f,.22f),TextAnchor.MiddleCenter,new Vector2(.05f,.025f),new Vector2(.95f,.14f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            game.choiceButtons[i] = button;
        }
        EndlessHuntSceneFactory.Text("Hint",panel.transform,"Güçlenmek için bir kart seç",30,Color.white,TextAnchor.MiddleCenter,new Vector2(.05f,.10f),new Vector2(.95f,.18f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        panel.gameObject.SetActive(false);
    }

    private static void BuildPausePanel(SurvivalGame game, Transform parent)
    {
        Image panel = EndlessHuntSceneFactory.Image("Pause Panel (Prepared)", parent,
            EndlessHuntSceneFactory.Ui("Modals/darkModal_dark.png"), Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, Color.white, true);
        game.pausePanel = panel.gameObject; panel.gameObject.AddComponent<PopupMotion>();
        EndlessHuntSceneFactory.Text("Title", panel.transform, "DURAKLATILDI", 52, Color.white,
            TextAnchor.MiddleCenter, new Vector2(.1f, .62f), new Vector2(.9f, .77f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        game.resumeButton = EndlessHuntSceneFactory.Button("Resume", panel.transform,
            EndlessHuntSceneFactory.Ui("Buttons/button_green.png"), "DEVAM ET", 32,
            new Vector2(.16f, .45f), new Vector2(.84f, .55f), Vector2.zero, Vector2.zero);
        game.pauseMenuButton = EndlessHuntSceneFactory.Button("Return To Menu", panel.transform,
            EndlessHuntSceneFactory.Ui("Buttons/button_blue.png"), "ANA MENÜ", 29,
            new Vector2(.16f, .32f), new Vector2(.84f, .42f), Vector2.zero, Vector2.zero);
        panel.gameObject.SetActive(false);
    }

    private static void BuildExitConfirmation(SurvivalGame game, Transform parent)
    {
        Image dim = EndlessHuntSceneFactory.Image("Exit Confirmation (Prepared)", parent,
            EndlessHuntSceneFactory.Ui("Modals/darkModalSimple_dark.png"), Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, Color.white, true);
        game.exitConfirmPanel = dim.gameObject; dim.gameObject.AddComponent<PopupMotion>();
        Image modal = EndlessHuntSceneFactory.Image("Vector UI Confirmation Card", dim.transform,
            EndlessHuntSceneFactory.Ui("Modals/modalSimple_v1.png"), new Vector2(.12f, .31f), new Vector2(.88f, .69f),
            Vector2.zero, Vector2.zero, Color.white, true);
        EndlessHuntSceneFactory.Image("Blue Ribbon", modal.transform,
            EndlessHuntSceneFactory.Ui("Headers/header_blue.png"), new Vector2(.04f, .75f), new Vector2(.96f, 1.04f),
            Vector2.zero, Vector2.zero, Color.white);
        EndlessHuntSceneFactory.Text("Title", modal.transform, "OYUNDAN ÇIK", 38, Color.white,
            TextAnchor.MiddleCenter, new Vector2(.08f, .77f), new Vector2(.92f, .99f), Vector2.zero, Vector2.zero, FontStyle.Bold);
        EndlessHuntSceneFactory.Text("Question", modal.transform, "Avı bırakıp ana menüye dönmek istediğine emin misin?", 27,
            new Color(.16f,.12f,.2f), TextAnchor.MiddleCenter, new Vector2(.09f,.35f), new Vector2(.91f,.72f), Vector2.zero, Vector2.zero, FontStyle.Bold);
        game.exitConfirmButton = EndlessHuntSceneFactory.Button("Leave", modal.transform,
            EndlessHuntSceneFactory.Ui("Buttons/button_red.png"), "ÇIK", 28, new Vector2(.08f,.08f), new Vector2(.47f,.31f), Vector2.zero, Vector2.zero);
        game.exitCancelButton = EndlessHuntSceneFactory.Button("Cancel", modal.transform,
            EndlessHuntSceneFactory.Ui("Buttons/button_blue.png"), "VAZGEÇ", 28, new Vector2(.53f,.08f), new Vector2(.92f,.31f), Vector2.zero, Vector2.zero);
        dim.gameObject.SetActive(false);
    }

    private static void BuildGameOverPanel(SurvivalGame game, Transform parent)
    {
        Image panel = EndlessHuntSceneFactory.Image("Game Over Panel (Prepared)", parent,
            EndlessHuntSceneFactory.Ui("Modals/modalCastle.png"), Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, Color.white, true);
        game.gameOverPanel = panel.gameObject; panel.gameObject.AddComponent<PopupMotion>();
        game.gameOverText = EndlessHuntSceneFactory.Text("Results", panel.transform, "AV SONA ERDİ", 42,
            Color.white, TextAnchor.UpperCenter, new Vector2(.08f, .4f), new Vector2(.92f, .8f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        game.retryButton = EndlessHuntSceneFactory.Button("Retry", panel.transform,
            EndlessHuntSceneFactory.Ui("Buttons/button_yellow.png"), "TEKRAR DENE", 31,
            new Vector2(.16f, .25f), new Vector2(.84f, .35f), Vector2.zero, Vector2.zero);
        game.gameOverMenuButton = EndlessHuntSceneFactory.Button("Return To Menu", panel.transform,
            EndlessHuntSceneFactory.Ui("Buttons/button_blue.png"), "ANA MENÜ", 29,
            new Vector2(.16f, .13f), new Vector2(.84f, .23f), Vector2.zero, Vector2.zero);
        panel.gameObject.SetActive(false);
    }
}
