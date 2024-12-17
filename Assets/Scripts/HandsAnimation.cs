using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class HandsAnimation : MonoBehaviour
{
    public InputActionReference grip;
    public InputActionReference trigger;

    private Animator handsAnimator;
    private float gripValue;
    private float triggerValue;

    // Start is called before the first frame update
    void Start()
    {
        handsAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AnimateGrip();
        AnimateTrigger();
    }

    private void AnimateGrip()
    {
        gripValue = grip.action.ReadValue<float>();
        handsAnimator.SetFloat("Grip", gripValue);
    }

    private void AnimateTrigger()
    {
        triggerValue = trigger.action.ReadValue<float>();
        handsAnimator.SetFloat("Trigger", triggerValue);
    }
}
