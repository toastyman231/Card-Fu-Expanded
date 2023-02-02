using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputFieldCapitalization : MonoBehaviour
{
    private TMP_InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValidateInput +=
            delegate (string s, int i, char c) { return char.ToUpper(c); };
    }
}
