using System;
using System.Collections.Generic;
using UnityEngine;

[UnityEngine.DefaultExecutionOrder(-200)]
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    public static event Action OnLanguageChanged;

    static readonly Dictionary<string, Dictionary<string, string>> _table =
        new Dictionary<string, Dictionary<string, string>>
    {
        ["appName"]     = new Dictionary<string,string> { ["id"]="M'Organs",                                        ["en"]="M'Organs" },
        ["tagline"]     = new Dictionary<string,string> { ["id"]="Anatomi dalam genggaman",                          ["en"]="Anatomy at hand" },
        ["sub"]         = new Dictionary<string,string> { ["id"]="Lihat organ tubuh manusia di mejamu, lewat AR.",   ["en"]="See human organs at full size in your room, through AR." },
        ["start"]       = new Dictionary<string,string> { ["id"]="Mulai",                                            ["en"]="Begin" },
        ["skip"]        = new Dictionary<string,string> { ["id"]="Lewati",                                           ["en"]="Skip" },
        ["next"]        = new Dictionary<string,string> { ["id"]="Lanjut",                                           ["en"]="Next" },
        ["continue"]    = new Dictionary<string,string> { ["id"]="Lanjut",                                           ["en"]="Continue" },
        ["pickerTitle"] = new Dictionary<string,string> { ["id"]="Pilih organ.",                                     ["en"]="Pick an organ." },
        ["pickerSub"]   = new Dictionary<string,string> { ["id"]="Tiga organ tersedia",                            ["en"]="Three available" },
        ["soon"]        = new Dictionary<string,string> { ["id"]="Segera",                                           ["en"]="Soon" },
        ["placeAR"]     = new Dictionary<string,string> { ["id"]="Tempatkan di ruangan",                             ["en"]="Place in room" },
        ["arHint"]      = new Dictionary<string,string> { ["id"]="Gerakkan kamera ke lantai",                        ["en"]="Aim the camera at the floor" },
        ["quiz"]        = new Dictionary<string,string> { ["id"]="Kuis",                                             ["en"]="Quiz" },
        ["backLabel"]   = new Dictionary<string,string> { ["id"]="Kembali",                                          ["en"]="Back" },
        ["backOrgans"]  = new Dictionary<string,string> { ["id"]="Kembali ke daftar organ",                          ["en"]="Back to organs" },
        ["funfact"]     = new Dictionary<string,string> { ["id"]="Yang menarik",                                     ["en"]="Of note" },
        ["facts"]       = new Dictionary<string,string> { ["id"]="Catatan",                                          ["en"]="Notes" },
        ["chapter"]     = new Dictionary<string,string> { ["id"]="Bab",                                              ["en"]="Chapter" },
        ["score"]       = new Dictionary<string,string> { ["id"]="Skor",                                             ["en"]="Score" },
        ["retry"]       = new Dictionary<string,string> { ["id"]="Ulang",                                            ["en"]="Retry" },
        ["done"]        = new Dictionary<string,string> { ["id"]="Selesai",                                          ["en"]="Close" },
        ["quizDone"]    = new Dictionary<string,string> { ["id"]="Selesai",                                          ["en"]="Done" },
        ["settings"]    = new Dictionary<string,string> { ["id"]="Pengaturan",                                       ["en"]="Settings" },
        ["language"]    = new Dictionary<string,string> { ["id"]="Bahasa",                                           ["en"]="Language" },
        ["theme"]       = new Dictionary<string,string> { ["id"]="Tema",                                             ["en"]="Theme" },
        ["light"]       = new Dictionary<string,string> { ["id"]="Terang",                                           ["en"]="Light" },
        ["dark"]        = new Dictionary<string,string> { ["id"]="Gelap",                                            ["en"]="Dark" },
        ["mint"]        = new Dictionary<string,string> { ["id"]="Mint",                                             ["en"]="Mint" },
        ["sound"]       = new Dictionary<string,string> { ["id"]="Suara",                                            ["en"]="Sound" },
        ["haptics"]     = new Dictionary<string,string> { ["id"]="Getaran",                                          ["en"]="Haptics" },
        ["about"]       = new Dictionary<string,string> { ["id"]="Tentang",                                          ["en"]="About" },
        ["info"]        = new Dictionary<string,string> { ["id"]="Catatan",                                          ["en"]="Notes" },
        ["share"]       = new Dictionary<string,string> { ["id"]="Bagikan",                                          ["en"]="Share" },
        ["help"]        = new Dictionary<string,string> { ["id"]="Bantuan",                                          ["en"]="Help" },
        ["place"]       = new Dictionary<string,string> { ["id"]="Tempatkan",                                        ["en"]="Place" },
        ["aiming"]      = new Dictionary<string,string> { ["id"]="Cari permukaan datar",                             ["en"]="Find a flat surface" },
        ["versionTag"]  = new Dictionary<string,string> { ["id"]="v1.0",                                             ["en"]="v1.0" },
        ["on"]          = new Dictionary<string,string> { ["id"]="On",                                               ["en"]="On" },
        ["off"]         = new Dictionary<string,string> { ["id"]="Off",                                              ["en"]="Off" },
        ["soonHead"]    = new Dictionary<string,string> { ["id"]="Belum tersedia",                                   ["en"]="Not yet" },
        ["soonParu"]    = new Dictionary<string,string> { ["id"]="Paru-paru",                                       ["en"]="Lungs" },
        ["soonGinjal"]  = new Dictionary<string,string> { ["id"]="Ginjal",                                          ["en"]="Kidneys" },
        ["soonHati"]    = new Dictionary<string,string> { ["id"]="Hati",                                            ["en"]="Liver" },
        ["soonNote"]    = new Dictionary<string,string> { ["id"]="Model 3D belum dibuat",                           ["en"]="No 3D model" },
        ["explore"]     = new Dictionary<string,string> { ["id"]="Jelajahi",                                         ["en"]="Explore" },
        ["onb0_kicker"] = new Dictionary<string,string> { ["id"]="01",                                               ["en"]="01" },
        ["onb0_title"]  = new Dictionary<string,string> { ["id"]="Apa ini",                                          ["en"]="What this is" },
        ["onb0_body"]   = new Dictionary<string,string> { ["id"]="M’Organs menampilkan organ tubuh ukuran asli di ruanganmu, lewat kamera HP.", ["en"]="M’Organs places life-size organs in your room using your phone camera." },
        ["onb1_kicker"] = new Dictionary<string,string> { ["id"]="02",                                               ["en"]="02" },
        ["onb1_title"]  = new Dictionary<string,string> { ["id"]="Cara kerja",                                       ["en"]="How it works" },
        ["onb1_body"]   = new Dictionary<string,string> { ["id"]="Arahkan kamera ke lantai. Saat permukaan terbaca, titik kuning muncul.",  ["en"]="Aim the camera at the floor. A yellow dot appears when a surface is found." },
        ["onb2_kicker"] = new Dictionary<string,string> { ["id"]="03",                                               ["en"]="03" },
        ["onb2_title"]  = new Dictionary<string,string> { ["id"]="Kontrol",                                          ["en"]="Controls" },
        ["onb2_body"]   = new Dictionary<string,string> { ["id"]="Tap untuk menempatkan, dua jari untuk memutar, cubit untuk perbesar.",    ["en"]="Tap to place, two fingers to rotate, pinch to scale." },
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public string Get(string key)
    {
        string lang = AppState.Instance ? AppState.Instance.Language : "id";
        if (_table.TryGetValue(key, out var row) && row.TryGetValue(lang, out var val))
            return val;
        Debug.LogWarning($"[Loc] Missing key: '{key}' lang='{lang}'");
        return key;
    }

    public void SetLanguage(string lang)
    {
        AppState.Instance?.SetLanguage(lang);
        OnLanguageChanged?.Invoke();
    }
}
