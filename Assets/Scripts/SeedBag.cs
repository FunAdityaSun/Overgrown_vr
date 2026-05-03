using UnityEngine;
using System.Collections;
using Fusion;

public class SeedBag : NetworkBehaviour
{
    public NetworkPrefabRef plantPrefab;
    public int uses = 3;
    public float shrinkAmt = 33f;

    private Vector3 initialScale;
    private Vector3 initialColliderSize;
    private BoxCollider seedCollider;

    void Start()
    {
        shrinkAmt = 100f / uses;
        initialScale = transform.localScale;
        seedCollider = GetComponent<BoxCollider>();
        if (seedCollider != null)
        {
            initialColliderSize = seedCollider.size;
        }
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

        Vector3 startColliderSize = seedCollider.size;
        float targetScale = Mathf.Max(endScale.x / initialScale.x, 0.01f);
        Vector3 endColliderSize = initialColliderSize / targetScale;

        float duration = 0.25f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);

            if (seedCollider != null)
            {
                seedCollider.size = Vector3.Lerp(startColliderSize, endColliderSize, elapsed / duration);
            }

            yield return null;
        }
        transform.localScale = endScale; 

        if (uses <= 0)
        {
            Destroy(gameObject);
        }

    }
}
