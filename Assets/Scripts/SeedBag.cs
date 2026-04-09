using UnityEngine;
using System.Collections;

public class SeedBag : MonoBehaviour
{
    public GameObject plantPrefab;
    public int uses = 3;
    public float shrinkAmt = 33f;

    void Start()
    {
        shrinkAmt = 100f / uses;
    }
    void Update()
    {
        
    }
    public void use()
    {
        uses--;
        Debug.Log(uses);
        StartCoroutine(Animate(shrinkAmt));
    }
    IEnumerator Animate(float amount)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale - new Vector3(amount, amount, amount);
        if (endScale.x < 0) endScale = Vector3.zero;
        float duration = 0.25f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            yield return null;
        }
        transform.localScale = endScale; 

        if (uses <= 0)
        {
            Destroy(gameObject);
        }

    }
}
