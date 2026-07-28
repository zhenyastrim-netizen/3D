using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [SerializeField] private ParticleSystem flash;

    public void Play()
    {
        flash.Play();
    }
}