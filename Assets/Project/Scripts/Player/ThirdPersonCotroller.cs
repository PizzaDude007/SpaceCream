using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using Unity.Services.Analytics;
using Unity.Services.Core;

public class ThirdPersonCotroller : MonoBehaviour
{
    private Animator playerAnimator;
    private AnimatorStateInfo playerAnimatorInfo;

    public GameObject playerTarget, lookTarget;
    private Rigidbody rigidBody;
    public float mouseSensX = 5f, mouseSensY = 5f, controlSensX = 1f, controlSensY = 0.5f, rotationSpeed = 0.3f, counterRotate = 0.5f;
    public float controlDeadZoneX = 0.1f, controlDeadZoneY = 0.1f;
    public float speedJump = 10f;

    public int maxJumps = 1, maxHeight = 10;
    private int numJumps;

    [SerializeField]
    private float maxAngle = 60f;

    //public Transform targetToGrab;

    //public GameObject cocktail;

    private float heightCollider = 1f, baseHeight, baseRadius;
    private float gravity;
    private Vector3 InitialGravity;
    //private float timeScale;
    private CapsuleCollider capsuleCollider;

    private string levelName;
    public float soundTime = 1f, shootTime = 1.8f;

    private bool isRunning;
    
    private VisualEffect vfxMuzzleFlash;
    private Transform bulletBarrel;
    public GameObject bulletPrefab;
    public float bulletForce = 40f;
    public bool isShooting = false;

    public float invulnerableTime = 1f;
    private bool isInvulnerable = false;

    private string activeScene;

    private bool isShootingLevel;

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

        rigidBody = GetComponent<Rigidbody>();

        //Set jump variables
        numJumps = 0;

        //Ver si es nieve o desierto
        levelName = SceneManager.GetActiveScene().name;
        isShootingLevel = levelName.Contains("level");

