using System.Collections;
using UnityEngine;

public class MeleeWeaponVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponModel;

    [Header("Timing")]
    [SerializeField] private float windupTime = 0.07f;
    [SerializeField] private float swingTime = 0.1f;
    [SerializeField] private float returnTime = 0.15f;

    [Header("Movement")]
    [SerializeField]
    private Vector3 windupRotation =
        new Vector3(10f, -25f, -15f);

    [SerializeField]
    private Vector3 attackRotation =
        new Vector3(-15f, 70f, 35f);

    [SerializeField]
    private Vector3 attackOffset =
        new Vector3(0f, 0f, 0.15f);

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Coroutine animationRoutine;

    private void Awake()
    {
        if (weaponModel == null)
            weaponModel = transform;

        startPosition = weaponModel.localPosition;
        startRotation = weaponModel.localRotation;
    }

    public void PlayAttack()
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine =
            StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        Quaternion windupTarget =
            startRotation *
            Quaternion.Euler(windupRotation);

        Quaternion attackTarget =
            startRotation *
            Quaternion.Euler(attackRotation);

        yield return Animate(
            startPosition,
            startPosition,
            startRotation,
            windupTarget,
            windupTime
        );

        yield return Animate(
            startPosition,
            startPosition + attackOffset,
            windupTarget,
            attackTarget,
            swingTime
        );

        yield return Animate(
            startPosition + attackOffset,
            startPosition,
            attackTarget,
            startRotation,
            returnTime
        );

        weaponModel.localPosition = startPosition;
        weaponModel.localRotation = startRotation;

        animationRoutine = null;
    }

    private IEnumerator Animate(
        Vector3 fromPosition,
        Vector3 toPosition,
        Quaternion fromRotation,
        Quaternion toRotation,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            weaponModel.localPosition =
                Vector3.Lerp(
                    fromPosition,
                    toPosition,
                    t
                );

            weaponModel.localRotation =
                Quaternion.Slerp(
                    fromRotation,
                    toRotation,
                    t
                );

            yield return null;
        }
    }

    private void OnDisable()
    {
        if (weaponModel == null)
            return;

        weaponModel.localPosition = startPosition;
        weaponModel.localRotation = startRotation;
    }
}