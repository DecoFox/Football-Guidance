using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuBar : MonoBehaviour
{
    [SerializeField]
    private Slider lowerSpeedRange;
    [SerializeField]
    private Slider upperSpeedRange;

    [SerializeField]
    private Slider lowerReactionRange;
    [SerializeField]
    private Slider upperReactionRange;

    [SerializeField]
    private Toggle randomizeSpeed;
    [SerializeField]
    private Toggle randomizeReaction;

    [SerializeField]
    private TMP_InputField setSpeed;
    [SerializeField]
    private TMP_InputField setReaction;

    [SerializeField]
    private TMP_Text lowSpeed;
    [SerializeField]
    private TMP_Text highSpeed;

    [SerializeField]
    private TMP_Text lowReac;
    [SerializeField]
    private TMP_Text highReac;

    [SerializeField]
    private TMP_Text averageSpeedStat;
    [SerializeField]
    private TMP_Text averageReactionStat;

    [SerializeField]
    private Toggle regenDefense;

    [SerializeField]
    DefenseOrganizer dO;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        lowSpeed.text = "" + (float)Mathf.RoundToInt(lowerSpeedRange.value * 100) / 100;
        highSpeed.text = "" + (float)Mathf.RoundToInt(upperSpeedRange.value * 100) / 100;

        lowReac.text = "" + lowerReactionRange.value;
        highReac.text = "" + upperReactionRange.value;
    }

    public void SpawnDefense()
    {
        DefenseOrganizer.SpawnContext ctx = new DefenseOrganizer.SpawnContext();
        ctx.randomSpeed = randomizeSpeed.isOn;
        ctx.randomReaction = randomizeReaction.isOn;

        //Construct a spawn context object, which will convey respawn settings to our new defense team
        if (!ctx.randomSpeed)
        {
            float desiredSpeed;
            if (float.TryParse(setSpeed.text, out desiredSpeed))
            {
                ctx.setSpeed = desiredSpeed;
            }
            else
            {
                ctx.setSpeed = 1;
                Debug.LogWarning("Speed set failed. Defaulting to one...");
            }
        }
        else
        {
            ctx.minSpeed = lowerSpeedRange.value;
            ctx.maxSpeed = upperSpeedRange.value;
        }

        if (!ctx.randomReaction)
        {
            int desiredReaction;
            if (int.TryParse(setReaction.text, out desiredReaction))
            {
                ctx.setReaction = desiredReaction;
            }
            else
            {
                ctx.setReaction = 0;
                Debug.LogWarning("Reaction set failed. Defaulting to zero...");
            }
        }
        else
        {
            ctx.minReaction = (int)lowerReactionRange.value;
            ctx.maxReaction = (int)upperReactionRange.value;
        }

        dO.SpawnDefense(regenDefense.isOn, ctx);
    }

    public void SetAverages(float averageSpeed, float averageReaction)
    {
        averageSpeedStat.text = "" + averageSpeed;
        averageReactionStat.text = "" + averageReaction;
    }

}
