using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CharacterDamage : MonoBehaviour
{
    private float health = 100;
    public float damage = 10;
    public float timeToDestroy = 3f;
    public float immunityTime = 1f;
    public GameObject[] damageCollider;
    private bool waiting;

    private AnimatorStateInfo animatorState;

    // Start is called before the first frame update
    void Start()
    {
        waiting = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        animatorState = gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if (animatorState.IsName("Punch") || animatorState.IsName("Kick") || animatorState.IsName("Attack"))
        {
            foreach (GameObject sphere in damageCollider)
            {
                sphere.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject sphere in damageCollider)
            {
                sphere.SetActive(false);
            }
        }

    }

    public void OnTriggerEnter(Collider other)
    {
        if (gameObject.CompareTag("Player") && other.gameObject.CompareTag("Enemy") || gameObject.CompareTag("Enemy") && other.gameObject.CompareTag("Player"))
        {
            ReceiveDamage(other.gameObject.GetComponentInParent<CharacterDamage>().GetDamage());
        }
        //else if(gameObject.CompareTag("Enemy") && other.gameObject.CompareTag("Player"))
        //{
        //    ReceiveDamage(other.gameObject.GetComponent<CharacterDamage>().GetDamage());
        //}
    }

    //Check collider to receive damage
    //public void OnCollisionExit(Collision collision)
    //{
    //    if (gameObject.CompareTag("Player") && collision.gameObject.CompareTag("Enemy") || gameObject.CompareTag("Enemy") && collision.gameObject.CompareTag("Player"))
    //    {
    //        ReceiveDamage(collision.gameObject.GetComponentInParent<CharacterDamage>().GetDamage());
    //        //ReceiveDamage(damage);
    //    }
    //}

    public void ReceiveDamage(float damage)
    {
        if (!waiting)
        {
            health -= damage;
            StartCoroutine("WaitForDamage");
        }
        Debug.Log("Character " + gameObject.name + " received " + damage + " damage. Health: " + health);
        if (health <= 0)
        {
            gameObject.GetComponent<Animator>().SetTrigger("Dead");
            if (gameObject.CompareTag("Player"))
            {
                Debug.Log("Player dead");
                StartCoroutine("WaitForSceneLoad");
            }
            else
            {
                Debug.Log("Enemy dead");
                gameObject.GetComponent<EnemyCrabController>().ChangeStateToDeath();
                Destroy(gameObject, timeToDestroy);
            }
        }
    }

    private IEnumerator WaitForDamage()
    {
        waiting = true;
        yield return new WaitForSeconds(immunityTime);
        waiting = false;
    }

    private IEnumerator WaitForSceneLoad()
    {
        yield return new WaitForSeconds(timeToDestroy);
        //Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public int GetHealth()
    {
        return (int)health;
    }

    public void SetHealth(float newHealth)
    {
        health = newHealth;
    }

    public float GetDamage()
    {
        return damage;
    }
}
