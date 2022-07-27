using UnityEngine;
class AnimationTrigger : MonoBehaviour
{
    public void StartAnimation()
    {
        if (TryGetComponent(out Animation animation))
        {
            animation.Play();
        }
    }
}
