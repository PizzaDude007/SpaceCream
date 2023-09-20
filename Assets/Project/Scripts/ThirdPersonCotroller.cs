using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ThirdPersonCotroller : MonoBehaviour
{
    private Animator playerAnimator;
    private AnimatorStateInfo playerAnimatorInfo;

    public GameObject playerTarget;
    public float mouseSensX = 5f, mouseSensY = 5f, controlSensX = 1f, rotationSpeed = 0.3f, counterRotate = 0.5f;
    public float speedJump = 10f;

    [SerializeField] private float maxAngle = 90f;

    //public Transform targetToGrab;

    //public GameObject cocktail;

    private float heightCollider = 1f, baseHeight, baseRadius;
    private float gravity;
    private Vector3 InitialGravity;
    //private float timeScale;
    private CapsuleCollider capsuleCollider;

    private string levelName;
    public float soundTime = 1f;

    private bool isRunning;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerTarget.transform.Rotate(0, 0, 0);
        baseHeight = capsuleCollider.height;
        //baseRadius = capsuleCollider.radius;
        InitialGravity = Physics.gravity;

        isRunning = false;

        //Ver si es nieve o desierto
        levelName = SceneManager.GetActiveScene().name;
        //StartCoroutine(PlayRunSound());
    }

    private void FixedUpdate()
    {
        //Audio al caminar
        if (Input.GetAxis("Vertical") != 0 || playerAnimatorInfo.IsName("runs") && !isRunning)
        {
            isRunning = true;
            StartCoroutine(PlayRunSound());
        }
    }

    IEnumerator PlayRunSound()
    {
        //if(Input.GetAxis("Vertical") != 0 || playerAnimatorInfo.IsName("runs"))
        //{
        SoundFxManager.Instance.StopSFX();
        switch (levelName)
        {
            case "level_snow":
                SoundFxManager.Instance.RunSnow();
                break;
            case "level_desert":
                SoundFxManager.Instance.RunSand();
                break;
        }
        yield return new WaitForSeconds(soundTime);
        SoundFxManager.Instance.StopSFX();
        isRunning = false;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");

        playerAnimatorInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        playerAnimator.SetFloat("speed", Input.GetAxis("Vertical"));
        playerAnimator.SetFloat("direction", horizontal);

        //Para rodar
        if (Input.GetButtonDown("Roll") && playerAnimatorInfo.IsName("runs"))
        {
            playerAnimator.SetTrigger("roll");
        }

        //Para disparar
        if (Input.GetButtonDown("Fire1"))
        {
            playerAnimator.SetTrigger("shoot");
        }

        //Debug.Log("Capsule collider = " + capsuleCollider.height + "\n Base Height = " + baseHeight + "\n HeightCollider = " + heightCollider);
        heightCollider = playerAnimator.GetFloat("HeightCollider");
        capsuleCollider.height = heightCollider * baseHeight;
        //capsuleCollider.radius = 1/heightCollider * baseRadius;

        //Debug.Log("Player Velocity = " + gameObject.GetComponent<Rigidbody>().velocity);
        gameObject.GetComponent<Rigidbody>().velocity += speedJump * Math.Abs(1 - heightCollider) * Vector3.up * Time.deltaTime;
        //Debug.Log("Player Velocity with Height = " + gameObject.GetComponent<Rigidbody>().velocity);

        //gravity = playerAnimator.GetFloat("Gravity");
        //Physics.gravity = InitialGravity * gravity;

        //angle between player and camera
        float angle = Vector3.Angle(playerTarget.transform.forward, transform.forward);
        
        //Rotar camera con mouse
        if(Input.GetAxis("Vertical") != 0)
        {
            transform.Rotate(0, Input.GetAxis("Controller X") * controlSensX, 0);
            //centrar la camara
            if (angle > maxAngle)
            {
                if (playerTarget.transform.rotation.y < transform.rotation.y)
                    playerTarget.transform.Rotate(0, 1f * mouseSensX, 0);
                else
                    playerTarget.transform.Rotate(0, -1f * mouseSensX, 0);
            }
            else
            {   //forzar la camara centrada si se usa mouse
                transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensX, 0);
            }
        } 
        else
        {
            playerTarget.transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensX, 0);
            playerTarget.transform.Rotate(0, Input.GetAxis("Controller X") * controlSensX, 0);
        }

        //playerTarget.transform.Rotate(0, 0, Input.GetAxis("Mouse Y") * mouseSensY);

        //Para rotar al jugador
        if (playerAnimator.GetFloat("speed") == 0 && horizontal != 0)
        {
            transform.Rotate(0, horizontal * rotationSpeed, 0);
            if(angle>maxAngle)
                playerTarget.transform.Rotate(0f, -horizontal * counterRotate, 0f);
        }

        //if (Input.GetAxis("Vertical") > 0 && playerTarget.transform.rotation != transform.rotation && angle > maxAngle)
        //{
        //    if (playerTarget.transform.rotation.y < transform.rotation.y)
        //        playerTarget.transform.Rotate(0, 1f * mouseSensX / 5 * Time.deltaTime, 0);
        //    else
        //        playerTarget.transform.Rotate(0, -1f * mouseSensX / 5 * Time.deltaTime, 0);
        //}




        //Para saludar
        /*if (Input.GetKeyDown(KeyCode.E))
        {
            playerAnimator.SetTrigger("wave");
        }*/

        /*
        timeScale = playerAnimator.GetFloat("TimeScale");

        
        Time.timeScale = timeScale;*/

        //if (Input.GetKeyDown(KeyCode.G))
        //    playerAnimator.SetTrigger("Grab");
    }

    //Para que el personaje agarre el objeto que se encuentra en el target (calabaza)
    /*private void OnAnimatorIK(int layerIndex)
    {
        playerAnimatorInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        if (playerAnimatorInfo.IsName("Grab"))
        {
            playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, targetToGrab.position);
            playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, targetToGrab.rotation);
        }
        else
        {
            playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
        }
    }

    public void EnableCocktail()
    {
        cocktail.SetActive(true);
    }
    public void DisableCocktail()
    {
        cocktail.SetActive(false);
    }*/
}
