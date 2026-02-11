using UnityEngine;
using UnityEngine.Rendering;

public class ForceSorting : MonoBehaviour
{
    private void Awake()
    {
        GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;

        GraphicsSettings.transparencySortAxis = new Vector3(0, 1, 0);
    }
}
