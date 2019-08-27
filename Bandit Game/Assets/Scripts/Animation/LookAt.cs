using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Animator anim;
    public Vector3 addRotation;
    
    void LateUpdate()
    {
        Transform chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        Transform spine = anim.GetBoneTransform(HumanBodyBones.Spine);        
        chest.Rotate(addRotation / 2.0f);
        spine.Rotate(addRotation / 2.0f);        
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Quaternion quatRotation = Quaternion.Euler(addRotation);
        anim.SetIKPosition(AvatarIKGoal.LeftHand, transform.TransformPoint(quatRotation * transform.InverseTransformPoint(anim.GetIKPosition(AvatarIKGoal.LeftHand))));
        anim.SetIKPosition(AvatarIKGoal.RightHand, transform.TransformPoint(quatRotation * transform.InverseTransformPoint(anim.GetIKPosition(AvatarIKGoal.RightHand))));
    }
}