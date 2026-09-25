using EndlessHunt.UI;
using EndlessHunt.UI.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

internal static class EndlessHuntUICommonPrefabBuilder
{
    internal static void Build(UITheme theme)
    {
        BuildNotificationBadge(theme);
        BuildItemCard(theme);
        BuildEquipmentSlot(theme);
        BuildTalentNode(theme);
        BuildRewardItem(theme);
        BuildPopupFrame(theme);
        BuildToast(theme);
    }

    internal static UINotificationBadge AddBadgeVisual(Transform parent, UITheme theme)
    {
        RectTransform holder = EndlessHuntUIPrefabFactory.Rect("Notification Badge", parent,
            new Vector2(.58f, .64f), new Vector2(1.08f, 1.08f), Vector2.zero, Vector2.zero);
        UINotificationBadge badge = holder.gameObject.AddComponent<UINotificationBadge>();
        Image display = EndlessHuntUIPrefabFactory.Image("Display", holder, theme.badgeSprite, Vector2.zero,
            Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        EndlessHuntUIPrefabFactory.ApplySliced(display);
        Text label = EndlessHuntUIPrefabFactory.Text("Label", display.transform, "!", theme.captionSize,
            theme.textPrimary, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
            new Vector2(5f, 2f), new Vector2(-5f, -2f), FontStyle.Bold);
        badge.Configure(display.gameObject, label);
        return badge;
    }

    private static void BuildNotificationBadge(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_NotificationBadge", new Vector2(190f, 72f));
        RectTransform holder = EndlessHuntUIPrefabFactory.Rect("Badge Host", root, Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero);
        UINotificationBadge badge = holder.gameObject.AddComponent<UINotificationBadge>();
        Image display = EndlessHuntUIPrefabFactory.Image("Display", holder, theme.badgeSprite, Vector2.zero,
            Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        EndlessHuntUIPrefabFactory.ApplySliced(display);
        Text label = EndlessHuntUIPrefabFactory.Text("Label", display.transform, "NEW", theme.captionSize,
            theme.textPrimary, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
            new Vector2(8f, 3f), new Vector2(-8f, -3f), FontStyle.Bold);
        badge.Configure(display.gameObject, label);
        EndlessHuntUIPrefabFactory.Save(root, "UI_NotificationBadge");
    }

    private static void BuildItemCard(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_ItemCard", new Vector2(260f, 330f),
            typeof(Image), typeof(Button), typeof(LayoutElement), typeof(UIItemCard));
        Image background = root.GetComponent<Image>();
        background.sprite = theme.itemSlot;
        background.raycastTarget = true;
        EndlessHuntUIPrefabFactory.ApplySliced(background);
        Button button = root.GetComponent<Button>();
        button.targetGraphic = background;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minWidth = 150f;
        layout.minHeight = 190f;
        layout.preferredWidth = 260f;
        layout.preferredHeight = 330f;
        Image icon = EndlessHuntUIPrefabFactory.Image("Icon", root, null, new Vector2(.12f, .28f),
            new Vector2(.88f, .9f), Vector2.zero, Vector2.zero, Color.white);
        icon.preserveAspect = true;
        Text title = EndlessHuntUIPrefabFactory.Text("Title", root, "ITEM", theme.bodySize,
            theme.textPrimary, TextAnchor.MiddleCenter, new Vector2(.05f, .12f), new Vector2(.95f, .29f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Text subtitle = EndlessHuntUIPrefabFactory.Text("Subtitle", root, "DETAIL", theme.captionSize,
            theme.textSecondary, TextAnchor.MiddleCenter, new Vector2(.05f, .02f), new Vector2(.95f, .14f),
            Vector2.zero, Vector2.zero);
        Image selection = EndlessHuntUIPrefabFactory.AddSelection(root);
        selection.transform.SetAsFirstSibling();
        Image disabled = EndlessHuntUIPrefabFactory.Image("Disabled Overlay", root, null, Vector2.zero,
            Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, .55f));
        disabled.gameObject.SetActive(false);
        root.GetComponent<UIItemCard>().Configure(button, icon, title, subtitle, selection, disabled.gameObject);
        EndlessHuntUIPrefabFactory.Save(root, "UI_ItemCard");
    }

    private static void BuildEquipmentSlot(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_EquipmentSlot", new Vector2(210f, 230f),
            typeof(Image), typeof(Button), typeof(LayoutElement), typeof(UIEquipmentSlot));
        Image background = root.GetComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui("Item Slots/itemSlotAdvanced_purple.png");
        background.raycastTarget = true;
        EndlessHuntUIPrefabFactory.ApplySliced(background);
        Button button = root.GetComponent<Button>();
        button.targetGraphic = background;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minWidth = 132f;
        layout.minHeight = 132f;
        layout.preferredWidth = 210f;
        layout.preferredHeight = 230f;
        Image icon = EndlessHuntUIPrefabFactory.Image("Icon", root, null, new Vector2(.12f, .13f),
            new Vector2(.88f, .84f), Vector2.zero, Vector2.zero, Color.white);
        icon.preserveAspect = true;
        Text level = EndlessHuntUIPrefabFactory.Text("Level", root, "LV.1", theme.captionSize,
            theme.textPrimary, TextAnchor.UpperCenter, new Vector2(.05f, .78f), new Vector2(.95f, .98f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Text empty = EndlessHuntUIPrefabFactory.Text("Empty", root, "+", theme.titleSize, theme.textSecondary,
            TextAnchor.MiddleCenter, new Vector2(.2f, .2f), new Vector2(.8f, .8f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Image selection = EndlessHuntUIPrefabFactory.AddSelection(root);
        selection.transform.SetAsFirstSibling();
        root.GetComponent<UIEquipmentSlot>().Configure("Slot", button, icon, level, empty.gameObject, selection);
        EndlessHuntUIPrefabFactory.Save(root, "UI_EquipmentSlot");
    }

    private static void BuildTalentNode(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_TalentNode", new Vector2(820f, 170f),
            typeof(Image), typeof(Button), typeof(LayoutElement), typeof(UITalentNode));
        Image background = root.GetComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui("Cards/card_purple.png");
        background.raycastTarget = true;
        EndlessHuntUIPrefabFactory.ApplySliced(background);
        Button button = root.GetComponent<Button>();
        button.targetGraphic = background;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minHeight = 120f;
        layout.preferredHeight = 170f;
        Image icon = EndlessHuntUIPrefabFactory.Image("Icon", root, null, new Vector2(.03f, .14f),
            new Vector2(.2f, .86f), Vector2.zero, Vector2.zero, Color.white);
        icon.preserveAspect = true;
        Text title = EndlessHuntUIPrefabFactory.Text("Title", root, "TALENT", theme.bodySize,
            theme.textPrimary, TextAnchor.MiddleLeft, new Vector2(.23f, .48f), new Vector2(.67f, .9f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Text level = EndlessHuntUIPrefabFactory.Text("Level", root, "LV.0", theme.captionSize,
            theme.textSecondary, TextAnchor.MiddleLeft, new Vector2(.23f, .1f), new Vector2(.5f, .52f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Text cost = EndlessHuntUIPrefabFactory.Text("Cost", root, "COST", theme.captionSize,
            theme.accent, TextAnchor.MiddleCenter, new Vector2(.67f, .18f), new Vector2(.95f, .82f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Image locked = EndlessHuntUIPrefabFactory.Image("Locked Overlay", root, null, Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, .62f));
        Text lockText = EndlessHuntUIPrefabFactory.Text("Label", locked.transform, "LOCKED", theme.bodySize,
            theme.textPrimary, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            FontStyle.Bold);
        lockText.raycastTarget = false;
        locked.gameObject.SetActive(false);
        root.GetComponent<UITalentNode>().Configure(button, icon, title, level, cost, locked.gameObject);
        EndlessHuntUIPrefabFactory.Save(root, "UI_TalentNode");
    }

    private static void BuildRewardItem(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_RewardItem", new Vector2(250f, 270f),
            typeof(Image), typeof(LayoutElement), typeof(UIRewardItem));
        Image background = root.GetComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui("Containers/containerReward_yellow.png");
        EndlessHuntUIPrefabFactory.ApplySliced(background);
        Image icon = EndlessHuntUIPrefabFactory.Image("Icon", root, null, new Vector2(.17f, .3f),
            new Vector2(.83f, .88f), Vector2.zero, Vector2.zero, Color.white);
        icon.preserveAspect = true;
        Text title = EndlessHuntUIPrefabFactory.Text("Title", root, "REWARD", theme.captionSize,
            theme.textPrimary, TextAnchor.MiddleCenter, new Vector2(.05f, .13f), new Vector2(.95f, .32f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Text amount = EndlessHuntUIPrefabFactory.Text("Amount", root, "+0", theme.bodySize,
            theme.accent, TextAnchor.MiddleCenter, new Vector2(.05f, .01f), new Vector2(.95f, .16f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        root.GetComponent<UIRewardItem>().Configure(icon, title, amount);
        EndlessHuntUIPrefabFactory.Save(root, "UI_RewardItem");
    }

    private static void BuildPopupFrame(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_PopupFrame", new Vector2(1080f, 1920f),
            typeof(Image), typeof(CanvasGroup), typeof(UIAnimationController), typeof(UIPopup),
            typeof(UIPopupFrame));
        Image dim = root.GetComponent<Image>();
        dim.sprite = EndlessHuntUIPrefabFactory.Ui("Modals/darkModalSimple_dark.png");
        dim.raycastTarget = true;
        Image card = EndlessHuntUIPrefabFactory.Image("Popup Card", root, theme.modalSprite,
            new Vector2(.08f, .23f), new Vector2(.92f, .77f), Vector2.zero, Vector2.zero, Color.white, true);
        EndlessHuntUIPrefabFactory.ApplySliced(card);
        Image header = EndlessHuntUIPrefabFactory.Image("Header", card.transform,
            EndlessHuntUIPrefabFactory.Ui("Headers/header_blue.png"), new Vector2(.03f, .82f),
            new Vector2(.97f, 1.02f), Vector2.zero, Vector2.zero, Color.white);
        Text title = EndlessHuntUIPrefabFactory.Text("Title", header.transform, "TITLE", theme.titleSize,
            theme.textPrimary, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
            new Vector2(70f, 8f), new Vector2(-70f, -8f), FontStyle.Bold);
        Button close = EndlessHuntUIPrefabFactory.Button("Close", card.transform,
            EndlessHuntUIPrefabFactory.Ui("Buttons/buttonSquare_blue.png"), "X", theme.bodySize,
            new Vector2(.84f, .84f), new Vector2(.97f, .98f), Vector2.zero, Vector2.zero);
        RectTransform content = EndlessHuntUIPrefabFactory.Rect("Content", card.transform,
            new Vector2(.06f, .22f), new Vector2(.94f, .8f), Vector2.zero, Vector2.zero);
        RectTransform footer = EndlessHuntUIPrefabFactory.Rect("Footer", card.transform,
            new Vector2(.06f, .04f), new Vector2(.94f, .2f), Vector2.zero, Vector2.zero);
        HorizontalLayoutGroup footerLayout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
        footerLayout.spacing = theme.commonSpacing;
        footerLayout.childAlignment = TextAnchor.MiddleCenter;
        footerLayout.childControlWidth = true;
        footerLayout.childControlHeight = true;
        footerLayout.childForceExpandWidth = true;
        footerLayout.childForceExpandHeight = true;
        Button secondary = ActionButton("Secondary Action", footer, theme, theme.buttonNormal, "CANCEL");
        Button primary = ActionButton("Primary Action", footer, theme, theme.buttonSelected, "CONFIRM");
        UIPopup popup = root.GetComponent<UIPopup>();
        popup.Configure("Popup", card.rectTransform, true);
        root.GetComponent<UIPopupFrame>().Configure(popup, title, content, footer, close, primary, secondary);
        EndlessHuntUIPrefabFactory.Save(root, "UI_PopupFrame");
    }

    private static Button ActionButton(string name, Transform parent, UITheme theme, Sprite sprite, string label)
    {
        Button button = EndlessHuntUIPrefabFactory.Button(name, parent, sprite, label, theme.bodySize,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = theme.buttonHeight;
        layout.preferredHeight = theme.buttonHeight;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        return button;
    }

    private static void BuildToast(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_Toast", new Vector2(760f, 104f),
            typeof(Image), typeof(CanvasGroup), typeof(LayoutElement), typeof(UIAnimationController),
            typeof(UIToast));
        Image background = root.GetComponent<Image>();
        background.sprite = theme.toastSprite;
        background.raycastTarget = false;
        EndlessHuntUIPrefabFactory.ApplySliced(background);
        Text message = EndlessHuntUIPrefabFactory.Text("Message", root, "MESSAGE", theme.bodySize,
            theme.textPrimary, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
            new Vector2(theme.largeSpacing, theme.smallSpacing),
            new Vector2(-theme.largeSpacing, -theme.smallSpacing), FontStyle.Bold);
        CanvasGroup group = root.GetComponent<CanvasGroup>();
        UIAnimationController animation = root.GetComponent<UIAnimationController>();
        root.GetComponent<UIToast>().Configure(message, group, root, animation);
        EndlessHuntUIPrefabFactory.Save(root, "UI_Toast");
    }
}
