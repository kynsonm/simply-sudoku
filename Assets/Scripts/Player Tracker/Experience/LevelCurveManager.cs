using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class LevelCurveManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelXP
    {
        public int level;       // To this level
        public int xp;          // xp Needed
        public float exponent;  // Exponent modifier

        // If you have everything you need
        public LevelXP(int levelIn, int xpIn, float exponentIn) {
            level = levelIn;
            xp = xpIn;
            exponent = exponentIn;
        }

        // If you have nothing
        public LevelXP(int levelIn) {
            level = levelIn;
            xp = -1;
            exponent = -1f;
        }
    }

    // Useful stuff
    [SerializeField] bool fillXPs;
    [SerializeField] bool clearXPs;
    [SerializeField] int numLevels;
    [SerializeField] int maxLevelXP;

    // How much xp is needed to get to the next level
    public List<LevelXP> XPperLevel;


    // Fill XPperLevel depending on stufffff
    void FillXPs() {
        // Just fill it with numbers if no XPperLevel yet (just for me)
        if (XPperLevel == null || XPperLevel.Count == 0) {
            XPperLevel = new List<LevelXP>(numLevels);
            for (int i = 0; i < numLevels; ++i) {
                XPperLevel.Add(new LevelXP(i+1));
            }
            return;
        }

        int fxnStartIndex = 0;
        float lastExp = 2f;

        for (int i = 0; i < XPperLevel.Count; ++i) {
            LevelXP lvl = XPperLevel[i];
            // Exponent of interest
            if (lvl.exponent != -1) {
                lastExp = lvl.exponent;
            }
            // Function start
            if (lvl.xp != -1 && lvl.xp != 0) {
                fxnStartIndex = i;
                continue;
            }

            // Calculation
            int offset = XPperLevel[fxnStartIndex].xp;
            int diff = maxLevelXP - offset;
            float ratio = (float)(i+1) / (float)XPperLevel.Count;

            float xp = (float)offset + (float)diff * (float)(Mathf.Pow(ratio, lastExp));
            xp /= 10;
            xp = Mathf.RoundToInt(xp);
            xp *= 10;

            lvl.xp = (int)xp;
        }

        if (XPperLevel[0].level != 0) {
            XPperLevel.Insert(0, new LevelXP(0, 0, -1));
        }

        float sum = 0f;
        for (int i = 0; i < XPperLevel.Count; ++i) {
            sum += XPperLevel[i].xp;
        }
    }


    // Start is called before the first frame update
    void Awake()
    {
        FillXPs();
        UpdateLevelCurveManager();
    }

    // Update is called once per frame
    void UpdateLevelCurveManager()
    {
        StartCoroutine(UpdateLevelCurveManagerEnum());
    }
    IEnumerator UpdateLevelCurveManagerEnum() {
        while (true) {
            if (clearXPs) {
                List<LevelXP> list = new List<LevelXP>();
                for (int i = 0; i < numLevels; ++i) {
                    list.Add(new LevelXP(i+1));
                    if (i < 15) {
                        list[i].xp = XPperLevel[i].xp;
                    }
                    list[i].exponent = XPperLevel[i].exponent;
                }

                XPperLevel = list;
                clearXPs = false;
            }
            // Do the thing
            if (fillXPs) {
                FillXPs();
                fillXPs = false;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}
