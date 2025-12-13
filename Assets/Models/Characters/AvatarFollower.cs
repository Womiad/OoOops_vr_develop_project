using UnityEngine;

public class AvatarFollower : MonoBehaviour
{
    public Transform headTarget;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator not found!");
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // 啟動 IK
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);

        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);

        // 直接旋轉頭骨骼
        if (headTarget != null)
        {
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            head.rotation = headTarget.rotation;
        }
    }
}