        vfxMuzzleFlash = GetComponentInChildren<VisualEffect>();
        bulletBarrel = vfxMuzzleFlash.gameObject.transform;
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
        //SoundFxManager.Instance.StopSFX();
        switch (levelName)
        {
            case "level_snow":
                SoundFxManager.Instance.RunSnow();
                break;
            case "level_desert":
                SoundFxManager.Instance.RunSand();
                break;
            default:
                SoundFxManager.Instance.Walk();
                break;
        }
        yield return new WaitForSeconds(soundTime);
        //SoundFxManager.Instance.StopSFX();
        isRunning = false;
        //}
    }

    IEnumerator IsShootingWait()
    {
        isShooting = true;
        yield return new WaitForSeconds(shootTime*2.5f);
        //yield return new WaitUntil(() => (!playerAnimator.GetCurrentAnimatorStateInfo(1).IsName("shoot")));
        //WaitForSeconds wait = new WaitForSeconds(0.5f);
        isShooting = false;
    }

    IEnumerator PlayShootVFX()
    {
        playerAnimator.SetTrigger("shoot");
        //yield return new WaitUntil(() => (!playerAnimator.GetCurrentAnimatorStateInfo(1).IsName("shoot")));
        yield return new WaitForSeconds(shootTime);
        shootBullet();
        AnalyticsService.Instance.CustomData("weaponFired", new Dictionary<string, object>
        {
            { "levelName", levelName }
        });
        vfxMuzzleFlash.Play();
    }

    public void shootBullet()
    {
        SoundFxManager.Instance.Shoot();
        GameObject bullet = Instantiate(bulletPrefab, bulletBarrel.position, bulletBarrel.rotation);
        bullet.transform.LookAt(lookTarget.transform.position);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletForce, ForceMode.Impulse);                                
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");

        playerAnimatorInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        playerAnimator.SetFloat("speed", Input.GetAxis("Vertical"));
        playerAnimator.SetFloat("direction", horizontal);

        //Para correr
        if (!isShootingLevel)
        {
            //No se puede correr en la heladería
        }
        else if (isShootingLevel && Input.GetButton("Run") && (playerAnimatorInfo.IsName("runs") || playerAnimatorInfo.IsName("jog")))
        {
            playerAnimator.SetBool("run", true);
            rigidBody.AddForce(transform.forward * 2f, ForceMode.Impulse);
        }
        else
        {
            playerAnimator.SetBool("run", false);
        }

        //Para rodar
        if (Input.GetButtonDown("Roll"))
        {
            playerAnimator.SetTrigger("roll");
            AnalyticsService.Instance.CustomData("rollingAction", new Dictionary<string, object>
            {
                { "levelName", levelName }
            });
        }

        //Para disparar
        if ((Input.GetButtonDown("Fire1") || Input.GetAxis("Shoot") == 1) && isShootingLevel)
        {
            StopAllCoroutines(); 
            StartCoroutine("IsShootingWait");
            StartCoroutine("PlayShootVFX");
            //isShooting = true;
        }
        
        //Para saltar
        heightCollider = playerAnimator.GetFloat("HeightCollider");
        capsuleCollider.height = heightCollider * baseHeight;

        if(Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        rigidBody.velocity += speedJump * Math.Abs(1 - heightCollider) * Vector3.up * Time.deltaTime;
        //Debug.Log("Player Velocity with Height = " + gameObject.GetComponent<Rigidbody>().velocity);


        //angle between player and camera
        float angle = Vector3.Angle(playerTarget.transform.forward, transform.forward);
            
        float controllerX = Input.GetAxis("Controller X");
        //Debug.Log("X pos controller = " + controllerX);
        
        //Rotar cuerpo con mouse
        if(Input.GetAxis("Vertical") != 0 && Time.timeScale != 0)
        {
            if(controllerX > controlDeadZoneX || controllerX < controlDeadZoneX * -1)
                transform.Rotate(0, controllerX * controlSensX, 0);
            //centrar la camara
            if (angle > maxAngle)
            {
                //playerTarget.transform.rotation = Quaternion.Slerp(playerTarget.transform.rotation, transform.rotation, Time.deltaTime * 1f);
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
            playerTarget.transform.Rotate(0, controllerX * controlSensX, 0);
        }

        float controllerY = Input.GetAxis("Controller Y") * -1f;
        //Debug.Log("Y pos controller = " + controllerY);

        //Rotar camera con mouse si no esta pausado
        if(Time.timeScale != 0)
        {
            lookTarget.transform.position += new Vector3(0, Input.GetAxis("Mouse Y") * mouseSensY, 0);
            if (controllerY > controlDeadZoneY || controllerY < controlDeadZoneY * -1)
                lookTarget.transform.position += new Vector3(0, controllerY * controlSensY, 0);
        }

        //Para rotar al jugador
        //if (playerAnimator.GetFloat("speed") == 0 && horizontal != 0)
        if (horizontal != 0)
        {
            transform.Rotate(0, horizontal * rotationSpeed, 0);
            if(angle>maxAngle)
                playerTarget.transform.Rotate(0f, -horizontal * counterRotate, 0f);
        }

    }

    private void Jump()
    {
        if(numJumps < maxJumps)
        {
            numJumps++;
            playerAnimator.SetTrigger("jump");
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);
            rigidBody.AddForce(Vector3.up * speedJump, ForceMode.VelocityChange);

            AnalyticsService.Instance.CustomData("jumpingAction", new Dictionary<string, object>
            {
                { "levelName", levelName }
            });
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.layer == 8 && !isInvulnerable) //Enemy
            {
                //Player recieves damage based on enemy type
                PlayerBehaviour.Instance.TakeDamage(collision.gameObject.GetComponent<AssignEnemyType>().enemy.GetAttackDamage()); 
                StartCoroutine("IsInvulnerable");
                Debug.Log("Enemy " + collision.gameObject.name + " hit Player");
            }
            else if(collision.gameObject.layer != 7) //Not floor
            {
                Debug.Log("Hit " + collision.gameObject.name + ", Layer = " + collision.gameObject.layer);
            }
            else
            {
            numJumps = 0;
            }
        }
    }

    IEnumerator IsInvulnerable()
    {
        isInvulnerable = true;
        Debug.Log("Player is invulnerable");
        yield return new WaitForSeconds(invulnerableTime);
        isInvulnerable = false;
    }
}
