using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpButton : MonoBehaviour
{
    [SerializeField] [TextArea(minLines:3, maxLines:15)] string title;
    [SerializeField] [TextArea(minLines:3, maxLines:15)] string message;
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => {
            PopUp.CreatePopUp(title, message);
        });
    }
}
