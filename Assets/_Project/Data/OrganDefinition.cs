using UnityEngine;

[CreateAssetMenu(fileName = "Organ_", menuName = "MOrgans/OrganDefinition")]
public class OrganDefinition : ScriptableObject
{
    [System.Serializable]
    public struct FactEntry
    {
        public string labelID;
        public string labelEN;
        public string value;
    }

    [System.Serializable]
    public struct QuizQuestion
    {
        [TextArea(1, 3)] public string questionID;
        [TextArea(1, 3)] public string questionEN;
        [Tooltip("4 answer options in Indonesian")]
        public string[] answersID;
        [Tooltip("4 answer options in English")]
        public string[] answersEN;
        [Tooltip("0-based index of correct answer")]
        public int correctIndex;
    }

    [Header("Identity")]
    public string organKey;          // "jantung" | "otak" | "lambung"
    public string number;            // "01" | "02" | "03"
    public string latinName;         // "Cor" | "Cerebrum" | "Gaster"
    public Color  organTone;         // legacy — prefer paletteHue

    [Header("Palette")]
    public Color paletteHue;         // vivid accent (#FF5C6E jantung)
    public Color paletteSoft;        // light tint for backgrounds (#FFD8DC)
    public Color paletteDeep;        // dark text on palette (#8A1424)

    [Header("Names")]
    public string nameID;
    public string nameEN;

    [Header("Content (ID)")]
    public string kickerID;
    [TextArea(1, 2)] public string taglineID;
    [TextArea(2, 4)] public string paragraphID;
    [TextArea(1, 3)] public string funfactID;

    [Header("Content (EN)")]
    public string kickerEN;
    [TextArea(1, 2)] public string taglineEN;
    [TextArea(2, 4)] public string paragraphEN;
    [TextArea(1, 3)] public string funfactEN;

    [Header("Data")]
    public FactEntry[]    facts;
    public QuizQuestion[] quiz;

    [Header("Assets")]
    [Tooltip("PNG sprite of the organ line-art icon (export from organs.jsx SVGs)")]
    public Sprite     icon;
    [Tooltip("3D organ prefab from AnatomicPack")]
    public GameObject prefab3D;

    public string GetName(string lang)      => lang == "id" ? nameID      : nameEN;
    public string GetKicker(string lang)    => lang == "id" ? kickerID    : kickerEN;
    public string GetTagline(string lang)   => lang == "id" ? taglineID   : taglineEN;
    public string GetParagraph(string lang) => lang == "id" ? paragraphID : paragraphEN;
    public string GetFunfact(string lang)   => lang == "id" ? funfactID   : funfactEN;

    public string GetPlateLabel(string lang)
    {
        string label = string.IsNullOrEmpty(latinName) ? GetName(lang) : latinName;
        return $"Pl. {number} — {label}";
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (quiz == null) return;
        for (int i = 0; i < quiz.Length; i++)
        {
            var q = quiz[i];
            int maxAnswers = Mathf.Min(
                q.answersID != null ? q.answersID.Length : 0,
                q.answersEN != null ? q.answersEN.Length : 0);
            if (maxAnswers > 0 && q.correctIndex >= maxAnswers)
            {
                Debug.LogError($"[{name}] quiz[{i}].correctIndex={q.correctIndex} out of bounds (answers={maxAnswers}), clamping.");
                q.correctIndex = maxAnswers - 1;
                quiz[i] = q;
            }
        }
    }
#endif
}
