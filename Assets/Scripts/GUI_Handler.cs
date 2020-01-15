using UnityEngine;
using UnityEngine.UI;

public class GUI_Handler : MonoBehaviour
{
    public static GUI_Handler Instance { get; private set; }

    enum State
    {
        ON = 0,
        OFF
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }
}
