using EndlessHunt;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

internal static class EndlessHuntMenuSceneBuilder
{
    private static readonly Color Orange = new(.96f,.49f,.12f,1f);
    private static readonly Color Dark = new(.075f,.085f,.13f,1f);
    private static readonly Color Ink = new(.12f,.09f,.16f,1f);

    internal static void Build(GameObject startPrefab, WeaponDefinition[] weapons)
    {
        EndlessHuntSceneFactory.Camera("Main Camera", Dark, 6.2f);
        GameObject systems = new("Main Menu Systems (Serialized References)");
        SurvivorMenu menu = systems.AddComponent<SurvivorMenu>();
        menu.weapons = weapons;

        Canvas canvas = EndlessHuntSceneFactory.Canvas("Survivor Style Main Menu Canvas");
        Image root = EndlessHuntSceneFactory.Image("Dark Application Background", canvas.transform,
            EndlessHuntSceneFactory.Ui("Panels/panel_black.png"), Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, Dark);
        RectTransform safe = EndlessHuntSceneFactory.Rect("Mobile Safe Area", root.transform,
            Vector2.zero, Vector2.one, new Vector2(0f,24f), new Vector2(0f,-22f));

        BuildPlayerBar(menu, safe);
        RectTransform pages = EndlessHuntSceneFactory.Rect("Screens (Rebuilt From Zero)", safe,
            new Vector2(0f,.105f), new Vector2(1f,.93f), Vector2.zero, Vector2.zero);
        menu.homePage = BuildBattle(menu, pages, startPrefab);
        menu.equipmentPage = BuildEquipment(menu, pages, startPrefab, weapons);
        menu.upgradePage = BuildUpgrade(menu, pages);
        menu.storePage = BuildStore(menu, pages, startPrefab, weapons);
        BuildNavigation(menu, safe);
        BuildConfirmation(menu, safe);
        BuildToast(menu, safe);
        EndlessHuntSceneFactory.EventSystem();
    }

