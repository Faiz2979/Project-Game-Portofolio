using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private AudioClip dashAudioClip;
    [SerializeField] private Animator animator;
    private float lastFootstep;
    // Update is called once per frame

    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 = Base Layer
        if (stateInfo.IsName("Dashing")) return; // nama state sesuai dengan animator

        float footstep = animator.GetFloat("Footstep");

        if (lastFootstep > 0 && footstep < 0 || lastFootstep < 0 && footstep > 0)
        {
            if (audioClips.Length > 0)
            {
                int randomIndex = Random.Range(0, audioClips.Length);
                AudioSource.PlayClipAtPoint(audioClips[randomIndex], transform.position);
            }
        }


        lastFootstep = footstep;
    }

    void OnValidate()
    {
        if (audioClips.Length == 0)
        {
            Debug.LogWarning("FootstepSound: No audio clips assigned.");
        }

        if (animator == null)
        {
            Debug.LogWarning("FootstepSound: No animator assigned.");
        }
    }

    public void DashSound()
    {
        AudioSource.PlayClipAtPoint(dashAudioClip, transform.position);
    }
}
