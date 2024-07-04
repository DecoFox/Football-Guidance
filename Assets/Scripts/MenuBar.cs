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
    private Button gameStateSwitch;
    [SerializeField]
    private TMP_Text gameStateIndicator;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Transform playerSpawn;

    [SerializeField]
    DefenseOrganizer dO;

    [SerializeField]
    TMP_Text runStat;

    [SerializeField]
    TMP_Text bestRun;

    [SerializeField]
    TouchdownHandler th;

    private float bestRunSoFar;



    private GameState gameState = GameState.Stopped;

    public enum GameState
    {
        Stopped,
        Running,
        Tackled,
        Touchdown
    }


    // Start is called before the first frame update
    void Start()
    {
        SetGameState(GameState.Stopped);
    }

    public void ToggleGameState()
    {
        switch (gameState)
        {
            case GameState.Running:
                SetGameState(GameState.Stopped);
                break;
            case GameState.Stopped:
                SetGameState(GameState.Running);
                break;
            case GameState.Tackled:
                ResetPlayer();
                SpawnDefense(true);
                SetGameState(GameState.Stopped);
                break;
            case GameState.Touchdown:
                bestRun.text = "Touchdown!";
                //Max out run distance for the touchdown record
                bestRunSoFar = 100.0f;
                ResetPlayer();
                SpawnDefense(true);
                SetGameState(GameState.Stopped);
                break;
        }

    }

    public void PlayerTackled()
    {
        SetGameState(GameState.Tackled);
        float run = Mathf.Round((playerSpawn.InverseTransformPoint(player.transform.position).z * 1.09361f) * 100.0f) / 100.0f;
        if (run > bestRunSoFar)
        {
            bestRunSoFar = run;
            bestRun.text = "" + bestRunSoFar;
        }
    }

    //Stopping and starting the game.
    //For more complex projects, it would probably be a better idea to use singletons and C# Events. For a single-scene demo of this scale, this is expedient.
    public void SetGameState(GameState gS)
    {
        this.gameState = gS;

        switch (gS)
        {
            case GameState.Running:
                gameStateIndicator.text = "RUNNING...";
                Cursor.lockState = CursorLockMode.Locked;
                th.ResetTouchdown();
                break;
            case GameState.Stopped:
                gameStateIndicator.text = "START";
                Cursor.lockState = CursorLockMode.Confined;
                th.ResetTouchdown();
                break;
            case GameState.Tackled:
                gameStateIndicator.text = "RESTART";
                Cursor.lockState = CursorLockMode.Confined;
                th.ResetTouchdown();
                break;
            case GameState.Touchdown:
                gameStateIndicator.text = "RESTART";
                Cursor.lockState = CursorLockMode.Confined;
                break;
        }


        if(gameState == GameState.Stopped || gameState == GameState.Tackled || gameState == GameState.Touchdown)
        {
            player.GetComponent<PlayerInputHandler>().enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            player.GetComponent<Rigidbody>().isKinematic = true;
            dO.SetDefenseActive(false);
        }
        else
        {
            player.GetComponent<PlayerInputHandler>().enabled = true;
            player.GetComponent<CharacterController>().enabled = true;
            player.GetComponent<Rigidbody>().isKinematic = false;
            dO.SetDefenseActive(true);
        }


    }

    public void ResetPlayer()
    {
        player.transform.position = playerSpawn.position;
        player.transform.rotation = playerSpawn.rotation;
        player.GetComponent<PlayerState>().ResetHealth();
        player.GetComponent<PlayerInputHandler>().ResetCamera();
    }
    
    public GameState GetGameState()
    {
        return this.gameState;
    }

    // Update is called once per frame
    void Update()
    {
        lowSpeed.text = "" + (float)Mathf.RoundToInt(lowerSpeedRange.value * 100) / 100;
        highSpeed.text = "" + (float)Mathf.RoundToInt(upperSpeedRange.value * 100) / 100;

        lowReac.text = "" + lowerReactionRange.value;
        highReac.text = "" + upperReactionRange.value;

        if(gameState == GameState.Running)
        {
            //Distance in Yards
            float run = Mathf.Round((playerSpawn.InverseTransformPoint(player.transform.position).z * 1.09361f) * 100.0f) / 100.0f;
            runStat.text = "" + run;
        }
    }

    public void ResetBest()
    {
        bestRunSoFar = 0;
        bestRun.text = "" + bestRunSoFar;
    }

    public void SpawnDefense(bool forceConservationSpawn)
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
        if (!forceConservationSpawn)
        {
            //Reset personal best if we reroll defense stats. Personal best is per defense.
            if (regenDefense.isOn)
            {
                ResetBest();
            }
            dO.SpawnDefense(regenDefense.isOn, ctx);
        }
        else
        {
            dO.SpawnDefense(false, ctx);
        }

    }



    public void SetAverages(float averageSpeed, float averageReaction)
    {
        averageSpeedStat.text = "" + averageSpeed;
        averageReactionStat.text = "" + averageReaction;
    }

}
