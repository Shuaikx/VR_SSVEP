using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EyeInteractable : MonoBehaviour
{
    // Start is called before the first frame update
    public bool IsHovered { get; set; }

    [SerializeField]
    private UnityEvent<GameObject> OnObjectHover;

    [SerializeField]
    private Material OnHoverActiveMaterial;
    [SerializeField]
    private Material OnHoverInActiveMaterial;
    private MeshRenderer meshRenderer;
    void Start() => meshRenderer = GetComponent<MeshRenderer>();

    // Update is called once per frame
    void Update()
    {
        if(IsHovered)
        {
            meshRenderer.material = OnHoverActiveMaterial;
            OnObjectHover?.Invoke(gameObject);
        }
        else
        {
            meshRenderer.material = OnHoverInActiveMaterial;
        }
    }
}
