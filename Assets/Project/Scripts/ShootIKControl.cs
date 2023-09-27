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

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        thirdPerson = GetComponent<ThirdPersonCotroller>();
        spineBone = playerAnimator.GetBoneTransform(HumanBodyBones.Spine);
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
            if(isShooting)
            {
                if(followTarget != null)
                {
                    playerAnimator.SetLookAtWeight(1);
                    playerAnimator.SetLookAtPosition(followTarget.position);

                    spineBone.rotation = followTarget.rotation;
                    //playerAnimator.SetIKRotationWeight(AvatarMaskBodyPart.Body, 0.8f);
                    //playerAnimator.bodyRotation = followTarget.rotation;
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
