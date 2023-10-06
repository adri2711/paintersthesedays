using UnityEngine;

namespace Util
{
    public class BrushHairColorChanger : MonoBehaviour
    {

        [SerializeField] private Color _newColor;
    
        private MeshRenderer _meshRenderer;
    
        // Start is called before the first frame update
        void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            ChangeEdgeColor();
        }

        private void ChangeEdgeColor()
        {
            foreach (Material material in _meshRenderer.materials)
            {
                if (material.name == "Not Fixed Brush Hair (Instance)")
                {
                    material.color = _newColor;
                }
            }
        }
    }
}
