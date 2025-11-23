using System.Collections;
using UnityEngine;

public class TwoDimensionalTextureAnimation : MonoBehaviour
{
    [SerializeField] private float secondsBtwnNextAnimation;
    [SerializeField] private Material[] animationMaterials;
    private MeshRenderer meshRenderer;
    private Material[] currentMaterial;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        currentMaterial = meshRenderer.materials;
    }

    private void OnEnable()
    {
        StartCoroutine(AnimateMaterials(secondsBtwnNextAnimation));
    }

    private IEnumerator AnimateMaterials(float seconds)
    {
        int index = 0;

        while (true)
        {
            // Guard if array is empty
            if (animationMaterials.Length > 0)
            {
                currentMaterial[0] = animationMaterials[index];
                meshRenderer.materials = currentMaterial;  // assign back so renderer updates

                // advance to next frame (looping)
                index = (index + 1) % animationMaterials.Length;
            }

            yield return new WaitForSeconds(seconds);
        }
    }
}
