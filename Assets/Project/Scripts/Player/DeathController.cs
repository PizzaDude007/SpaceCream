using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class DeathController : MonoBehaviour
{
    public UnityEvent onPlayerDeath;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerDeath()
    {
        Debug.Log("Player died");
        onPlayerDeath.Invoke();
        animator.SetTrigger("death");
        StartCoroutine("Death");
    }

    IEnumerator Death()
    {
        
        yield return new WaitForSeconds(8f);
        SceneManager.LoadScene("ice_cream_shop");
    }
}
