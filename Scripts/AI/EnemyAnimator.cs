using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class AnimSeq
{
    public string name;
    public int[] frames;
    public float frameTime = 0.08f;
    public bool loop = false;
}

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private AnimSeq idleSeq;
    [SerializeField] private AnimSeq walkSeq;
    [SerializeField] private AnimSeq attackSeq;
    [SerializeField] private AnimSeq painSeq;
    [SerializeField] private AnimSeq deathSeq;

    private SkinnedMeshRenderer mesh;
    private Coroutine curRoutine;
    private AnimSeq curSeq;
    private bool isWalking;

    private void Awake()
    {
        mesh = GetComponent<SkinnedMeshRenderer>();
        ResetAll();
        PlayIdle();
    }

    public void PlayIdle()
    {
        if (curSeq == deathSeq) return;
        if (curSeq == idleSeq && curRoutine != null) return;
        Play(idleSeq);
    }

    public void SetWalking(bool walking)
    {
        if (isWalking == walking) return;
        if (curSeq == deathSeq) return;
        if (curSeq == painSeq && curRoutine != null) return;

        isWalking = walking;
        Play(walking ? walkSeq : idleSeq);
    }

    public void PlayAttack()
    {
        if (curSeq == deathSeq) return;
        if (curSeq == painSeq && curRoutine != null) return;
        Play(attackSeq);
    }

    public void PlayPain()
    {
        if (curSeq == deathSeq) return;
        Play(painSeq);
    }

    public void PlayDeath()
    {
        StopAllCoroutines();
        curSeq = deathSeq;
        curRoutine = StartCoroutine(PlaySeq(deathSeq, true));
    }

    public void ResetAll()
    {
        if (mesh == null) return;
        for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
            mesh.SetBlendShapeWeight(i, 0);
    }

    private void Play(AnimSeq seq)
    {
        if (seq == null || seq.frames == null || seq.frames.Length == 0) return;
        if (curSeq == deathSeq) return;

        if (curRoutine != null)
            StopCoroutine(curRoutine);

        curSeq = seq;
        curRoutine = StartCoroutine(PlaySeq(seq, false));
    }

    private IEnumerator PlaySeq(AnimSeq seq, bool isDeath)
    {
        do
        {
            for (int i = 0; i < seq.frames.Length; i++)
            {
                ResetAll();
                int f = seq.frames[i];
                if (f >= 0 && f < mesh.sharedMesh.blendShapeCount)
                    mesh.SetBlendShapeWeight(f, 100);
                yield return new WaitForSeconds(seq.frameTime);
            }
            ResetAll();
        }
        while (seq.loop && curSeq == seq && !isDeath);

        if (isDeath && seq.frames.Length > 0)
        {
            // hold last death frame
            int last = seq.frames[seq.frames.Length - 1];
            if (last >= 0 && last < mesh.sharedMesh.blendShapeCount)
                mesh.SetBlendShapeWeight(last, 100);
        }

        if (!isDeath && curSeq == seq && seq != idleSeq)
        {
            curSeq = null;
            Play(isWalking ? walkSeq : idleSeq);
        }

        if (curSeq == seq)
            curRoutine = null;
    }
}