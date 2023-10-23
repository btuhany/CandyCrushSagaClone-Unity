using UnityEngine;
using Tools;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    [SerializeField] private AudioClip[] _audioClips;
    [SerializeField] private AudioClip _matchRemovedClip;
    [SerializeField] private AudioSource _audioSource2;
    private AudioSource _audioSource;
    protected override void Awake()
    {
        base.Awake();
        _audioSource= GetComponent<AudioSource>();
    }
    public void PlaySound(int index)
    {
        _audioSource.PlayOneShot(_audioClips[index]);
    }
    public void PlayRandomInRangeOf(int inclusiveIndex, int exclusiveIndex)
    {
        _audioSource.PlayOneShot(_audioClips[Random.Range(inclusiveIndex, exclusiveIndex)]);
    }
    public void PlayMatchRemovedClip()
    {
        _audioSource2.PlayOneShot(_matchRemovedClip);
    }
    
}
