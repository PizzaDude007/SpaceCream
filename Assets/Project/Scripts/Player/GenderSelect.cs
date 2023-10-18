using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenderSelect : MonoBehaviour
{
    public GameObject malePlayer;
    public GameObject femalePlayer;

    public UnityEvent onMaleSet, onFemSet;

    private PlayerBehaviour player = PlayerBehaviour.Instance;

    // Start is called before the first frame update
    void Start()
    {
        if (player.player.isFemale)
        {
            SetFemale();
        }
        else
        {
            SetMale();
        }

        player.genderSelect = this;
    }

    public void SetFemale()
    {
        Debug.Log("Set Female");
        malePlayer.SetActive(false);
        femalePlayer.SetActive(true);
        femalePlayer.transform.position = malePlayer.transform.position;
        onFemSet.Invoke();
    }

    public void SetMale()
    {
        Debug.Log("Set Male");
        femalePlayer.SetActive(false);
        malePlayer.SetActive(true);
        malePlayer.transform.position = femalePlayer.transform.position;
        onMaleSet.Invoke();
    }
}
