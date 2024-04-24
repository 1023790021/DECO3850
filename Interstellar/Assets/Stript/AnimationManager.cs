using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public GameObject Model;
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = Model.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetTrigger("TrSurprise");
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            animator.SetTrigger("TrpickUp");
        }
    }
}
