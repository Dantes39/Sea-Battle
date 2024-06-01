using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnScript : MonoBehaviour
{
    public AudioSource thatFx;
    public AudioClip thatClip;

    public void Click()
    {
        thatFx.PlayOneShot(thatClip);
    }
}
