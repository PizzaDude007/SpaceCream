using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ShootIKControl : MonoBehaviour
{
    private Animator playerAnimator;
    private bool isShooting = false;
    public Transform rightHandTarget;
    public Transform followTarget;
    private Transform spineBone;

    public float rotateWeight = 0.8f, positionWeight = 0.8f;

    private ThirdPersonCotroller thirdPerson;

    public Cinemachine.CinemachineVirtualCamera aimCamera;

    [SerializeField]
    private Vector3 shoulderOffset;

    private Quaternion quaternion;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        thirdPerson = GetComponent<ThirdPersonCotroller>();
        spineBone = playerAnimator.GetBoneTransform(HumanBodyBones.Spine);
        quaternion = Quaternion.Euler(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1) || Input.GetAxis("ADS") == 1)
        {
            isShooting = true;
            aimCamera.Priority = 11;
        }
        else
        {
            isShooting = thirdPerson.isShooting;
            aimCamera.Priority = 9;
        }

    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (playerAnimator)
        {
            if(isShooting && Time.timeScale != 0)
            {
                if(followTarget != null)
                {
                    playerAnimator.SetLookAtWeight(1);
                    playerAnimator.SetLookAtPosition(followTarget.position);

                    Quaternion lookAt = Quaternion.LookRotation(followTarget.transform.position - playerAnimator.GetBoneTransform(HumanBodyBones.Chest).position);
                    Quaternion correction = Quaternion.Euler(shoulderOffset);

                    float angle = Vector3.Angle(followTarget.transform.forward, playerAnimator.transform.forward);
                    if (angle > 20 && angle < 45)
                    {
                        playerAnimator.SetBoneLocalRotation(HumanBodyBones.Chest, lookAt * correction);
                    }
                    else if (angle > 45)
                    {
                        playerAnimator.SetBoneLocalRotation(HumanBodyBones.Chest, lookAt * correction);
                        playerAnimator.SetBoneLocalRotation(HumanBodyBones.Hips, Quaternion.FromToRotation(playerAnimator.transform.forward, followTarget.transform.forward));
                    }

                    //playerAnimator.SetBoneLocalRotation(HumanBodyBones.Chest, Quaternion.FromToRotation(playerAnimator.transform.forward, followTarget.transform.forward));


                    //if (angle > shoulderOffset)
                    //{
                    //    Quaternion lookAt = Quaternion.LookRotation(followTarget.transform.forward);
                    //    playerAnimator.SetBoneLocalRotation(HumanBodyBones.Hips, Quaternion.FromToRotation(playerAnimator.transform.forward, followTarget.transform.forward));
                    //    //playerAnimator.SetFloat("speed", 1f);
                    //    //float direction = Vector3.Dot(playerAnimator.transform.right, followTarget.transform.forward);
                    //    //playerAnimator.SetFloat("direction", direction);
                    //    //playerAnimator.bodyRotation = Quaternion.FromToRotation(playerAnimator.transform.forward, followTarget.transform.forward);
                    //}
                    //else
                    //{
                    //    playerAnimator.SetFloat("rotate", 0f);
                    //    quaternion = Quaternion.FromToRotation(playerAnimator.transform.forward, followTarget.transform.forward);
                    //    playerAnimator.SetBoneLocalRotation(HumanBodyBones.Chest, quaternion);
                    //}
                }

                if(rightHandTarget != null)
                {
                    playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, positionWeight);
                    playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, rotateWeight);
                    playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                    playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
                }
            }
            else
            {
                playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                //playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                //playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                playerAnimator.SetLookAtWeight(1);
                playerAnimator.SetLookAtPosition(followTarget.position);
            }
        }
    }

}
