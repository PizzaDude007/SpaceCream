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

    //public float lookAtSpeed = 0.8f, chestRotationSpeed = 0.8f, hipsRotationSpeed = 0.8f, handIKSpeed = 0.8f;
    public float lookAtWeight = 1.0f, bodyWeight = 0.8f, headWeight = 0.8f, eyesWeight = 0.8f, clampWeight = 0.8f;
    private Vector3 currentLookAtPosition;

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
        spineBone = playerAnimator.GetBoneTransform(HumanBodyBones.Hips);
        quaternion = Quaternion.Euler(0, 0, 0);

        //Set look-at position to followTarget
        currentLookAtPosition = transform.position + transform.forward;
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
            if (isShooting && Time.timeScale != 0)
            {
                if (followTarget != null)
                {
                    playerAnimator.SetLookAtWeight(1.0f);
                    playerAnimator.SetLookAtWeight(lookAtWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
                    playerAnimator.SetLookAtPosition(followTarget.position);
                }

                if (rightHandTarget != null)
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