    private static void BuildPlayerBar(SurvivorMenu menu, Transform parent)
    {
        Image bar = EndlessHuntSceneFactory.Image("Player And Resources Bar", parent,
            EndlessHuntSceneFactory.Ui("Modals/topBarEmpty.png"), new Vector2(0f,.93f), Vector2.one,
            Vector2.zero, Vector2.zero, new Color(.09f,.1f,.15f,1f));
        Image profile = EndlessHuntSceneFactory.Image("Player Profile", bar.transform,
            EndlessHuntSceneFactory.Ui("Cards/cardShop_yellow.png"), new Vector2(.005f,.06f), new Vector2(.35f,.95f),
            Vector2.zero, Vector2.zero, Color.white);
        Image avatar = EndlessHuntSceneFactory.Image("Avatar", profile.transform,
            EndlessHuntSceneFactory.Ui("Icons/128px/adventure_icon_128px.png"), new Vector2(.01f,.05f), new Vector2(.28f,.95f),
            new Vector2(4f,4f), new Vector2(-4f,-4f), Color.white);
        avatar.preserveAspect = true;
        menu.playerNameText = EndlessHuntSceneFactory.Text("Player Name", profile.transform,"Player 69252009",27,
            Color.white,TextAnchor.MiddleLeft,new Vector2(.29f,.42f),new Vector2(.98f,.94f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        menu.playerLevelText = EndlessHuntSceneFactory.Text("Player Level", profile.transform,"LV.20",21,
            new Color(1f,.9f,.25f),TextAnchor.MiddleLeft,new Vector2(.29f,.05f),new Vector2(.49f,.42f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        EndlessHuntSceneFactory.Bar("Player Experience",profile.transform,
            EndlessHuntSceneFactory.Ui("ProgressBars/progressBarBase_black.png"),
            EndlessHuntSceneFactory.Ui("ProgressBars/progressBar_green.png"),Color.white,
            new Vector2(.48f,.12f),new Vector2(.96f,.34f),Vector2.zero,Vector2.zero).value=.62f;

        ResourceCell(bar.transform,"Energy","Icons/128px/energy_icon_128px.png","20/20",new Vector2(.355f,.12f),new Vector2(.565f,.9f),out menu.energyText);
        ResourceCell(bar.transform,"Gems","Icons/128px/gem_icon_128px.png","120",new Vector2(.57f,.12f),new Vector2(.775f,.9f),out menu.gemText);
        ResourceCell(bar.transform,"Coins","Icons/128px/coin_icon_128px.png","800",new Vector2(.78f,.12f),new Vector2(.995f,.9f),out menu.coinText);
        menu.energyTimerText = EndlessHuntSceneFactory.Text("Energy Timer",bar.transform,"DOLU",14,
            new Color(1f,.9f,.25f),TextAnchor.LowerCenter,new Vector2(.39f,0f),new Vector2(.55f,.28f),Vector2.zero,Vector2.zero,FontStyle.Bold);
    }

    private static void ResourceCell(Transform parent,string name,string iconPath,string defaultValue,Vector2 min,Vector2 max,out Text value)
    {
        Image cell = EndlessHuntSceneFactory.Image(name,parent,
            EndlessHuntSceneFactory.Ui("Labels/label_backgroundV2.png"),min,max,Vector2.zero,Vector2.zero,Color.white);
        Image icon = EndlessHuntSceneFactory.Image("Icon",cell.transform,EndlessHuntSceneFactory.Ui(iconPath),
            new Vector2(0f,.02f),new Vector2(.36f,.98f),new Vector2(2f,0f),Vector2.zero,Color.white);
        icon.preserveAspect=true;
        value = EndlessHuntSceneFactory.Text("Value",cell.transform,defaultValue,25,Color.white,TextAnchor.MiddleCenter,
            new Vector2(.3f,0f),new Vector2(.84f,1f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        Image plus = EndlessHuntSceneFactory.Image("Plus",cell.transform,
            EndlessHuntSceneFactory.Ui("Icons/128px/plusV1_icon_128px.png"),new Vector2(.79f,.18f),new Vector2(1f,.82f),Vector2.zero,Vector2.zero,Color.white);
        plus.preserveAspect=true;
    }

    private static GameObject BuildBattle(SurvivorMenu menu, Transform parent, GameObject startPrefab)
    {
        Image page = Page("Battle Screen",parent,Orange);
        IconButton("Settings",page.transform,"Icons/128px/settings_icon_128px.png",new Vector2(.02f,.87f),new Vector2(.12f,.98f));
        IconButton("Mail",page.transform,"Icons/128px/mail_icon_128px.png",new Vector2(.88f,.87f),new Vector2(.98f,.98f));
        menu.stageTitle = EndlessHuntSceneFactory.Text("Stage Name",page.transform,"1. YEŞİL KAMP",48,Color.white,
            TextAnchor.MiddleCenter,new Vector2(.08f,.79f),new Vector2(.92f,.9f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        menu.bestScoreText = EndlessHuntSceneFactory.Text("Longest Survived",page.transform,"En Uzun Hayatta Kalma: 00m 00s",23,
            Color.white,TextAnchor.MiddleCenter,new Vector2(.1f,.755f),new Vector2(.9f,.81f),Vector2.zero,Vector2.zero,FontStyle.Bold);

        Image stage = EndlessHuntSceneFactory.Image("Stage Preview",page.transform,
            EndlessHuntSceneFactory.Ui("Containers/cardContainer_black.png"),new Vector2(.19f,.39f),new Vector2(.81f,.75f),
            Vector2.zero,Vector2.zero,Color.white);
        Image map = EndlessHuntSceneFactory.Image("LayerLab Stage Art",stage.transform,
            EndlessHuntSceneFactory.Sprite("Assets/Layer Lab/2D Art Maker/AMCasual Character/Demo/Map/background.png"),
            new Vector2(.03f,.03f),new Vector2(.97f,.97f),Vector2.zero,Vector2.zero,new Color(.65f,.8f,.55f,1f));
        map.type=Image.Type.Simple;
        menu.battlePreview = EndlessHuntSceneFactory.SpineGraphic(startPrefab,"Battle Character Preview",stage.transform,
            new Vector2(.27f,.04f),new Vector2(.73f,.74f),Vector2.zero,Vector2.zero,2.15f);
        EndlessHuntSceneFactory.Text("Stage Number",stage.transform,(GameProfile.SelectedStage+1).ToString(),34,
            new Color(.16f,.12f,.2f),TextAnchor.MiddleCenter,new Vector2(.43f,.81f),new Vector2(.57f,.96f),Vector2.zero,Vector2.zero,FontStyle.Bold);

        Image chest = EndlessHuntSceneFactory.Image("Chapter Chest",page.transform,
            EndlessHuntSceneFactory.Ui("Icons/256px/chestRuby_icon_256px.png"),new Vector2(.42f,.28f),new Vector2(.58f,.39f),Vector2.zero,Vector2.zero,Color.white);
        chest.preserveAspect=true;
        EndlessHuntSceneFactory.Text("Chest Label",page.transform,"Bölüm Sandığı",20,Color.white,TextAnchor.MiddleCenter,
            new Vector2(.34f,.245f),new Vector2(.66f,.29f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        menu.playButton = EndlessHuntSceneFactory.Button("Start Battle",page.transform,
            EndlessHuntSceneFactory.Ui("Buttons/buttonAdvanced_yellow.png"),"BAŞLA\n⚡ 5",32,
            new Vector2(.31f,.105f),new Vector2(.69f,.225f),Vector2.zero,Vector2.zero);
        Button patrol = IconButton("Patrol",page.transform,"Icons/128px/adventure_icon_128px.png",new Vector2(.03f,.08f),new Vector2(.22f,.22f));
        EndlessHuntSceneFactory.Text("Label",patrol.transform,"DEVRİYE",19,Color.white,TextAnchor.LowerCenter,Vector2.zero,Vector2.one,Vector2.zero,new Vector2(0f,-4f),FontStyle.Bold);
        Button daily = IconButton("Daily Event",page.transform,"Icons/128px/store_icon_128px.png",new Vector2(.78f,.08f),new Vector2(.97f,.22f));
        EndlessHuntSceneFactory.Text("Label",daily.transform,"GÜNLÜK ETKİNLİK",18,Color.white,TextAnchor.LowerCenter,Vector2.zero,Vector2.one,Vector2.zero,new Vector2(0f,-4f),FontStyle.Bold);
        menu.previousStageButton = EndlessHuntSceneFactory.Button("Previous Stage",page.transform,
            EndlessHuntSceneFactory.Ui("Buttons/buttonSquare_blue.png"),"‹",42,new Vector2(.08f,.51f),new Vector2(.18f,.62f),Vector2.zero,Vector2.zero);
        menu.nextStageButton = EndlessHuntSceneFactory.Button("Next Stage",page.transform,
            EndlessHuntSceneFactory.Ui("Buttons/buttonSquare_blue.png"),"›",42,new Vector2(.82f,.51f),new Vector2(.92f,.62f),Vector2.zero,Vector2.zero);
        return page.gameObject;
    }

    private static GameObject BuildEquipment(SurvivorMenu menu, Transform parent, GameObject startPrefab, WeaponDefinition[] weapons)
    {
        Image page = Page("Equipment Screen",parent,Orange);
        Image upper = EndlessHuntSceneFactory.Image("Equipped Hero Area",page.transform,
            EndlessHuntSceneFactory.Ui("Panels/panel_yellow.png"),new Vector2(0f,.43f),Vector2.one,
            Vector2.zero,Vector2.zero,new Color(1f,.66f,.2f,1f));
        EndlessHuntSceneFactory.Image("Halftone Overlay",upper.transform,
            EndlessHuntSceneFactory.Ui("Effects/effect_yellow.png"),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,new Color(1f,1f,1f,.2f));
        menu.equipmentPreview = EndlessHuntSceneFactory.SpineGraphic(startPrefab,"Equipment Live Character",upper.transform,
            new Vector2(.29f,.13f),new Vector2(.71f,.76f),Vector2.zero,Vector2.zero,2.05f);

        menu.equippedSlotIcons = new Image[6];
        menu.equippedSlotLevels = new Text[6];
        menu.equippedSlotButtons = new Button[6];
        Vector2[] mins = { new(.035f,.69f),new(.765f,.69f),new(.035f,.42f),new(.765f,.42f),new(.035f,.15f),new(.765f,.15f) };
        Vector2[] maxs = { new(.245f,.94f),new(.975f,.94f),new(.245f,.67f),new(.975f,.67f),new(.245f,.4f),new(.975f,.4f) };
        for(int i=0;i<6;i++)
        {
            Sprite initial = i==0 ? weapons[2].icon : GearSlotIcon(i);
            EquippedSlot(upper.transform,i,initial,mins[i],maxs[i],i%2==0,
                out menu.equippedSlotButtons[i],out menu.equippedSlotIcons[i],out menu.equippedSlotLevels[i]);
        }
        Image tech = EndlessHuntSceneFactory.Image("Tech Parts",upper.transform,
            EndlessHuntSceneFactory.Ui("Icons/128px/skills_icon_128px.png"),new Vector2(.44f,.86f),new Vector2(.56f,.98f),Vector2.zero,Vector2.zero,Color.white);
        tech.preserveAspect=true;
        EndlessHuntSceneFactory.Text("Tech Label",upper.transform,"TEKNOLOJİ",18,Color.white,TextAnchor.MiddleCenter,
            new Vector2(.38f,.81f),new Vector2(.62f,.87f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        menu.attackText = StatLabel(upper.transform,"Attack","Icons/128px/sword_icon_128px.png","ATK  19",new Vector2(.02f,.02f),new Vector2(.39f,.14f));
        menu.healthText = StatLabel(upper.transform,"Health","Icons/128px/armor_icon_128px.png","HP  100",new Vector2(.61f,.02f),new Vector2(.98f,.14f));

        Image titleBand = EndlessHuntSceneFactory.Image("My Equipment Header",page.transform,
            EndlessHuntSceneFactory.Ui("Headers/header_black.png"),new Vector2(0f,.36f),new Vector2(1f,.44f),Vector2.zero,Vector2.zero,Color.white);
        EndlessHuntSceneFactory.Button("Quality Filter",titleBand.transform,
            EndlessHuntSceneFactory.Ui("Buttons/buttonAdvanced_yellow.png"),"KALİTE",22,new Vector2(.025f,.12f),new Vector2(.25f,.88f),Vector2.zero,Vector2.zero);
        EndlessHuntSceneFactory.Text("Title",titleBand.transform,"EKİPMANLARIM",35,Color.white,TextAnchor.MiddleCenter,
            new Vector2(.25f,0f),new Vector2(.75f,1f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        EndlessHuntSceneFactory.Button("Merge",titleBand.transform,
            EndlessHuntSceneFactory.Ui("Buttons/buttonAdvanced_yellow.png"),"BİRLEŞTİR",21,new Vector2(.75f,.12f),new Vector2(.975f,.88f),Vector2.zero,Vector2.zero);

        RectTransform content = BuildInventoryScroll(page.transform,"Five Column Equipment Inventory",new Vector2(0f,0f),new Vector2(1f,.36f),2050f);
        menu.inventoryWeaponCards = new WeaponCardView[weapons.Length];
        for(int i=0;i<weapons.Length;i++) menu.inventoryWeaponCards[i]=InventoryWeaponCard(content,weapons[i],i);
        menu.equipmentCards = new GearCardView[GameCatalog.Gear.Length];
        for(int i=0;i<GameCatalog.Gear.Length;i++) menu.equipmentCards[i]=InventoryGearCard(content,GameCatalog.Gear[i],weapons.Length+i);
        content.GetComponentInParent<ScrollRect>().verticalNormalizedPosition=1f;
        return page.gameObject;
    }

    private static void EquippedSlot(Transform parent,int index,Sprite sprite,Vector2 min,Vector2 max,bool purple,
        out Button button,out Image icon,out Text level)
    {
        Image card = EndlessHuntSceneFactory.Image("Equipped Slot "+(index+1),parent,
            EndlessHuntSceneFactory.Ui(purple?"Item Slots/itemSlotAdvanced_purple.png":"Item Slots/itemSlotAdvanced_blue.png"),
            min,max,Vector2.zero,Vector2.zero,Color.white,true);
        button=card.gameObject.AddComponent<Button>(); button.targetGraphic=card; button.navigation=new Navigation{mode=Navigation.Mode.None};
        icon=EndlessHuntSceneFactory.Image("Equipped Icon",card.transform,sprite,new Vector2(.12f,.12f),new Vector2(.88f,.82f),Vector2.zero,Vector2.zero,Color.white); icon.preserveAspect=true;
        level=EndlessHuntSceneFactory.Text("Level",card.transform,"LV.1",24,Color.white,TextAnchor.UpperCenter,new Vector2(.03f,.7f),new Vector2(.97f,.98f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        Image arrow=EndlessHuntSceneFactory.Image("Upgrade Arrow",card.transform,EndlessHuntSceneFactory.Ui("Arrows/arrow_green.png"),new Vector2(.68f,.02f),new Vector2(.98f,.34f),Vector2.zero,Vector2.zero,Color.white); arrow.preserveAspect=true;
    }

    private static Text StatLabel(Transform parent,string name,string iconPath,string value,Vector2 min,Vector2 max)
    {
        Image root=EndlessHuntSceneFactory.Image(name,parent,EndlessHuntSceneFactory.Ui("Labels/label_backgroundV2.png"),min,max,Vector2.zero,Vector2.zero,new Color(1f,.45f,.15f,1f));
        Image icon=EndlessHuntSceneFactory.Image("Icon",root.transform,EndlessHuntSceneFactory.Ui(iconPath),new Vector2(.02f,.05f),new Vector2(.27f,.95f),Vector2.zero,Vector2.zero,Color.white); icon.preserveAspect=true;
        return EndlessHuntSceneFactory.Text("Value",root.transform,value,30,Color.white,TextAnchor.MiddleCenter,new Vector2(.23f,0f),Vector2.one,Vector2.zero,Vector2.zero,FontStyle.Bold);
    }

    private static RectTransform BuildInventoryScroll(Transform parent,string name,Vector2 min,Vector2 max,float height)
    {
        Image viewport=EndlessHuntSceneFactory.Image(name,parent,null,min,max,Vector2.zero,Vector2.zero,new Color(.1f,.11f,.16f,1f),true);
        Mask mask=viewport.gameObject.AddComponent<Mask>(); mask.showMaskGraphic=true;
        ScrollRect scroll=viewport.gameObject.AddComponent<ScrollRect>(); scroll.horizontal=false; scroll.vertical=true; scroll.movementType=ScrollRect.MovementType.Elastic; scroll.scrollSensitivity=42f;
        RectTransform content=EndlessHuntSceneFactory.Rect("Inventory Content (All Objects Persistent)",viewport.transform,new Vector2(0f,1f),new Vector2(1f,1f),new Vector2(12f,-height),new Vector2(-12f,0f));
        content.pivot=new Vector2(.5f,1f); scroll.content=content; scroll.viewport=viewport.rectTransform;
        return content;
    }

    private static WeaponCardView InventoryWeaponCard(RectTransform content,WeaponDefinition weapon,int index)
    {
        RectTransform rect=InventoryRect(content,"Weapon - "+weapon.id,index);
        Image card=rect.gameObject.AddComponent<Image>(); card.sprite=EndlessHuntSceneFactory.Ui(index<10?"Item Slots/itemSlotAdvanced_purple.png":"Item Slots/itemSlotAdvanced_blue.png"); card.type=Image.Type.Sliced; card.raycastTarget=true;
        Button button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=card; button.navigation=new Navigation{mode=Navigation.Mode.None};
        Image icon=EndlessHuntSceneFactory.Image("Icon",rect,weapon.icon,new Vector2(.12f,.12f),new Vector2(.88f,.82f),Vector2.zero,Vector2.zero,Color.white); icon.preserveAspect=true;
        Text level=EndlessHuntSceneFactory.Text("Level",rect,"LV.1",20,Color.white,TextAnchor.UpperCenter,new Vector2(.02f,.72f),new Vector2(.98f,.98f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        Text price=EndlessHuntSceneFactory.Text("Price",rect,"",14,new Color(1f,.9f,.25f),TextAnchor.LowerCenter,new Vector2(.02f,.01f),new Vector2(.98f,.2f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        Image selected=EndlessHuntSceneFactory.Image("Equipped",rect,EndlessHuntSceneFactory.Ui("Effects/effect_yellow.png"),Vector2.zero,Vector2.one,new Vector2(-3f,-3f),new Vector2(3f,3f),Color.white); selected.enabled=false;
        return new WeaponCardView{button=button,icon=icon,level=level,price=price,selectedFrame=selected};
    }

    private static GearCardView InventoryGearCard(RectTransform content,GearDefinition gear,int index)
    {
        RectTransform rect=InventoryRect(content,"Clothing - "+gear.id,index);
        Image card=rect.gameObject.AddComponent<Image>(); card.sprite=EndlessHuntSceneFactory.Ui("Item Slots/itemSlotAdvanced_blue.png"); card.type=Image.Type.Sliced; card.raycastTarget=true;
        Button button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=card; button.navigation=new Navigation{mode=Navigation.Mode.None};
        Image icon=EndlessHuntSceneFactory.Image("Icon",rect,EndlessHuntSceneFactory.GearIcon(gear),new Vector2(.12f,.12f),new Vector2(.88f,.82f),Vector2.zero,Vector2.zero,Color.white); icon.preserveAspect=true;
        Text title=EndlessHuntSceneFactory.Text("Name",rect,gear.title,14,Color.white,TextAnchor.LowerCenter,new Vector2(.02f,.01f),new Vector2(.98f,.18f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        Text status=EndlessHuntSceneFactory.Text("Level",rect,"LV.1",19,Color.white,TextAnchor.UpperCenter,new Vector2(.02f,.72f),new Vector2(.98f,.98f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        Image selected=EndlessHuntSceneFactory.Image("Equipped",rect,EndlessHuntSceneFactory.Ui("Effects/effect_yellow.png"),Vector2.zero,Vector2.one,new Vector2(-3f,-3f),new Vector2(3f,3f),Color.white); selected.enabled=false;
        return new GearCardView{button=button,icon=icon,title=title,status=status,selectedFrame=selected};
    }

    private static RectTransform InventoryRect(RectTransform content,string name,int index)
    {
        int col=index%5,row=index/5; float left=12f+col*204f,top=-12f-row*184f;
        return EndlessHuntSceneFactory.Rect(name,content,new Vector2(0f,1f),new Vector2(0f,1f),new Vector2(left,top-168f),new Vector2(left+188f,top));
    }

    private static GameObject BuildUpgrade(SurvivorMenu menu,Transform parent)
    {
        Image page=Page("Upgrade Screen",parent,Orange);
        Header(page.transform,"KARAKTER GELİŞİMİ",new Vector2(.08f,.86f),new Vector2(.92f,.98f));
        string[] names={"CAN","HASAR","HAREKET","TOPLAMA ALANI"}; menu.upgradeRows=new UpgradeRowView[4];
        for(int i=0;i<4;i++)
        {
            float top=.81f-i*.18f; Image row=EndlessHuntSceneFactory.Image(names[i],page.transform,EndlessHuntSceneFactory.Ui("Cards/card_purple.png"),new Vector2(.06f,top-.15f),new Vector2(.94f,top),Vector2.zero,Vector2.zero,Color.white);
            Text title=EndlessHuntSceneFactory.Text("Stat",row.transform,names[i],30,Color.white,TextAnchor.MiddleLeft,new Vector2(.06f,.43f),new Vector2(.58f,.9f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            Text level=EndlessHuntSceneFactory.Text("Level",row.transform,"LV.1",22,new Color(.8f,.9f,1f),TextAnchor.MiddleLeft,new Vector2(.06f,.08f),new Vector2(.58f,.5f),Vector2.zero,Vector2.zero,FontStyle.Bold);
            Button button=EndlessHuntSceneFactory.Button("Upgrade",row.transform,EndlessHuntSceneFactory.Ui("Buttons/buttonAdvanced_yellow.png"),"",22,new Vector2(.6f,.18f),new Vector2(.95f,.82f),Vector2.zero,Vector2.zero);
            Text cost=EndlessHuntSceneFactory.Text("Cost",button.transform,"150 ALTIN",21,Color.white,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one,new Vector2(5f,4f),new Vector2(-5f,-4f),FontStyle.Bold);
            menu.upgradeRows[i]=new UpgradeRowView{title=title,level=level,cost=cost,button=button};
        }
        return page.gameObject;
    }

    private static GameObject BuildStore(SurvivorMenu menu,Transform parent,GameObject startPrefab,WeaponDefinition[] weapons)
    {
        Image page=Page("Arsenal Store Screen",parent,Orange);
        Header(page.transform,"SİLAH MAĞAZASI",new Vector2(.08f,.87f),new Vector2(.92f,.99f));
        Image detail=EndlessHuntSceneFactory.Image("Selected Weapon And Character",page.transform,EndlessHuntSceneFactory.Ui("Modals/modalSimple_v2.png"),new Vector2(.035f,.57f),new Vector2(.965f,.87f),Vector2.zero,Vector2.zero,Color.white);
        menu.storePreview=EndlessHuntSceneFactory.SpineGraphic(startPrefab,"Store Character Preview",detail.transform,new Vector2(.03f,.05f),new Vector2(.34f,.9f),Vector2.zero,Vector2.zero,1.75f);
        menu.weaponDetailIcon=EndlessHuntSceneFactory.Image("Weapon Icon",detail.transform,weapons[0].icon,new Vector2(.37f,.38f),new Vector2(.57f,.9f),Vector2.zero,Vector2.zero,Color.white); menu.weaponDetailIcon.preserveAspect=true;
        menu.weaponDetailTitle=EndlessHuntSceneFactory.Text("Weapon Name",detail.transform,"SİLAH",27,Ink,TextAnchor.MiddleLeft,new Vector2(.57f,.68f),new Vector2(.97f,.91f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        menu.weaponDetailStats=EndlessHuntSceneFactory.Text("Stats",detail.transform,"ATK",20,Ink,TextAnchor.UpperLeft,new Vector2(.38f,.27f),new Vector2(.96f,.68f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        menu.weaponActionButton=EndlessHuntSceneFactory.Button("Buy Or Equip",detail.transform,EndlessHuntSceneFactory.Ui("Buttons/buttonAdvanced_blue.png"),"",20,new Vector2(.38f,.04f),new Vector2(.67f,.26f),Vector2.zero,Vector2.zero);
        menu.weaponActionText=EndlessHuntSceneFactory.Text("Action",menu.weaponActionButton.transform,"KUŞAN",19,Color.white,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one,new Vector2(5f,4f),new Vector2(-5f,-4f),FontStyle.Bold);
        menu.weaponUpgradeButton=EndlessHuntSceneFactory.Button("Upgrade Weapon",detail.transform,EndlessHuntSceneFactory.Ui("Buttons/buttonAdvanced_yellow.png"),"",20,new Vector2(.68f,.04f),new Vector2(.97f,.26f),Vector2.zero,Vector2.zero);
        menu.weaponUpgradeText=EndlessHuntSceneFactory.Text("Upgrade Cost",menu.weaponUpgradeButton.transform,"GELİŞTİR",18,Color.white,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one,new Vector2(5f,4f),new Vector2(-5f,-4f),FontStyle.Bold);
        RectTransform content=BuildInventoryScroll(page.transform,"Weapon Store Grid",new Vector2(0f,0f),new Vector2(1f,.56f),1750f);
        menu.weaponCards=new WeaponCardView[weapons.Length];
        for(int i=0;i<weapons.Length;i++) menu.weaponCards[i]=InventoryWeaponCard(content,weapons[i],i);
        content.GetComponentInParent<ScrollRect>().verticalNormalizedPosition=1f;
        return page.gameObject;
    }

    private static void BuildNavigation(SurvivorMenu menu,Transform parent)
    {
        Image nav=EndlessHuntSceneFactory.Image("Five Button Bottom Navigation",parent,
            EndlessHuntSceneFactory.Ui("Containers/ButtonContainer.png"),Vector2.zero,new Vector2(1f,.108f),Vector2.zero,Vector2.zero,Color.white);
        string[] labels={"MAĞAZA","EKİPMAN","SAVAŞ","GELİŞİM","SİLAHLAR"};
        string[] sprites={"Buttons/shop_1ButtonDefault.png","Buttons/equipment_1ButtonDefault.png","Buttons/adventure_1ButtonDefault.png","Buttons/skills_1ButtonDefault.png","Buttons/store_1ButtonDefault.png"};
        Button[] buttons=new Button[5]; menu.tabHighlights=new Image[5];
        for(int i=0;i<5;i++)
        {
            float left=i*.2f; buttons[i]=EndlessHuntSceneFactory.Button(labels[i]+" Tab",nav.transform,EndlessHuntSceneFactory.Ui(sprites[i]),"",16,new Vector2(left,.02f),new Vector2(left+.2f,.98f),new Vector2(3f,2f),new Vector2(-3f,-2f));
            menu.tabHighlights[i]=EndlessHuntSceneFactory.Image("Selected Orange Glow",buttons[i].transform,EndlessHuntSceneFactory.Ui("Effects/effect_yellow.png"),Vector2.zero,Vector2.one,new Vector2(-4f,-4f),new Vector2(4f,4f),new Color(1f,.65f,.12f,.85f));
            EndlessHuntSceneFactory.Text("Tab Label",buttons[i].transform,labels[i],i==2?20:15,Color.white,TextAnchor.LowerCenter,new Vector2(0f,0f),new Vector2(1f,.32f),new Vector2(2f,0f),new Vector2(-2f,0f),FontStyle.Bold);
        }
        menu.storeTab=buttons[0]; menu.equipmentTab=buttons[1]; menu.homeTab=buttons[2]; menu.upgradeTab=buttons[3]; menu.arsenalTab=buttons[4];
    }

    private static void BuildConfirmation(SurvivorMenu menu,Transform parent)
    {
        Image dim=EndlessHuntSceneFactory.Image("Purchase Confirmation (Prepared)",parent,EndlessHuntSceneFactory.Ui("Modals/darkModalSimple_dark.png"),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,Color.white,true); menu.confirmationPanel=dim.gameObject;
        Image modal=EndlessHuntSceneFactory.Image("Confirmation Card",dim.transform,EndlessHuntSceneFactory.Ui("Modals/modalSimple_v1.png"),new Vector2(.1f,.31f),new Vector2(.9f,.69f),Vector2.zero,Vector2.zero,Color.white,true);
        Image ribbon=EndlessHuntSceneFactory.Image("Blue Ribbon",modal.transform,EndlessHuntSceneFactory.Ui("Headers/header_blue.png"),new Vector2(.04f,.77f),new Vector2(.96f,1.05f),Vector2.zero,Vector2.zero,Color.white);
        menu.confirmationTitle=EndlessHuntSceneFactory.Text("Title",ribbon.transform,"SATIN AL",38,Color.white,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one,new Vector2(15f,5f),new Vector2(-15f,-5f),FontStyle.Bold);
        menu.confirmationIcon=EndlessHuntSceneFactory.Image("Item Icon",modal.transform,null,new Vector2(.08f,.36f),new Vector2(.34f,.72f),Vector2.zero,Vector2.zero,Color.white); menu.confirmationIcon.preserveAspect=true;
        menu.confirmationBody=EndlessHuntSceneFactory.Text("Question",modal.transform,"Satın almak istiyor musun?",24,Ink,TextAnchor.MiddleCenter,new Vector2(.34f,.34f),new Vector2(.93f,.73f),Vector2.zero,Vector2.zero,FontStyle.Bold);
        menu.confirmationAccept=EndlessHuntSceneFactory.Button("Confirm",modal.transform,EndlessHuntSceneFactory.Ui("Buttons/buttonAdvanced_yellow.png"),"",24,new Vector2(.08f,.08f),new Vector2(.47f,.3f),Vector2.zero,Vector2.zero);
        menu.confirmationPrice=EndlessHuntSceneFactory.Text("Price",menu.confirmationAccept.transform,"100 ELMAS",20,Color.white,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one,new Vector2(4f,3f),new Vector2(-4f,-3f),FontStyle.Bold);
        menu.confirmationCancel=EndlessHuntSceneFactory.Button("Cancel",modal.transform,EndlessHuntSceneFactory.Ui("Buttons/buttonAdvanced_blue.png"),"VAZGEÇ",23,new Vector2(.53f,.08f),new Vector2(.92f,.3f),Vector2.zero,Vector2.zero);
        dim.gameObject.SetActive(false);
    }

    private static void BuildToast(SurvivorMenu menu,Transform parent)
    {
        Image toast=EndlessHuntSceneFactory.Image("Toast (Prepared)",parent,EndlessHuntSceneFactory.Ui("Labels/labelAdvanced_red.png"),new Vector2(.12f,.47f),new Vector2(.88f,.55f),Vector2.zero,Vector2.zero,Color.white);
        menu.toastPanel=toast.gameObject; menu.toastText=EndlessHuntSceneFactory.Text("Message",toast.transform,"",27,Color.white,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one,new Vector2(12f,6f),new Vector2(-12f,-6f),FontStyle.Bold); toast.gameObject.SetActive(false);
    }

    private static Image Page(string name,Transform parent,Color color) => EndlessHuntSceneFactory.Image(name,parent,
        EndlessHuntSceneFactory.Ui("Panels/panel_yellow.png"),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,color);

    private static void Header(Transform parent,string title,Vector2 min,Vector2 max)
    {
        Image header=EndlessHuntSceneFactory.Image("Header",parent,EndlessHuntSceneFactory.Ui("Headers/header_black.png"),min,max,Vector2.zero,Vector2.zero,Color.white);
        EndlessHuntSceneFactory.Text("Title",header.transform,title,38,Color.white,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one,new Vector2(15f,5f),new Vector2(-15f,-5f),FontStyle.Bold);
    }

    private static Button IconButton(string name,Transform parent,string iconPath,Vector2 min,Vector2 max)
    {
        Button button=EndlessHuntSceneFactory.Button(name,parent,EndlessHuntSceneFactory.Ui("Buttons/buttonSquare_blue.png"),"",18,min,max,Vector2.zero,Vector2.zero);
        Image icon=EndlessHuntSceneFactory.Image("Icon",button.transform,EndlessHuntSceneFactory.Ui(iconPath),new Vector2(.14f,.14f),new Vector2(.86f,.86f),Vector2.zero,Vector2.zero,Color.white); icon.preserveAspect=true;
        return button;
    }

    private static Sprite GearSlotIcon(int index)
    {
        GearSlot slot=index switch { 1=>GearSlot.Top,2=>GearSlot.Helmet,3=>GearSlot.Bottom,4=>GearSlot.Hair,_=>GearSlot.Boots };
        GearDefinition gear=GameProfile.FindEquipped(slot);
        return gear!=null?EndlessHuntSceneFactory.GearIcon(gear):EndlessHuntSceneFactory.Ui("Icons/128px/armor_icon_128px.png");
    }
}
