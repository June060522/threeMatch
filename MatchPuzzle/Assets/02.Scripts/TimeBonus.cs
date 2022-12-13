using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GamePiece))]
public class TimeBonus : MonoBehaviour
{
    [Range(0, 5)]
    public int bonusValue = 5;

    [Range(0f, 1f)]
    public float chanceForBonus = 0.1f;
    public GameObject bonusGlow;
    public Material[] bonusMaterials;
    public GameObject ringGlow;

    private void Start()
    {
        float random = Random.Range(0f, 1f);

        if(random > chanceForBonus)
        {
            bonusValue = 0;
        }
        if(GameManager.Instance != null)
        {
            if(GameManager.Instance.LevelGoalTimed == null)
            {
                bonusValue = 0;
            }
        }
        SetActive(bonusValue != 0);
        if(bonusValue != 0)
        {
            SetupMaterial(bonusValue -1, bonusGlow);
        }
    }

    private void SetupMaterial(int value, GameObject bonusGlow)
    {
        int clampValue = Mathf.Clamp(value, 0, bonusMaterials.Length - 1);

        if(bonusMaterials[clampValue] != null)
        {
            if(bonusGlow != null)
            {
                ParticleSystemRenderer bonusGlowRenderer = bonusGlow.GetComponent<ParticleSystemRenderer>();
                bonusGlowRenderer.material = bonusMaterials[clampValue];
            }
        }
    }

    private void SetActive(bool state)
    {
        if(bonusGlow != null)
        {
            bonusGlow.SetActive(state);
        }

        if(ringGlow != null)
        {
            ringGlow.SetActive(state);
        }
    }
}
