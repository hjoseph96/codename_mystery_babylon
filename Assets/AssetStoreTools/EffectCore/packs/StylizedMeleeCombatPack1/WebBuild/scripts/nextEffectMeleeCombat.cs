using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class nextEffectMeleeCombat : MonoBehaviour {

    public int startEffectVariation = 0;
    public static int effectVariation = 0;

    public Text VariationText;

    // public string[] VariationNames;

    void Start()
    {
        effectVariation = startEffectVariation;
        CheckEffectName();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            nextEffectVariation(false);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            nextEffectVariation(true);
        }
    }

    public void nextEffectVariation(bool increase)
    {
        if(increase)
        {
            effectVariation++;
        }
       else if (!increase)
        {
            effectVariation--;
        }
        if (effectVariation > 10)
        {
            effectVariation = 0;
        }
        if (effectVariation < 0)
        {
            effectVariation = 10;
        }
        CheckEffectName();
    }
    
    void CheckEffectName()
    {
        if (effectVariation == 0)
        {
            VariationText.text = "#1. Classic (BlueSlash+OrangeHit)";
        }
        else if (effectVariation == 1)
        {
            VariationText.text = "#2. Fire Guard (RedSlash+RedHit)";
        }
        else if (effectVariation == 2)
        {
            VariationText.text = "#3. Knight Slash (OrangeSlash+OrangeHit)";
        }
        else if (effectVariation == 3)
        {
            VariationText.text = "#4. Magic Slash (PurpleSlash+PurpleHit)";
        }
        else if (effectVariation == 4)
        {
            VariationText.text = "#5. GreenWarrior (GreenSlash+GreenHit)";
        }
        else if (effectVariation == 5)
        {
            VariationText.text = "#6. Wild One (RedGreenSlash+RedGreenHit)";
        }
        else if (effectVariation == 6)
        {
            VariationText.text = "#7. DarkKnight (DarkBlueSlash+DarkBlueHit)";
        }
        else if (effectVariation == 7)
        {
            VariationText.text = "#8. HeroFlare (HeroFlareSlash+HeroFlareHit)";
        }
        else if (effectVariation == 8)
        {
            VariationText.text = "#9. Rainbow Warrior(RainbowSlash+RainbowHit)";
        }
        else if (effectVariation == 9)
        {
            VariationText.text = "#10. Tranquility(DeepOceanSlash+DeepOceanHit)";
        }
        else if (effectVariation == 10)
        {
            VariationText.text = "#11. White (WhiteSlash+WhiteHit)";
        }
    }


}
