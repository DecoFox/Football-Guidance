using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TouchdownHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject touchdown;

    [SerializeField]
    private MenuBar mb;

    //A very simple onTriggerEnter system to turn on the touchdown indications

    private void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent(out PlayerInputHandler p))
        {
            touchdown.gameObject.SetActive(true);
            mb.SetGameState(MenuBar.GameState.Touchdown);
        }
    }

    public void ResetTouchdown()
    {
        touchdown.SetActive(false);
    }

}
