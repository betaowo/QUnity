using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class WeaponAnimationSequence
{
    public string animationName = "fire";
    public int[] blendShapeFrames = new int[] { 0, 1, 2 };
    public float frameDuration = 0.05f;
    public bool loop = false;
    public int playCount = 1;
}

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class WeaponAnimator : MonoBehaviour
{
    [Header("Animation Sequences")]
    public WeaponAnimationSequence fireSequence;
    public WeaponAnimationSequence reloadSequence;
    public WeaponAnimationSequence singleReloadSequence; // formerly dryfire

    private SkinnedMeshRenderer meshRenderer;
    private Coroutine currentAnimation;
    private Coroutine queuedAnimation;
    private WeaponAnimationSequence currentSequence;

    private void Awake()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
        ResetAllBlendShapes();
    }

    public void PlayAnimation(string animationName)
    {
        WeaponAnimationSequence seq = animationName switch
        {
            "fire" => fireSequence,
            "reload" => reloadSequence,
            "reload_single" => singleReloadSequence,
            _ => null
        };

        if (seq == null)
        {
            Debug.LogWarning($"Animation '{animationName}' not found on {gameObject.name}");
            return;
        }
        PlaySequence(seq);
    }

    public void PlayFireAnimation() => PlaySequence(fireSequence);
    public void PlayReloadAnimation() => PlaySequence(reloadSequence);
    public void PlaySingleReloadAnimation() => PlaySequence(singleReloadSequence);
    public void PlayDryfireAnimation() => PlaySequence(singleReloadSequence);

    public void ResetAllBlendShapes()
    {
        if (meshRenderer == null) return;
        for (int i = 0; i < meshRenderer.sharedMesh.blendShapeCount; i++)
        {
            meshRenderer.SetBlendShapeWeight(i, 0);
        }
    }

    public void StopCurrentAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        if (queuedAnimation != null)
        {
            StopCoroutine(queuedAnimation);
            queuedAnimation = null;
        }
        currentSequence = null;
        ResetAllBlendShapes();
    }

    private void PlaySequence(WeaponAnimationSequence seq)
    {
        if (seq == null || seq.blendShapeFrames == null || seq.blendShapeFrames.Length == 0)
        {
            Debug.LogWarning($"Animation sequence is empty!");
            return;
        }

        if (currentSequence == seq && currentAnimation != null)
            return;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        currentSequence = seq;
        currentAnimation = StartCoroutine(PlayFrameSequence(seq));
    }

    private IEnumerator PlayFrameSequence(WeaponAnimationSequence seq)
    {
        int playsRemaining = seq.playCount;

        while (playsRemaining > 0)
        {
            for (int i = 0; i < seq.blendShapeFrames.Length; i++)
            {
                // reset prev frame
                if (i > 0)
                {
                    int prevFrame = seq.blendShapeFrames[i - 1];
                    if (prevFrame >= 0 && prevFrame < meshRenderer.sharedMesh.blendShapeCount)
                        meshRenderer.SetBlendShapeWeight(prevFrame, 0);
                }

                // show current frame
                int frameIndex = seq.blendShapeFrames[i];
                if (frameIndex >= 0 && frameIndex < meshRenderer.sharedMesh.blendShapeCount)
                    meshRenderer.SetBlendShapeWeight(frameIndex, 100);

                yield return new WaitForSeconds(seq.frameDuration);
            }

            // reset last frame
            if (seq.blendShapeFrames.Length > 0)
            {
                int lastFrame = seq.blendShapeFrames[seq.blendShapeFrames.Length - 1];
                if (lastFrame >= 0 && lastFrame < meshRenderer.sharedMesh.blendShapeCount)
                    meshRenderer.SetBlendShapeWeight(lastFrame, 0);
            }

            playsRemaining--;
            if (!seq.loop)
                break;
        }

        currentAnimation = null;
        currentSequence = null;
    }
}