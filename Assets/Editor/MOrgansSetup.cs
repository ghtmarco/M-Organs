using UnityEditor;
using UnityEngine;

/// <summary>
/// Batch-mode asset generator for M'Organs.
/// RED:   Unity.exe -batchmode -executeMethod MOrgansSetup.VerifyAssets  -projectPath "..." -quit
/// GREEN: Unity.exe -batchmode -executeMethod MOrgansSetup.CreateAllAssets -projectPath "..." -quit
/// </summary>
public static class MOrgansSetup
{
    const string DataPath = "Assets/_Project/Data";

    // ── VERIFY (RED check) ────────────────────────────────────────────────
    public static void VerifyAssets()
    {
        string[] expected = {
            $"{DataPath}/MO2Theme_Light.asset",
            $"{DataPath}/MO2Theme_Dark.asset",
            $"{DataPath}/Organ_Jantung.asset",
            $"{DataPath}/Organ_Otak.asset",
            $"{DataPath}/Organ_Lambung.asset",
        };

        bool allPass = true;
        foreach (var path in expected)
        {
            bool exists = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null;
            Debug.Log($"[MOrgans] {(exists ? "PASS" : "FAIL")} {path}");
            if (!exists) allPass = false;
        }
        Debug.Log(allPass ? "[MOrgans] ALL PASS" : "[MOrgans] SOME FAILED — run CreateAllAssets");
    }

