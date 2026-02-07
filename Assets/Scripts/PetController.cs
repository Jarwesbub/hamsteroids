using System.Collections;
using UnityEngine;
using UnityEngine.UI;

enum PetMoveActivity
{
    Rest,
    Study
}

public class PetController : MonoBehaviour
{
    [SerializeField] private GameObject Pet;
    [SerializeField] private GameObject Hamster;
    [SerializeField] private GameObject HamsterStudies;
    [SerializeField] private GameObject Bed;
    [SerializeField] private GameObject Desk;

    [SerializeField] private Animator animator;

    private Vector3 BedTarget;
    private Vector3 DeskTarget;
    [SerializeField] private float moveSpeed = 100f;

    [SerializeField] private Sprite BedEmpty;
    [SerializeField] private Sprite BedTaken;

    private Vector3 petStartPos;

    private Coroutine moveCoroutine;

    void Awake()
    {
        petStartPos = Pet.transform.position;
        Bed.GetComponent<Image>().sprite = BedEmpty;

        // Calculate bottom positions for UI elements
        RectTransform bedRect = Bed.GetComponent<RectTransform>();
        RectTransform deskRect = Desk.GetComponent<RectTransform>();

        // Hard coded offset to move the pet below the UI elements (adjust as needed).
        float offsetY = 0.5f;
        BedTarget = Bed.transform.position - new Vector3(0, offsetY, 0);
        DeskTarget = Desk.transform.position - new Vector3(0, offsetY, 0);

        HamsterStudies.SetActive(false);
    }

    public void MoveToStudy(System.Action onComplete = null)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveToTargetAndReturn(Pet.transform, DeskTarget, onComplete, PetMoveActivity.Study));
    }

    public void MoveToRest(System.Action onComplete = null)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveToTargetAndReturn(Pet.transform, BedTarget, onComplete, PetMoveActivity.Rest));
    }

    public void Eat(System.Action onComplete = null)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        animator.SetTrigger("triggerOnEat");
        StartCoroutine(WaitForAnimationEnds("hamster_eat", onComplete));
    }

    public void Exercise(System.Action onComplete = null)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        animator.SetTrigger("triggerOnExercise");
        StartCoroutine(WaitForAnimationEnds("hamster_exercise", onComplete));
    }

    public void Play(System.Action onComplete = null)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        animator.SetTrigger("triggerOnPlay");
        StartCoroutine(WaitForAnimationEnds("hamster_play", onComplete));
    }

    private IEnumerator WaitForAnimationEnds(string animationName, System.Action onComplete)
    {
        // Wait one frame for the animation to start
        yield return null;

        // Wait until the animation state is playing
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            yield return null;
        }

        // Wait until the animation is finished
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        onComplete?.Invoke();
    }

    public void MoveToSleep(System.Action onComplete = null)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveToSleepTransition(Pet.transform, BedTarget, onComplete));
    }

    private IEnumerator MoveToTargetAndReturn(Transform target, Vector3 destination, System.Action onComplete, PetMoveActivity activity)
    {
        Vector3 originalPosition = target.position;
        animator.SetTrigger("triggerOnWalk");
        while (Vector3.Distance(target.position, destination) > 0.01f)
        {

            target.position = Vector3.MoveTowards(target.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
        animator.SetTrigger("triggerOnWalkEnd");
        if (activity == PetMoveActivity.Study) { SetStudyState(true); }
        else { SetRestState(true); }

        yield return new WaitForSeconds(2f); // Wait at the destination for a moment.

        target.position = destination;

        if (activity == PetMoveActivity.Study) { SetStudyState(false); }
        else { SetRestState(false); }

        animator.SetTrigger("triggerOnWalk");
        while (Vector3.Distance(target.position, originalPosition) > 0.01f)
        {
            target.position = Vector3.MoveTowards(target.position, originalPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        animator.SetTrigger("triggerOnWalkEnd");
        target.position = originalPosition;

        onComplete?.Invoke();

    }

    private void SetStudyState(bool isStudying)
    {
        HamsterStudies.SetActive(isStudying);
        Hamster.SetActive(!isStudying);
    }

    private void SetRestState(bool isResting)
    {
        Hamster.SetActive(!isResting);

        if (isResting)
        {
            Bed.GetComponent<Image>().sprite = BedTaken;
        }
        else
        {
            Bed.GetComponent<Image>().sprite = BedEmpty;
        }
    }

    // Pet moves to bed and wakeup actions.

    public void PetWakeUp(System.Action onComplete = null)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        SetRestState(false);
        StartCoroutine(MoveToWakeUpTransition(Pet.transform, petStartPos, onComplete));

    }


    private IEnumerator MoveToWakeUpTransition(Transform target, Vector3 destination, System.Action onComplete = null)
    {
        animator.SetTrigger("triggerOnWalk");
        while (Vector3.Distance(target.position, destination) > 0.01f)
        {
            target.position = Vector3.MoveTowards(target.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
        animator.SetTrigger("triggerOnWalkEnd");
        target.position = destination;
        onComplete?.Invoke();

    }

    private IEnumerator MoveToSleepTransition(Transform target, Vector3 destination, System.Action onComplete)
    {
        animator.SetTrigger("triggerOnWalk");
        while (Vector3.Distance(target.position, destination) > 0.01f)
        {
            target.position = Vector3.MoveTowards(target.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
        animator.SetTrigger("triggerOnWalkEnd");
        target.position = destination;

        Bed.GetComponent<Image>().sprite = BedTaken;
        Hamster.SetActive(false);

        yield return new WaitForSeconds(0.2f); // Fix hide hamster bug.

        onComplete?.Invoke();
    }
}
