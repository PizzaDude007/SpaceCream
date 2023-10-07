using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyDamageOutput : MonoBehaviour
{
    public GameObject damageBarParent;
    public RectTransform rect;
    public TMP_Text damageText;

    private float damageBarMax = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        damageBarParent.SetActive(false);
        damageText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        damageBarParent.transform.LookAt(Camera.main.transform);
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Color.white);
    }

    public void TakeDamage(int damage, Color color)
    {
        damageBarParent.SetActive(true);
        //damageBarMax -= damage;
        //rect.sizeDelta = new Vector2(damageBarMax, 1.0f);
        damageBarMax -= (float)damage / 100.0f;
        rect.anchorMax = new Vector2(damageBarMax, 1.0f);
        StartCoroutine(DeactivateDamageBar(damage, color));
    }

    IEnumerator DeactivateDamageBar(int damage, Color color)
    {
        damageText.gameObject.SetActive(true);
        damageText.color = color;
        damageText.text = damage.ToString();
        yield return new WaitForSeconds(2.5f);
        damageText.gameObject.SetActive(false);
    }
}