    // ── CREATE (GREEN) ────────────────────────────────────────────────────
    public static void CreateAllAssets()
    {
        EnsureFolder(DataPath);

        CreateThemes();
        CreateOrganJantung();
        CreateOrganOtak();
        CreateOrganLambung();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MOrgans] CreateAllAssets complete.");
        VerifyAssets();
    }

    // ── THEMES ────────────────────────────────────────────────────────────
    static void CreateThemes()
    {
        var light = GetOrCreate<MO2ThemeData>($"{DataPath}/MO2Theme_Light.asset");
        light.paper     = Hex("#F1ECE2");
        light.paperDeep = Hex("#E8E1D2");
        light.surface   = Hex("#FBF7EE");
        light.ink       = Hex("#1A1714");
        light.ink2      = Hex("#3D3631");
        light.inkMute   = Hex("#7A6F64");
        light.inkFaint  = Hex("#B5A99B");
        light.rule      = Hex("#1A1714");
        light.ruleSoft  = new Color(0.102f, 0.090f, 0.078f, 0.12f);
        light.accent    = Hex("#C2410C");
        light.ok        = Hex("#3F6B3A");
        light.isDark    = false;
        EditorUtility.SetDirty(light);

        var dark = GetOrCreate<MO2ThemeData>($"{DataPath}/MO2Theme_Dark.asset");
        dark.paper     = Hex("#14110D");
        dark.paperDeep = Hex("#1B1813");
        dark.surface   = Hex("#1F1B16");
        dark.ink       = Hex("#F1ECE2");
        dark.ink2      = Hex("#D6CDBE");
        dark.inkMute   = Hex("#9A8E7E");
        dark.inkFaint  = Hex("#5A5046");
        dark.rule      = Hex("#F1ECE2");
        dark.ruleSoft  = new Color(0.945f, 0.925f, 0.886f, 0.14f);
        dark.accent    = Hex("#E07A4D");
        dark.ok        = Hex("#7DB07A");
        dark.isDark    = true;
        EditorUtility.SetDirty(dark);
    }

    // ── JANTUNG ───────────────────────────────────────────────────────────
    static void CreateOrganJantung()
    {
        var o = GetOrCreate<OrganDefinition>($"{DataPath}/Organ_Jantung.asset");
        o.organKey  = "jantung";
        o.number    = "01";
        o.latinName = "Cor";
        o.organTone = Hex("#8B1E2D");
        o.nameID    = "Jantung";
        o.nameEN    = "Heart";

        o.kickerID    = "Sistem peredaran";
        o.taglineID   = "Otot yang tidak pernah istirahat.";
        o.paragraphID = "Empat ruang. Dua atrium di atas, dua ventrikel di bawah. Setiap detak mendorong kira-kira 70 ml darah keluar dari ventrikel kiri.";
        o.funfactID   = "Seumur hidup, jantungmu memompa cukup darah untuk mengisi tiga kapal tanker.";

        o.kickerEN    = "Circulatory";
        o.taglineEN   = "A muscle that never rests.";
        o.paragraphEN = "Four chambers. Two atria up top, two ventricles below. Each beat pushes about 70 ml out of the left ventricle.";
        o.funfactEN   = "Across a lifetime, your heart pumps enough blood to fill three oil tankers.";

        o.facts = new OrganDefinition.FactEntry[]
        {
            new OrganDefinition.FactEntry { labelID = "Detak/hari",  labelEN = "Beats/day",  value = "≈100.000" },
            new OrganDefinition.FactEntry { labelID = "Berat",       labelEN = "Weight",     value = "≈300 g"   },
            new OrganDefinition.FactEntry { labelID = "Darah/menit", labelEN = "Blood/min",  value = "5 L"           },
        };

        o.quiz = new OrganDefinition.QuizQuestion[]
        {
            new OrganDefinition.QuizQuestion {
                questionID   = "Berapa ruang di dalam jantung?",
                questionEN   = "How many chambers in the heart?",
                answersID    = new[]{ "2", "4", "6", "8" },
                answersEN    = new[]{ "2", "4", "6", "8" },
                correctIndex = 1,
            },
            new OrganDefinition.QuizQuestion {
                questionID   = "Ruang yang memompa darah ke seluruh tubuh:",
                questionEN   = "Which chamber feeds the whole body?",
                answersID    = new[]{ "Atrium kanan", "Ventrikel kiri", "Atrium kiri", "Ventrikel kanan" },
                answersEN    = new[]{ "Right atrium", "Left ventricle", "Left atrium", "Right ventricle" },
                correctIndex = 1,
            },
            new OrganDefinition.QuizQuestion {
                questionID   = "Volume darah yang dipompa per menit:",
                questionEN   = "Liters of blood per minute:",
                answersID    = new[]{ "1 L", "5 L", "15 L", "20 L" },
                answersEN    = new[]{ "1 L", "5 L", "15 L", "20 L" },
                correctIndex = 1,
            },
        };
        EditorUtility.SetDirty(o);
    }

    // ── OTAK ──────────────────────────────────────────────────────────────
    static void CreateOrganOtak()
    {
        var o = GetOrCreate<OrganDefinition>($"{DataPath}/Organ_Otak.asset");
        o.organKey  = "otak";
        o.number    = "02";
        o.latinName = "Cerebrum";
        o.organTone = Hex("#3F2A6E");
        o.nameID    = "Otak";
        o.nameEN    = "Brain";

        o.kickerID    = "Sistem saraf";
        o.taglineID   = "Lipatan-lipatan untuk muat lebih banyak.";
        o.paragraphID = "Permukaan otak berlipat agar luasnya cukup. Bagian belakang, cerebellum, mengatur keseimbangan. Bagian depan untuk berpikir dan mengambil keputusan.";
        o.funfactID   = "Otak menggunakan energi sekitar 20 watt — sebanding dengan bohlam kecil.";

        o.kickerEN    = "Nervous system";
        o.taglineEN   = "Folded to fit more in.";
        o.paragraphEN = "The brain's surface folds so its area can be larger. The cerebellum at the back handles balance. The front handles thinking and choices.";
        o.funfactEN   = "Your brain runs on about 20 watts — the same as a small bulb.";

        o.facts = new OrganDefinition.FactEntry[]
        {
            new OrganDefinition.FactEntry { labelID = "Neuron", labelEN = "Neurons", value = "86 miliar"  },
            new OrganDefinition.FactEntry { labelID = "Berat",  labelEN = "Weight",  value = "≈1,4 kg" },
            new OrganDefinition.FactEntry { labelID = "Energi", labelEN = "Energy",  value = "20% tubuh"  },
        };

        o.quiz = new OrganDefinition.QuizQuestion[]
        {
            new OrganDefinition.QuizQuestion {
                questionID   = "Perkiraan jumlah neuron di otak:",
                questionEN   = "Approximate neuron count:",
                answersID    = new[]{ "1 juta", "86 miliar", "1 triliun", "500 ribu" },
                answersEN    = new[]{ "1 million", "86 billion", "1 trillion", "500,000" },
                correctIndex = 1,
            },
            new OrganDefinition.QuizQuestion {
                questionID   = "Bagian otak yang mengatur keseimbangan:",
                questionEN   = "Which part handles balance:",
                answersID    = new[]{ "Cerebrum", "Cerebellum", "Brainstem", "Hippocampus" },
                answersEN    = new[]{ "Cerebrum", "Cerebellum", "Brainstem", "Hippocampus" },
                correctIndex = 1,
            },
            new OrganDefinition.QuizQuestion {
                questionID   = "Persentase energi tubuh yang dipakai otak:",
                questionEN   = "Share of body's energy used:",
                answersID    = new[]{ "2%", "5%", "20%", "50%" },
                answersEN    = new[]{ "2%", "5%", "20%", "50%" },
                correctIndex = 2,
            },
        };
        EditorUtility.SetDirty(o);
    }

    // ── LAMBUNG ───────────────────────────────────────────────────────────
    static void CreateOrganLambung()
    {
        var o = GetOrCreate<OrganDefinition>($"{DataPath}/Organ_Lambung.asset");
        o.organKey  = "lambung";
        o.number    = "03";
        o.latinName = "Gaster";
        o.organTone = Hex("#7A4108");
        o.nameID    = "Lambung";
        o.nameEN    = "Stomach";

        o.kickerID    = "Sistem pencernaan";
        o.taglineID   = "Kantong asam yang melindungi dirinya sendiri.";
        o.paragraphID = "Lambung memecah makanan dengan asam klorida dan enzim. Lapisan lendir di dalam mencegahnya tercerna sendiri.";
        o.funfactID   = "Asamnya cukup kuat untuk melarutkan logam tipis — tapi lapisan lendir tidak ikut tercerna.";

        o.kickerEN    = "Digestive";
        o.taglineEN   = "An acid bag that protects itself.";
        o.paragraphEN = "The stomach breaks food down with hydrochloric acid and enzymes. A mucus lining keeps it from digesting itself.";
        o.funfactEN   = "The acid can dissolve thin metal — yet the mucus lining stays intact.";

        o.facts = new OrganDefinition.FactEntry[]
        {
            new OrganDefinition.FactEntry { labelID = "Kapasitas", labelEN = "Capacity", value = "1–1,5 L" },
            new OrganDefinition.FactEntry { labelID = "Cerna",     labelEN = "Digest",   value = "4–5 jam" },
            new OrganDefinition.FactEntry { labelID = "pH",        labelEN = "pH",       value = "1,5–3,5" },
        };

        o.quiz = new OrganDefinition.QuizQuestion[]
        {
            new OrganDefinition.QuizQuestion {
                questionID   = "Asam yang dihasilkan lambung:",
                questionEN   = "Acid produced by the stomach:",
                answersID    = new[]{ "Sulfat", "Klorida", "Asetat", "Sitrat" },
                answersEN    = new[]{ "Sulfuric", "Hydrochloric", "Acetic", "Citric" },
                correctIndex = 1,
            },
            new OrganDefinition.QuizQuestion {
                questionID   = "Waktu rata-rata mencerna makanan:",
                questionEN   = "Average digestion time:",
                answersID    = new[]{ "1 jam", "2–3 jam", "4–5 jam", "12 jam" },
                answersEN    = new[]{ "1 hr", "2–3 hr", "4–5 hr", "12 hr" },
                correctIndex = 2,
            },
            new OrganDefinition.QuizQuestion {
                questionID   = "Kapasitas lambung dewasa:",
                questionEN   = "Adult stomach capacity:",
                answersID    = new[]{ "0,2 L", "0,5 L", "1–1,5 L", "5 L" },
                answersEN    = new[]{ "0.2 L", "0.5 L", "1–1.5 L", "5 L" },
                correctIndex = 2,
            },
        };
        EditorUtility.SetDirty(o);
    }

    // ── HELPERS ───────────────────────────────────────────────────────────
    static T GetOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string folder = System.IO.Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
