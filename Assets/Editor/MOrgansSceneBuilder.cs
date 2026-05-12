using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public static class MOrgansSceneBuilder
{
    const string ScenePath = "Assets/_Project/Scenes";
    const string DataPath  = "Assets/_Project/Data";
    const float  CH        = 88f;   // chrome bar height
    const float  FH        = 100f;  // footer bar height

    static MO2ThemeData    _light;
    static OrganDefinition _jantung, _otak, _lambung;
    static Color _paper, _ink, _inkMute, _ink2, _surface, _paperDeep, _accent;

    // ── ENTRY POINT ─────────────────────────────────────────────────────────
    public static void BuildAllScenes()
    {
        LoadAssets();
        EnsureFolder(ScenePath);
        BuildPersistent();
        BuildSplash();
        BuildOnboarding();
        BuildPicker();
        BuildDetail();
        BuildAR();
        BuildQuiz();
        BuildSettings();
        AssetDatabase.Refresh();
        UpdateBuildSettings();
        Debug.Log("[MOrgans] All scenes built.");
    }

    static void LoadAssets()
    {
        _light    = AssetDatabase.LoadAssetAtPath<MO2ThemeData>($"{DataPath}/MO2Theme_Light.asset");
        _jantung  = AssetDatabase.LoadAssetAtPath<OrganDefinition>($"{DataPath}/Organ_Jantung.asset");
        _otak     = AssetDatabase.LoadAssetAtPath<OrganDefinition>($"{DataPath}/Organ_Otak.asset");
        _lambung  = AssetDatabase.LoadAssetAtPath<OrganDefinition>($"{DataPath}/Organ_Lambung.asset");
        _paper     = T(_light, t=>t.paper,     new Color(0.945f,0.925f,0.886f));
        _ink       = T(_light, t=>t.ink,       new Color(0.102f,0.090f,0.078f));
        _inkMute   = T(_light, t=>t.inkMute,   new Color(0.478f,0.435f,0.392f));
        _ink2      = T(_light, t=>t.ink2,      new Color(0.239f,0.212f,0.192f));
        _surface   = T(_light, t=>t.surface,   new Color(0.984f,0.969f,0.933f));
        _paperDeep = T(_light, t=>t.paperDeep, new Color(0.910f,0.882f,0.824f));
        _accent    = T(_light, t=>t.accent,    new Color(0.761f,0.255f,0.047f));
    }
    static Color T(MO2ThemeData d, System.Func<MO2ThemeData,Color> f, Color def) => d != null ? f(d) : def;

    // ── _PERSISTENT ─────────────────────────────────────────────────────────
    static void BuildPersistent()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("AppState").AddComponent<AppState>();
        new GameObject("LocalizationManager").AddComponent<LocalizationManager>();
        var tm = new GameObject("ThemeManager").AddComponent<ThemeManager>();
        Wire(tm, "lightTheme", _light);
        Wire(tm, "darkTheme", AssetDatabase.LoadAssetAtPath<MO2ThemeData>($"{DataPath}/MO2Theme_Dark.asset"));
        var fc = new GameObject("FadeCanvas").AddComponent<Canvas>();
        fc.renderMode = RenderMode.ScreenSpaceOverlay; fc.sortingOrder = 999;
        fc.gameObject.AddComponent<CanvasScaler>(); fc.gameObject.AddComponent<GraphicRaycaster>();
        var fadePanelGO = new GameObject("FadePanel"); fadePanelGO.transform.SetParent(fc.transform, false);
        StretchRT(fadePanelGO);
        var fadeImg = fadePanelGO.AddComponent<Image>(); fadeImg.color = new Color(0,0,0,0);
        var sl = new GameObject("SceneLoader").AddComponent<SceneLoader>();
        Wire(sl, "fadePanel", fadeImg);
        SaveScene(scene, "_Persistent");
    }

    // ── 01_SPLASH ────────────────────────────────────────────────────────────
    // Layout: Chrome("M'ORGANS / ID" + lang toggle) | RuleTop | hero | RuleBot | Footer(version + start)
    static void BuildSplash()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddEventSystem();
        var (rt, ctrl) = MakeScene<SplashController>("Canvas");

        var bg = MakeStretchImg(rt, "Background", _paper);

        // Chrome bar
        var chrome = MakeChromeBar(rt);
        var idxTxt = MakeMonoLabel(chrome, "IndexLabel", "M'ORGANS", new Vector2(-200,0), new Vector2(500,CH));
        var (langBg, langTxt) = MakePinBtn(chrome, "LangButton", "EN", new Vector2(300,0), new Vector2(80,44), Color.clear, _ink);

        // Rules
        var ruleTop = MakeRuleBelow(rt, "RuleTop", CH);
        var ruleBot = MakeRuleAbove(rt, "RuleBottom", FH);

        // Hero content (center-anchored coordinates, y = canvas-center units)
        var plate   = MakePlate(rt, "OrganPlate", new Vector2(0, 80), 186f);
        var appName = MakeItalic(rt, "AppNameText", "M'Organs", 65, TextAlignmentOptions.Left, _ink,     new Vector2(-40, 650), new Vector2(840,130));
        var tagline = MakeItalic(rt, "TaglineText", "Tagline.", 17, TextAlignmentOptions.Left, _ink2,    new Vector2(-40, 510), new Vector2(720, 80));
        var sub     = MakeTMP(rt,   "SubtitleText", "Subtitle.",13, TextAlignmentOptions.Left, _inkMute, new Vector2(-40, 440), new Vector2(720, 60));

        // Footer
        var verTxt                  = MakeMonoLabel(rt, "VersionText",   "v0.4", new Vector2(-220,-905), new Vector2(500,44));
        var (startBg, startTxt)     = MakePinBtn(rt, "StartButton", "Mulai →",   new Vector2(290,-905),  new Vector2(210,48), _ink, _paper);

        Wire(ctrl, "backgroundImage", bg);       Wire(ctrl, "ruleTop",        ruleTop);
        Wire(ctrl, "ruleBottom",      ruleBot);   Wire(ctrl, "organPlate",     plate);
        Wire(ctrl, "appNameText",     appName);   Wire(ctrl, "taglineText",    tagline);
        Wire(ctrl, "subtitleText",    sub);        Wire(ctrl, "versionText",   verTxt);
        Wire(ctrl, "startButtonText", startTxt);  Wire(ctrl, "langButtonText", langTxt);
        Wire(ctrl, "heartOrgan",      _jantung);
        BindClick(startBg, ctrl, "OnStartPressed"); BindClick(langBg, ctrl, "OnLangPressed");
        SaveScene(scene, "01_Splash");
    }

    // ── 02_ONBOARDING ────────────────────────────────────────────────────────
    // Layout: Chrome(progress + skip) | RuleTop | plate + text | RuleBot | Footer(dots + next)
    static void BuildOnboarding()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddEventSystem();
        var (rt, ctrl) = MakeScene<OnboardingController>("Canvas");

        var bg = MakeStretchImg(rt, "Background", _paper);

        var chrome   = MakeChromeBar(rt);
        var progTxt  = MakeMonoLabel(chrome, "ProgressText", "01 / 03", new Vector2(-300,0), new Vector2(300,CH));
        var (skipBg, skipTxt) = MakePinBtn(chrome, "SkipButton", "Lewati",   new Vector2(310,0),  new Vector2(140,44), Color.clear, _inkMute);

        var ruleTop = MakeRuleBelow(rt, "RuleTop", CH);
        var ruleBot = MakeRuleAbove(rt, "RuleBottom", FH);

        var plate   = MakePlate(rt, "OrganPlate", new Vector2(0, 450), 188f);
        var kicker  = MakeMonoLabel(rt, "KickerText",   "01 — M'Organs", new Vector2(-40, 130), new Vector2(820, 36));
        var title   = MakeItalic(rt,    "TitleText",    "Title.",  32, TextAlignmentOptions.Left, _ink,  new Vector2(-40, 70),  new Vector2(820,100));
        var body    = MakeTMP(rt,        "BodyText",    "Body",    14, TextAlignmentOptions.Left, _ink2, new Vector2(-40,-50),  new Vector2(820,160));

        // Footer: progress bars (left) + next button (right)
        var (nextBg, nextTxt) = MakePinBtn(rt, "NextButton", "Lanjut →", new Vector2(290,-905), new Vector2(210,48), _ink, _paper);
        var dots = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var d = new GameObject($"Dot{i}"); d.transform.SetParent(rt, false);
            var dRT = d.AddComponent<RectTransform>();
            dRT.anchorMin = dRT.anchorMax = new Vector2(0.5f,0.5f); dRT.pivot = new Vector2(0.5f,0.5f);
            dRT.anchoredPosition = new Vector2(-220 + i*28f, -905); dRT.sizeDelta = new Vector2(18, 2);
            dots[i] = d.AddComponent<Image>();
            dots[i].color = i == 0 ? _ink : new Color(_ink.r,_ink.g,_ink.b,0.2f);
        }

        Wire(ctrl, "backgroundImage", bg);     Wire(ctrl, "ruleTop",        ruleTop);
        Wire(ctrl, "ruleBottom",      ruleBot); Wire(ctrl, "organPlate",     plate);
        Wire(ctrl, "kickerText",      kicker);  Wire(ctrl, "titleText",      title);
        Wire(ctrl, "bodyText",        body);    Wire(ctrl, "progressText",   progTxt);
        Wire(ctrl, "skipText",        skipTxt); Wire(ctrl, "nextButtonText", nextTxt);
        WireArr(ctrl, "progressDots",     new Object[]{ dots[0],dots[1],dots[2] });
        WireArr(ctrl, "onboardingOrgans", new Object[]{ _jantung,_otak,_lambung });
        BindClick(nextBg, ctrl, "OnNextPressed");
        BindClick(skipBg, ctrl, "OnSkipPressed");
        SaveScene(scene, "02_Onboarding");
    }

    // ── 03_PICKER ────────────────────────────────────────────────────────────
    // Layout: Chrome(back) | RuleTop | mono count + heading | list items + rules | soon grid
    static void BuildPicker()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddEventSystem();
        var (rt, ctrl) = MakeScene<PickerController>("Canvas");

        var bg = MakeStretchImg(rt, "Background", _paper);

        var chrome    = MakeChromeBar(rt);
        var (backBg, backTxt2) = MakePinBtn(chrome, "BackButton", "← Kembali", new Vector2(-270,0), new Vector2(220,44), Color.clear, _ink);

        var ruleTop   = MakeRuleBelow(rt, "RuleTop", CH);

        var countLbl  = MakeMonoLabel(rt, "CountText",  "Tiga tersedia", new Vector2(-40, 760), new Vector2(820, 36));
        var titleTxt  = MakeItalic(rt,   "TitleText",  "Pilih organ.",  38, TextAlignmentOptions.Left, _ink, new Vector2(-40, 690), new Vector2(820, 90));

        // Rule below title
        MakeInnerRule(rt, "RuleBelowTitle", 620f);

        // List items (rules between them come from the item's own divider)
        float[] yPos = { 470f, 310f, 150f };
        var items = new OrganListItemUI[3];
        for (int i = 0; i < 3; i++) items[i] = MakeListItem(rt, $"ListItem_{i}", yPos[i]);

        // Rule above "soon" section
        MakeInnerRule(rt, "RuleSoon", -10f);

        // "Soon" section — static, not wired to controller
        MakeMonoLabel(rt, "SoonLabel", "Belum tersedia", new Vector2(-40,-70), new Vector2(500,36));
        string[] soonKeys = { "paru", "ginjal", "hati" };
        for (int i = 0; i < 3; i++)
        {
            var sGO = new GameObject($"Soon_{soonKeys[i]}"); sGO.transform.SetParent(rt, false);
            var sRT = sGO.AddComponent<RectTransform>();
            sRT.anchorMin = sRT.anchorMax = new Vector2(0.5f,0.5f); sRT.pivot = new Vector2(0.5f,0.5f);
            sRT.anchoredPosition = new Vector2(-200 + i*200f, -200); sRT.sizeDelta = new Vector2(180, 100);
            var sBg = sGO.AddComponent<Image>(); sBg.color = new Color(_ink.r,_ink.g,_ink.b,0.06f);
            var sLbl = MakeMonoLabel(sGO.transform, "Label", soonKeys[i], new Vector2(0,0), new Vector2(160,36));
            sLbl.alignment = TextAlignmentOptions.Center;
        }
        var soonNote = MakeTMP(rt, "SoonNote", "Model 3D belum dibuat.", 11, TextAlignmentOptions.Left, _inkMute, new Vector2(-40,-330), new Vector2(600,36));
        soonNote.fontStyle = FontStyles.Italic;

        Wire(ctrl, "backgroundImage", bg);    Wire(ctrl, "ruleTop",   ruleTop);
        Wire(ctrl, "titleText", titleTxt);    Wire(ctrl, "countText", countLbl);
        WireArr(ctrl, "organs",    new Object[]{ _jantung,_otak,_lambung });
        WireArr(ctrl, "listItems", new Object[]{ items[0],items[1],items[2] });
        BindClick(backBg, ctrl, "OnBackPressed");
        SaveScene(scene, "03_Picker");
    }

    // ── 04_DETAIL ────────────────────────────────────────────────────────────
    // Layout: Chrome(back + "Bab 01") | Rule | plate | mono + name | facts | para | funfact | Rule | Footer(place + quiz)
    static void BuildDetail()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddEventSystem();
        var (rt, ctrl) = MakeScene<DetailController>("Canvas");

        var bg = MakeStretchImg(rt, "Background", _paper);

        var chrome     = MakeChromeBar(rt);
        var (backBg, _)   = MakePinBtn(chrome, "BackButton", "← Kembali", new Vector2(-270,0), new Vector2(220,44), Color.clear, _ink);
        var chapterTxt    = MakeMonoLabel(chrome, "ChapterText", "Bab 01", new Vector2(280,0), new Vector2(200,CH));
        chapterTxt.alignment = TextAlignmentOptions.Right;

        var ruleTop = MakeRuleBelow(rt, "RuleTop", CH);
        var ruleBot = MakeRuleAbove(rt, "RuleBottom", FH);

        var heroPlate = MakePlate(rt, "HeroPlate", new Vector2(0, 640), 220f);

        var kickerTxt = MakeMonoLabel(rt, "KickerText",   "Sistem",       new Vector2(-40, 460), new Vector2(820, 36));
        var nameTxt   = MakeItalic(rt,   "NameText",      "Organ.",  44, TextAlignmentOptions.Left, _ink,  new Vector2(-40, 380), new Vector2(820,110));
        var tagTxt    = MakeItalic(rt,   "TaglineText",   "Tagline.",16, TextAlignmentOptions.Left, _ink2, new Vector2(-40, 290), new Vector2(820, 60));

        // Facts table (editorial: Rule above, then controller populates rows)
        MakeInnerRule(rt, "RuleAboveFacts", 210f);
        var ft = MakeFactTable(rt, new Vector2(0, 100), new Vector2(900, 200));

        // Paragraph + funfact below facts
        var paraTxt  = MakeTMP(rt,   "ParagraphText", "Para.",     14, TextAlignmentOptions.Left, _ink2, new Vector2(-40,-150), new Vector2(820, 200));
        var ffLabel  = MakeMonoLabel(rt, "FunfactLabel", "Yang menarik",  new Vector2(-40,-340), new Vector2(500, 36));
        var ffTxt    = MakeItalic(rt,    "FunfactText",  "“Funfact”", 15, TextAlignmentOptions.Left, _ink, new Vector2(-40,-430), new Vector2(820,120));

        // Footer buttons side by side
        var (arBg,  arTxt)  = MakePinBtn(rt, "ARButton",   "Tempatkan di ruangan", new Vector2(-130,-905), new Vector2(500,48), _ink, _paper);
        var (qzBg,  qzTxt)  = MakePinBtn(rt, "QuizButton", "Kuis",                  new Vector2(240,-905),  new Vector2(160,48), Color.clear, _ink);

        Wire(ctrl, "backgroundImage",  bg);    Wire(ctrl, "ruleTop",       ruleTop);
        Wire(ctrl, "heroPlate",        heroPlate); Wire(ctrl, "factTable", ft);
        Wire(ctrl, "chapterText",      chapterTxt); Wire(ctrl, "kickerText", kickerTxt);
        Wire(ctrl, "nameText",         nameTxt);  Wire(ctrl, "taglineText", tagTxt);
        Wire(ctrl, "paragraphText",    paraTxt);
        Wire(ctrl, "funfactLabelText", ffLabel);  Wire(ctrl, "funfactText", ffTxt);
        Wire(ctrl, "arButtonText",     arTxt);    Wire(ctrl, "quizButtonText", qzTxt);
        WireArr(ctrl, "organDefinitions", new Object[]{ _jantung,_otak,_lambung });
        BindClick(arBg,  ctrl, "OnARPressed");
        BindClick(qzBg,  ctrl, "OnQuizPressed");
        BindClick(backBg, ctrl, "OnBackPressed");
        SaveScene(scene, "04_Detail");
    }

    // ── 05_AR ────────────────────────────────────────────────────────────────
    // Dark overlay: bg | top pills (back + plate label) | status | right rail | bottom (info + place) | bottom sheet
    static void BuildAR()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddEventSystem();
        var (rt, ctrl) = MakeScene<ARUIBridge>("Canvas");

        // Dark background
        var bgImg = MakeStretchImg(rt, "Background", new Color(0.27f,0.22f,0.18f));

        // ── Top bar overlay ──────────────────────────
        var tbGO = new GameObject("TopBar"); tbGO.transform.SetParent(rt, false);
        var tbRT = tbGO.AddComponent<RectTransform>();
        tbRT.anchorMin = new Vector2(0,1); tbRT.anchorMax = new Vector2(1,1);
        tbRT.pivot = new Vector2(0.5f,1); tbRT.anchoredPosition = Vector2.zero; tbRT.sizeDelta = new Vector2(0, 76);
        var tbImg = tbGO.AddComponent<Image>(); tbImg.color = new Color(0,0,0,0.5f);

        var backTxt = MakeTMP(tbGO.transform, "BackText",   "← KEMBALI",         10, TextAlignmentOptions.Left,   new Color(0.945f,0.925f,0.886f), new Vector2(-300, 0), new Vector2(300, 76));
        backTxt.characterSpacing = 2f;
        var aimTxt  = MakeTMP(tbGO.transform, "AimingText", "CARI PERMUKAAN DATAR", 10, TextAlignmentOptions.Center, new Color(1f,0.784f,0.353f),      new Vector2(0, 0),    new Vector2(600, 76));
        aimTxt.characterSpacing = 1.5f;

        // Back button (invisible button on back text area)
        var bkBtnGO = new GameObject("BackBtn"); bkBtnGO.transform.SetParent(tbGO.transform, false);
        var bkBtnRT = bkBtnGO.AddComponent<RectTransform>();
        bkBtnRT.anchorMin = new Vector2(0,0); bkBtnRT.anchorMax = new Vector2(0.4f,1);
        bkBtnRT.sizeDelta = Vector2.zero; bkBtnRT.anchoredPosition = Vector2.zero;
        bkBtnGO.AddComponent<Image>().color = Color.clear; bkBtnGO.AddComponent<Button>();

        // ── Right rail ──────────────────────────────
        var railGO = new GameObject("RightRail"); railGO.transform.SetParent(rt, false);
        var railRT = railGO.AddComponent<RectTransform>();
        railRT.anchorMin = railRT.anchorMax = new Vector2(1,0.5f); railRT.pivot = new Vector2(1,0.5f);
        railRT.anchoredPosition = new Vector2(-14, 0); railRT.sizeDelta = new Vector2(44, 160);
        var railImg = railGO.AddComponent<Image>(); railImg.color = new Color(0,0,0,0.5f);

        string[] railLabels = { "i", "?", "↗", "?" };
        for (int i = 0; i < 4; i++)
        {
            var rBtnGO = new GameObject($"RailBtn_{i}"); rBtnGO.transform.SetParent(railGO.transform, false);
            var rBtnRT = rBtnGO.AddComponent<RectTransform>();
            rBtnRT.anchorMin = new Vector2(0,1); rBtnRT.anchorMax = new Vector2(1,1);
            rBtnRT.pivot = new Vector2(0.5f,1); rBtnRT.anchoredPosition = new Vector2(0, -i*40f); rBtnRT.sizeDelta = new Vector2(0,40);
            var rImg = rBtnGO.AddComponent<Image>(); rImg.color = Color.clear;
            rBtnGO.AddComponent<Button>();
            var rLbl = MakeTMP(rBtnGO.transform, "Label", railLabels[i], 14, TextAlignmentOptions.Center, new Color(0.945f,0.925f,0.886f), Vector2.zero, new Vector2(44,40));

            if (i == 0) { Wire(ctrl, "infoBg",  rImg); Wire(ctrl, "infoText",  rLbl); }
            if (i == 2) { Wire(ctrl, "shareBg", rImg); Wire(ctrl, "shareText", rLbl); }
        }

        // ── Bottom action bar ────────────────────────
        var botGO = new GameObject("BottomBar"); botGO.transform.SetParent(rt, false);
        var botRT = botGO.AddComponent<RectTransform>();
        botRT.anchorMin = new Vector2(0,0); botRT.anchorMax = new Vector2(1,0);
        botRT.pivot = new Vector2(0.5f,0); botRT.anchoredPosition = new Vector2(0,14); botRT.sizeDelta = new Vector2(-28,52);

        var (plBg, plTxt) = MakePinBtn(botGO.transform, "PlaceButton", "Tempatkan",
            new Vector2(75,0), new Vector2(140,52), new Color(0.945f,0.925f,0.886f), new Color(0.1f,0.09f,0.078f));
        plBg.GetComponent<RectTransform>().anchorMin = new Vector2(0,0);
        plBg.GetComponent<RectTransform>().anchorMax = new Vector2(0.65f,1);
        plBg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        plBg.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        var infoBtnGO = new GameObject("InfoToggle"); infoBtnGO.transform.SetParent(botGO.transform, false);
        var infoBtnRT = infoBtnGO.AddComponent<RectTransform>();
        infoBtnRT.anchorMin = new Vector2(0.67f,0); infoBtnRT.anchorMax = new Vector2(1f,1);
        infoBtnRT.sizeDelta = Vector2.zero; infoBtnRT.anchoredPosition = Vector2.zero;
        var infoBtnBg = infoBtnGO.AddComponent<Image>(); infoBtnBg.color = new Color(0,0,0,0.5f);
        infoBtnGO.AddComponent<Button>();
        MakeTMP(infoBtnGO.transform, "Label", "+ Info", 11, TextAlignmentOptions.Center, new Color(0.945f,0.925f,0.886f), Vector2.zero, new Vector2(200,52)).characterSpacing = 1.5f;

        // ── Bottom sheet (collapsible) ──────────────
        var shGO = new GameObject("BottomSheet"); shGO.transform.SetParent(rt, false);
        var shRT = shGO.AddComponent<RectTransform>();
        shRT.anchorMin = new Vector2(0,0); shRT.anchorMax = new Vector2(1,0);
        shRT.pivot = new Vector2(0.5f,0); shRT.anchoredPosition = new Vector2(0,-400); shRT.sizeDelta = new Vector2(0,500);
        var shBg = shGO.AddComponent<Image>(); shBg.color = _paper;

        var hGO = new GameObject("Handle"); hGO.transform.SetParent(shGO.transform, false);
        var hRT = hGO.AddComponent<RectTransform>();
        hRT.anchorMin = hRT.anchorMax = new Vector2(0.5f,1); hRT.pivot = new Vector2(0.5f,1);
        hRT.anchoredPosition = new Vector2(0,-12); hRT.sizeDelta = new Vector2(40,4);
        var hImg = hGO.AddComponent<Image>(); hImg.color = new Color(_ink.r,_ink.g,_ink.b,0.3f);

        var oN = MakeItalic(shGO.transform, "OrganNameText",    "Organ", 28, TextAlignmentOptions.Left, _ink,  new Vector2(-40,-400), new Vector2(820,60));
        var oT = MakeItalic(shGO.transform, "OrganTaglineText", "",      15, TextAlignmentOptions.Left, _ink2, new Vector2(-40,-350), new Vector2(820,48));
        var oP = MakeTMP(shGO.transform,    "OrganParaText",    "",      13, TextAlignmentOptions.Left, _ink2, new Vector2(-40,-220), new Vector2(820,160));
        var ft = MakeFactTable(shGO.transform, new Vector2(0,-120), new Vector2(820,120));

        Wire(ctrl, "topBar",             tbImg);   Wire(ctrl, "aimingText",       aimTxt);
        Wire(ctrl, "backButtonText",     backTxt); Wire(ctrl, "placeButtonBg",    plBg);
        Wire(ctrl, "placeButtonText",    plTxt);   Wire(ctrl, "bottomSheet",      shRT);
        Wire(ctrl, "sheetHandle",        hImg);    Wire(ctrl, "sheetBackground",  shBg);
        Wire(ctrl, "organNameText",      oN);      Wire(ctrl, "organTaglineText", oT);
        Wire(ctrl, "organParagraphText", oP);      Wire(ctrl, "factTable",        ft);
        WireArr(ctrl, "organDefinitions", new Object[]{ _jantung,_otak,_lambung });
        BindClick(plBg,       ctrl, "OnPlacePressed");
        BindClick(infoBtnBg,  ctrl, "OnInfoToggled");
        BindClick(bkBtnGO.GetComponent<Image>(), ctrl, "OnBackPressed");
        SaveScene(scene, "05_AR");
    }

    // ── 06_QUIZ ───────────────────────────────────────────────────────────────
    // Layout: Chrome(back + progress) | Rule | kicker + question | Rule | options (inline) | progress bars
    static void BuildQuiz()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddEventSystem();
        var (rt, ctrl) = MakeScene<QuizController>("Canvas");

        var bg = MakeStretchImg(rt, "Background", _paper);

        var chrome  = MakeChromeBar(rt);
        var (bkBg2, _) = MakePinBtn(chrome, "BackButton", "← Kembali", new Vector2(-270,0), new Vector2(220,44), Color.clear, _ink);
        var progTxt = MakeMonoLabel(chrome, "ProgressRight", "01 / 03", new Vector2(300,0), new Vector2(200,CH));
        progTxt.alignment = TextAlignmentOptions.Right;

        var ruleTop = MakeRuleBelow(rt, "RuleTop", CH);

        // Quiz panel
        var qpGO = new GameObject("QuizPanel"); qpGO.transform.SetParent(rt, false); StretchRT(qpGO);

        var kickTxt = MakeMonoLabel(qpGO.transform, "OrganKickerText", "Kuis — Organ", new Vector2(-40, 700), new Vector2(820, 36));
        var qTxt    = MakeItalic(qpGO.transform, "QuestionText", "Pertanyaan?", 26, TextAlignmentOptions.Left, _ink, new Vector2(-40, 580), new Vector2(820, 160));
        var pTxt    = MakeMonoLabel(qpGO.transform, "ProgressText", "01 / 03", new Vector2(300, 700), new Vector2(200, 36));
        pTxt.alignment = TextAlignmentOptions.Right;

        // Rule above options
        MakeInnerRule(qpGO.transform, "RuleAboveOptions", 390f);

        // Inline options (rule-separated, no cards)
        string[] ll = {"A","B","C","D"};
        var opts = new QuizOptionUI[4];
        for (int i = 0; i < 4; i++) opts[i] = MakeQuizOption(qpGO.transform, ll[i], 310f - i*140f);

        // Progress bar strip at bottom
        var pbGO = new GameObject("ProgressBar"); pbGO.transform.SetParent(qpGO.transform, false);
        var pbRT = pbGO.AddComponent<RectTransform>();
        pbRT.anchorMin = new Vector2(0,0); pbRT.anchorMax = new Vector2(1,0);
        pbRT.pivot = new Vector2(0.5f,0); pbRT.anchoredPosition = new Vector2(0,130); pbRT.sizeDelta = new Vector2(-80,2);
        var pbImg = pbGO.AddComponent<Image>();
        pbImg.color = _ink; pbImg.type = Image.Type.Filled;
        pbImg.fillMethod = Image.FillMethod.Horizontal; pbImg.fillAmount = 0.33f;

        // Score panel
        var spGO = new GameObject("ScorePanel"); spGO.transform.SetParent(rt, false); StretchRT(spGO); spGO.SetActive(false);
        var sChrome = MakeChromeBar(spGO.transform);
        var (spBkBg, _) = MakePinBtn(sChrome, "ScoreBackBtn", "← Kembali", new Vector2(-270,0), new Vector2(220,44), Color.clear, _ink);
        var spRightMono = MakeMonoLabel(sChrome, "QuizDoneMono", "Selesai", new Vector2(300,0), new Vector2(200,CH));
        spRightMono.alignment = TextAlignmentOptions.Right;

        var sPlate  = MakePlate(spGO.transform, "ScorePlate", new Vector2(0, 400), 170f);
        var sLbl    = MakeMonoLabel(spGO.transform, "ScoreLabel", "Skor", new Vector2(0,  60), new Vector2(600,36));
        sLbl.alignment = TextAlignmentOptions.Center;
        var sTxt    = MakeItalic(spGO.transform, "ScoreText", "3/3", 64, TextAlignmentOptions.Center, _ink, new Vector2(0,-60), new Vector2(600,120));

        MakeRuleAbove(spGO.transform, "ScoreRuleBot", FH);
        var (rBg,rTxt) = MakePinBtn(spGO.transform, "RetryButton", "Ulang",   new Vector2(-130,-905), new Vector2(340,48), Color.clear, _ink);
        var (dBg,dTxt) = MakePinBtn(spGO.transform, "DoneButton",  "Selesai", new Vector2(240,-905),  new Vector2(220,48), _ink, _paper);

        Wire(ctrl, "backgroundImage", bg);       Wire(ctrl, "ruleTop",         ruleTop);
        Wire(ctrl, "quizPanel",       qpGO);     Wire(ctrl, "scorePanel",      spGO);
        Wire(ctrl, "organKickerText", kickTxt);  Wire(ctrl, "questionText",    qTxt);
        Wire(ctrl, "progressText",    pTxt);     Wire(ctrl, "progressBar",     pbImg);
        Wire(ctrl, "scorePlate",      sPlate);
        Wire(ctrl, "scoreLabelText",  sLbl);     Wire(ctrl, "scoreText",       sTxt);
        Wire(ctrl, "retryButtonText", rTxt);     Wire(ctrl, "doneButtonText",  dTxt);
        WireArr(ctrl, "options",          new Object[]{ opts[0],opts[1],opts[2],opts[3] });
        WireArr(ctrl, "organDefinitions", new Object[]{ _jantung,_otak,_lambung });
        BindClick(rBg,  ctrl, "OnRetryPressed");
        BindClick(dBg,  ctrl, "OnDonePressed");
        BindClick(bkBg2, ctrl, "OnBackPressed");
        SaveScene(scene, "06_Quiz");
    }

    // ── 07_SETTINGS ──────────────────────────────────────────────────────────
    // Layout: Chrome(back) | Rule | mono kicker + heading | Rule | rows (lang, theme, sound, haptics)
    static void BuildSettings()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddEventSystem();
        var (rt, ctrl) = MakeScene<SettingsController>("Canvas");

        var bg = MakeStretchImg(rt, "Background", _paper);

        var chrome = MakeChromeBar(rt);
        var (bkBg3, _) = MakePinBtn(chrome, "BackButton", "← Kembali", new Vector2(-270,0), new Vector2(220,44), Color.clear, _ink);

        var ruleTop = MakeRuleBelow(rt, "RuleTop", CH);

        MakeMonoLabel(rt, "AppKicker", "M'Organs — Pengaturan", new Vector2(-40, 760), new Vector2(820, 36));
        var titleTxt = MakeItalic(rt, "TitleText", "Pengaturan.", 38, TextAlignmentOptions.Left, _ink, new Vector2(-40, 680), new Vector2(820, 90));

        // Rule below heading
        MakeInnerRule(rt, "RuleTop2", 600f);

        // Rows: label (left, display font 15px) + pill group (right), each separated by a soft rule
        float rowY = 500f;
        const float ROW_H = 90f;

        var langLbl  = MakeTMP(rt, "LangLabel",    "Bahasa",   15, TextAlignmentOptions.Left, _ink, new Vector2(-40, rowY),           new Vector2(400,44));
        var (lidBg,lidTxt) = MakePill(rt, "LangID",     "ID",     new Vector2(210, rowY), true);
        var (leBg, leTxt)  = MakePill(rt, "LangEN",     "EN",     new Vector2(340, rowY), false);
        MakeInnerRule(rt, "RuleLang",  rowY - ROW_H/2f); rowY -= ROW_H;

        var themeLbl = MakeTMP(rt, "ThemeLabel",   "Tema",     15, TextAlignmentOptions.Left, _ink, new Vector2(-40, rowY),           new Vector2(400,44));
        var (tlBg,tlTxt)   = MakePill(rt, "ThemeLight", "Terang", new Vector2(200, rowY), true);
        var (tdBg,tdTxt)   = MakePill(rt, "ThemeDark",  "Gelap",  new Vector2(350, rowY), false);
        MakeInnerRule(rt, "RuleTheme", rowY - ROW_H/2f); rowY -= ROW_H;

        var soundLbl = MakeTMP(rt, "SoundLabel",   "Suara",    15, TextAlignmentOptions.Left, _ink, new Vector2(-40, rowY),           new Vector2(400,44));
        var (sBg, sTxt2)   = MakePill(rt, "Sound",      "On",     new Vector2(280, rowY), true);
        MakeInnerRule(rt, "RuleSound", rowY - ROW_H/2f); rowY -= ROW_H;

        var hapLbl   = MakeTMP(rt, "HapticsLabel", "Getaran",  15, TextAlignmentOptions.Left, _ink, new Vector2(-40, rowY),           new Vector2(400,44));
        var (hBg, hTxt)    = MakePill(rt, "Haptics",    "Off",    new Vector2(280, rowY), false);
        MakeInnerRule(rt, "RuleHaptic",rowY - ROW_H/2f);

        Wire(ctrl, "backgroundImage",      bg);       Wire(ctrl, "ruleTop",              ruleTop);
        Wire(ctrl, "titleText",            titleTxt); Wire(ctrl, "langLabelText",        langLbl);
        Wire(ctrl, "langIDBackground",     lidBg);    Wire(ctrl, "langIDText",           lidTxt);
        Wire(ctrl, "langENBackground",     leBg);     Wire(ctrl, "langENText",           leTxt);
        Wire(ctrl, "themeLabelText",       themeLbl); Wire(ctrl, "themeLightBackground", tlBg);
        Wire(ctrl, "themeLightText",       tlTxt);    Wire(ctrl, "themeDarkBackground",  tdBg);
        Wire(ctrl, "themeDarkText",        tdTxt);    Wire(ctrl, "soundLabelText",       soundLbl);
        Wire(ctrl, "soundBackground",      sBg);      Wire(ctrl, "soundText",            sTxt2);
        Wire(ctrl, "hapticsLabelText",     hapLbl);   Wire(ctrl, "hapticsBackground",    hBg);
        Wire(ctrl, "hapticsText",          hTxt);
        BindClick(lidBg, ctrl, "OnLangID");      BindClick(leBg,  ctrl, "OnLangEN");
        BindClick(tlBg,  ctrl, "OnThemeLight");  BindClick(tdBg,  ctrl, "OnThemeDark");
        BindClick(sBg,   ctrl, "OnSoundToggle"); BindClick(hBg,   ctrl, "OnHapticsToggle");
        BindClick(bkBg3, ctrl, "OnBackPressed");
        SaveScene(scene, "07_Settings");
    }

    // ── SCENE / CANVAS HELPERS ───────────────────────────────────────────────

    static (RectTransform rt, T ctrl) MakeScene<T>(string name) where T : MonoBehaviour
    {
        var go = new GameObject(name);
        var c = go.AddComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080,1920); cs.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return (go.GetComponent<RectTransform>(), go.AddComponent<T>());
    }

    static void AddEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>(); go.AddComponent<StandaloneInputModule>();
    }

    // ── LAYOUT PRIMITIVES ────────────────────────────────────────────────────

    // Full-stretch background image
    static Image MakeStretchImg(Transform p, string name, Color color)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false); StretchRT(go);
        var img = go.AddComponent<Image>(); img.color = color; return img;
    }

    // Chrome bar anchored to top (height CH)
    static RectTransform MakeChromeBar(Transform p)
    {
        var go = new GameObject("Chrome"); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(1,1);
        rt.pivot = new Vector2(0.5f,1); rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(0,CH);
        return rt;
    }

    // 1px rule anchored to top, at yOffset below top edge (positive yOffset = further from top)
    static Image MakeRuleBelow(Transform p, string name, float yOffset)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(1,1);
        rt.pivot = new Vector2(0.5f,1); rt.anchoredPosition = new Vector2(0,-yOffset); rt.sizeDelta = new Vector2(0,1);
        var img = go.AddComponent<Image>(); img.color = _ink; return img;
    }

    // 1px rule anchored to bottom, at yOffset above bottom edge
    static Image MakeRuleAbove(Transform p, string name, float yOffset)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,0); rt.anchorMax = new Vector2(1,0);
        rt.pivot = new Vector2(0.5f,0); rt.anchoredPosition = new Vector2(0,yOffset); rt.sizeDelta = new Vector2(0,1);
        var img = go.AddComponent<Image>(); img.color = _ink; return img;
    }

    // 1px rule at center-anchored absolute y position (for content section dividers)
    static Image MakeInnerRule(Transform p, string name, float yPos)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,0.5f); rt.anchorMax = new Vector2(1,0.5f);
        rt.pivot = new Vector2(0.5f,0.5f); rt.anchoredPosition = new Vector2(0,yPos); rt.sizeDelta = new Vector2(-44,1);
        var img = go.AddComponent<Image>(); img.color = new Color(_ink.r,_ink.g,_ink.b,0.85f); return img;
    }

    // Mono label: small, uppercase, letterSpacing=3
    static TMP_Text MakeMonoLabel(Transform p, string name, string text, Vector2 pos, Vector2 size)
    {
        var t = MakeTMP(p, name, text, 9, TextAlignmentOptions.Left, _inkMute, pos, size);
        t.characterSpacing = 3f;
        return t;
    }

    // Italic TMP (display heading)
    static TMP_Text MakeItalic(Transform p, string name, string text, float size,
        TextAlignmentOptions align, Color color, Vector2 pos, Vector2 sd)
    {
        var t = MakeTMP(p, name, text, size, align, color, pos, sd);
        t.fontStyle = FontStyles.Italic;
        return t;
    }

    // ── WIDGET HELPERS ───────────────────────────────────────────────────────

    static TMP_Text MakeTMP(Transform p, string name, string text, float size,
        TextAlignmentOptions align, Color color, Vector2 pos, Vector2 sd)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.pivot = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = sd;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.alignment = align; tmp.color = color;
        return tmp;
    }

    // Fixed-size pin button (center-anchor, no stretch)
    static (Image bg, TMP_Text txt) MakePinBtn(Transform p, string name, string label,
        Vector2 pos, Vector2 size, Color bgColor, Color txtColor)
    {
        var go = new GameObject(name); go.transform.SetParent(p,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.pivot = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = bgColor; go.AddComponent<Button>();
        var tGO = new GameObject("Text"); tGO.transform.SetParent(go.transform,false); StretchRT(tGO);
        var tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text=label; tmp.fontSize=11; tmp.alignment=TextAlignmentOptions.Center;
        tmp.characterSpacing=1.5f; tmp.color=txtColor;
        return (img,tmp);
    }

    // Full-width stretch button (used for main action buttons on detail/quiz)
    static (Image bg, TMP_Text txt) MakeBtn(Transform p, string name, string label,
        Vector2 pos, Vector2 size, Color bgColor, Color txtColor)
    {
        var go = new GameObject(name); go.transform.SetParent(p,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin=new Vector2(0,.5f); rt.anchorMax=new Vector2(1,.5f);
        rt.pivot=new Vector2(.5f,.5f); rt.anchoredPosition=pos; rt.sizeDelta=size;
        var img = go.AddComponent<Image>(); img.color=bgColor; go.AddComponent<Button>();
        var tGO = new GameObject("Text"); tGO.transform.SetParent(go.transform,false); StretchRT(tGO);
        var tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text=label; tmp.fontSize=15; tmp.alignment=TextAlignmentOptions.Center; tmp.color=txtColor;
        return (img,tmp);
    }

    // Square pill button (settings toggles)
    static (Image bg, TMP_Text txt) MakePill(Transform p, string name, string label, Vector2 pos, bool active)
    {
        var go = new GameObject(name); go.transform.SetParent(p,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin=rt.anchorMax=new Vector2(0.5f,0.5f); rt.pivot=new Vector2(0.5f,0.5f);
        rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(120,44);
        var img = go.AddComponent<Image>(); img.color=active?_ink:Color.clear; go.AddComponent<Button>();
        var tGO = new GameObject("Text"); tGO.transform.SetParent(go.transform,false); StretchRT(tGO);
        var tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text=label; tmp.fontSize=11; tmp.alignment=TextAlignmentOptions.Center;
        tmp.characterSpacing=1.5f; tmp.color=active?_paper:_ink;
        return (img,tmp);
    }

    static OrganPlateUI MakePlate(Transform p, string name, Vector2 pos, float size)
    {
        var root = new GameObject(name); root.transform.SetParent(p, false);
        var rt = root.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.pivot = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(size,size);
        var bg    = root.AddComponent<Image>(); bg.color = _paperDeep;
        var plate = root.AddComponent<OrganPlateUI>();
        var hGO = new GameObject("Hatch");    hGO.transform.SetParent(root.transform,false); StretchRT(hGO); var hatch    = hGO.AddComponent<HatchGraphic>();
        var bGO = new GameObject("Brackets"); bGO.transform.SetParent(root.transform,false); StretchRT(bGO); bGO.AddComponent<CornerBracketsGraphic>();
        var iGO = new GameObject("Icon");     iGO.transform.SetParent(root.transform,false);
        var iRT = iGO.AddComponent<RectTransform>();
        iRT.anchorMin = iRT.anchorMax = new Vector2(0.5f,0.5f); iRT.pivot = new Vector2(0.5f,0.5f);
        iRT.anchoredPosition = Vector2.zero; iRT.sizeDelta = new Vector2(size*.55f,size*.55f);
        var icon = iGO.AddComponent<Image>(); icon.color = _ink; icon.preserveAspect = true;
        var lGO = new GameObject("Label"); lGO.transform.SetParent(root.transform,false);
        var lRT = lGO.AddComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0,0); lRT.anchorMax = new Vector2(1,0);
        lRT.pivot = new Vector2(0.5f,0); lRT.anchoredPosition = new Vector2(0,8); lRT.sizeDelta = new Vector2(0,20);
        var lbl = lGO.AddComponent<TextMeshProUGUI>(); lbl.fontSize=9; lbl.alignment=TextAlignmentOptions.Center; lbl.color=_inkMute;
        Wire(plate,"background",bg); Wire(plate,"hatch",hatch);
        Wire(plate,"iconImage",icon); Wire(plate,"labelText",lbl);
        return plate;
    }

    static OrganListItemUI MakeListItem(Transform p, string name, float yPos)
    {
        var root = new GameObject(name); root.transform.SetParent(p,false);
        var rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,.5f); rt.anchorMax = new Vector2(1,.5f);
        rt.pivot = new Vector2(.5f,.5f); rt.anchoredPosition = new Vector2(0,yPos); rt.sizeDelta = new Vector2(-44,130);
        root.AddComponent<Button>(); var item = root.AddComponent<OrganListItemUI>();
        var thumb  = MakePlate(root.transform,"Thumbnail",new Vector2(-390,0),88f);
        var numTxt = MakeMonoLabel(root.transform,"NumberKicker","01",new Vector2(80,35),new Vector2(500,28));
        var nmTxt  = MakeItalic(root.transform,"NameText","–",    26, TextAlignmentOptions.Left,_ink, new Vector2(80,0),   new Vector2(500,48));
        var tgTxt  = MakeTMP(root.transform,"TaglineText","–",    12, TextAlignmentOptions.Left,_inkMute, new Vector2(80,-32),new Vector2(500,36));
        var arrow  = MakeTMP(root.transform,"ArrowText","→",      18, TextAlignmentOptions.Right,_ink,   new Vector2(420,0), new Vector2(60,40));
        var dGO = new GameObject("Divider"); dGO.transform.SetParent(root.transform,false);
        var dRT = dGO.AddComponent<RectTransform>();
        dRT.anchorMin=new Vector2(0,0); dRT.anchorMax=new Vector2(1,0); dRT.pivot=new Vector2(.5f,0);
        dRT.anchoredPosition=Vector2.zero; dRT.sizeDelta=new Vector2(0,1);
        var div = dGO.AddComponent<Image>(); div.color=new Color(_ink.r,_ink.g,_ink.b,.2f);
        Wire(item,"thumbnail",thumb); Wire(item,"numberKickerText",numTxt);
        Wire(item,"nameText",nmTxt);  Wire(item,"taglineText",tgTxt);
        Wire(item,"arrowText",arrow); Wire(item,"divider",div);
        return item;
    }

    static QuizOptionUI MakeQuizOption(Transform p, string letter, float yPos)
    {
        var root = new GameObject($"Option_{letter}"); root.transform.SetParent(p,false);
        var rt = root.AddComponent<RectTransform>();
        rt.anchorMin=new Vector2(0,.5f); rt.anchorMax=new Vector2(1,.5f);
        rt.pivot=new Vector2(.5f,.5f); rt.anchoredPosition=new Vector2(0,yPos); rt.sizeDelta=new Vector2(-44,110);
        var btn = root.AddComponent<Button>(); var opt = root.AddComponent<QuizOptionUI>();
        var lTxt = MakeMonoLabel(root.transform,"LetterText",letter,new Vector2(-400,0),new Vector2(40,40));
        lTxt.alignment = TextAlignmentOptions.Center;
        var aTxt = MakeTMP(root.transform,"AnswerText","–",16,TextAlignmentOptions.Left,_ink, new Vector2(0,0),  new Vector2(680,60));
        var rTxt = MakeTMP(root.transform,"ResultIcon","", 18,TextAlignmentOptions.Right,_ink,new Vector2(400,0),new Vector2(40,40));
        var dGO  = new GameObject("Divider"); dGO.transform.SetParent(root.transform,false);
        var dRT  = dGO.AddComponent<RectTransform>();
        dRT.anchorMin=new Vector2(0,0); dRT.anchorMax=new Vector2(1,0); dRT.pivot=new Vector2(.5f,0);
        dRT.anchoredPosition=Vector2.zero; dRT.sizeDelta=new Vector2(0,1);
        var div = dGO.AddComponent<Image>(); div.color=new Color(_ink.r,_ink.g,_ink.b,.2f);
        Wire(opt,"letterText",lTxt); Wire(opt,"answerText",aTxt);
        Wire(opt,"resultIcon",rTxt); Wire(opt,"divider",div); Wire(opt,"button",btn);
        return opt;
    }

    static FactTableUI MakeFactTable(Transform p, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("FactTable"); go.transform.SetParent(p,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f); rt.pivot=new Vector2(.5f,.5f);
        rt.anchoredPosition=pos; rt.sizeDelta=size;
        var ft = go.AddComponent<FactTableUI>();
        var cGO = new GameObject("Container"); cGO.transform.SetParent(go.transform,false); StretchRT(cGO);
        Wire(ft,"container",cGO.transform);
        return ft;
    }

    // ── UTILITY ──────────────────────────────────────────────────────────────

    static void StretchRT(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.sizeDelta=Vector2.zero; rt.anchoredPosition=Vector2.zero;
    }

    static void BindClick(Image btnImg, MonoBehaviour target, string method)
    {
        var btn = btnImg.GetComponent<Button>();
        if (btn == null) btn = btnImg.gameObject.AddComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            btn.onClick,
            (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), target, method));
    }

    static void Wire(Object target, string field, Object value)
    {
        if (target == null || value == null) return;
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field);
        if (prop != null) { prop.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"[MOrgans] Missing field: {field} on {target.GetType().Name}");
    }

    static void WireArr(Object target, string field, Object[] values)
    {
        if (target == null) return;
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field);
        if (prop == null) { Debug.LogWarning($"[MOrgans] Missing array: {field}"); return; }
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            if (values[i] != null) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedProperties();
    }

    static void SaveScene(Scene scene, string name)
    {
        EditorSceneManager.SaveScene(scene, $"{ScenePath}/{name}.unity");
        Debug.Log($"[MOrgans] Saved: {name}");
    }

    static void UpdateBuildSettings()
    {
        string[] names = { "_Persistent","01_Splash","02_Onboarding","03_Picker","04_Detail","05_AR","06_Quiz","07_Settings" };
        var list = new List<EditorBuildSettingsScene>();
        foreach (var n in names)
        {
            string p = $"{ScenePath}/{n}.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(p) != null)
                list.Add(new EditorBuildSettingsScene(p, true));
        }
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"[MOrgans] Build Settings: {list.Count} scenes registered.");
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\','/');
        string folder = Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
