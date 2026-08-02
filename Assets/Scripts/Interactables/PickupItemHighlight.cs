using UnityEngine;

namespace Alien.Interactables
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PickupItem))]
    public class PickupItemHighlight : MonoBehaviour
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Transform visualsRoot;
        [SerializeField, ColorUsage(false, false)] private Color highlightTint = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField, Range(0f, 1f)] private float tintMix = 0.5f;
        [SerializeField, Min(0f)] private float emissionIntensity = 2f;

        private PickupItem pickupItem;
        private Renderer[] visualRenderers;
        private Material[][] originalMaterials;
        private Material[][] highlightedMaterials;

        private void Awake()
        {
            pickupItem = GetComponent<PickupItem>();

            if (visualsRoot == null)
            {
                Debug.LogError("PickupItemHighlight needs a visuals root.", this);
                return;
            }

            visualRenderers = visualsRoot.GetComponentsInChildren<Renderer>(true);
            originalMaterials = new Material[visualRenderers.Length][];

            for (int i = 0; i < visualRenderers.Length; i++)
                originalMaterials[i] = visualRenderers[i].sharedMaterials;
        }

        private void OnEnable()
        {
            pickupItem.TargetedChanged += SetHighlighted;
            SetHighlighted(pickupItem.IsTargeted);
        }

        private void OnDisable()
        {
            pickupItem.TargetedChanged -= SetHighlighted;
            SetHighlighted(false);
        }

        private void OnDestroy()
        {
            DestroyHighlightedMaterials();
        }

        private void SetHighlighted(bool isHighlighted)
        {
            if (visualRenderers == null) return;

            if (isHighlighted && highlightedMaterials == null)
                CreateHighlightedMaterials();

            Material[][] materials = isHighlighted ? highlightedMaterials : originalMaterials;

            for (int i = 0; i < visualRenderers.Length; i++)
            {
                if (visualRenderers[i] != null)
                    visualRenderers[i].sharedMaterials = materials[i];
            }
        }

        private void CreateHighlightedMaterials()
        {
            highlightedMaterials = new Material[originalMaterials.Length][];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                highlightedMaterials[i] = new Material[originalMaterials[i].Length];

                for (int j = 0; j < originalMaterials[i].Length; j++)
                {
                    Material originalMaterial = originalMaterials[i][j];

                    if (originalMaterial != null)
                        highlightedMaterials[i][j] = CreateHighlightedMaterial(originalMaterial);
                }
            }
        }

        private Material CreateHighlightedMaterial(Material originalMaterial)
        {
            Material highlightedMaterial = new Material(originalMaterial)
            {
                name = $"{originalMaterial.name} (Highlighted)",
                hideFlags = HideFlags.DontSave
            };

            Color mixedColor = highlightTint;

            if (highlightedMaterial.HasProperty(BaseColorProperty))
                mixedColor = SetTint(highlightedMaterial, BaseColorProperty);
            else if (highlightedMaterial.HasProperty(ColorProperty))
                mixedColor = SetTint(highlightedMaterial, ColorProperty);
            else
                Debug.LogWarning($"Material '{originalMaterial.name}' has no _BaseColor or _Color property.", this);

            SetEmission(highlightedMaterial, mixedColor);

            return highlightedMaterial;
        }

        private Color SetTint(Material material, int colorProperty)
        {
            Color originalColor = material.GetColor(colorProperty);
            Color tintedColor = Color.Lerp(originalColor, highlightTint, tintMix);
            tintedColor.a = originalColor.a;

            material.SetColor(colorProperty, tintedColor);
            return tintedColor;
        }

        private void SetEmission(Material material, Color mixedColor)
        {
            if (!material.HasProperty(EmissionColorProperty)) return;

            Color emissionColor = mixedColor * emissionIntensity;
            emissionColor.a = mixedColor.a;

            material.SetColor(EmissionColorProperty, emissionColor);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }

        private void DestroyHighlightedMaterials()
        {
            if (highlightedMaterials == null) return;

            foreach (Material[] rendererMaterials in highlightedMaterials)
            {
                foreach (Material material in rendererMaterials)
                {
                    if (material == null) continue;

                    if (Application.isPlaying)
                        Destroy(material);
                    else
                        DestroyImmediate(material);
                }
            }

            highlightedMaterials = null;
        }
    }
}
