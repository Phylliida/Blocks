using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DurabilityBar : MonoBehaviour {

    public RectTransform healthBar;
    float _durability;
    public float durability
    {
        get
        {
            return _durability;
        }
        set
        {
            _durability = value;
            healthBar.transform.localScale = new Vector3(value, 1, 1);
        }
    }

    private void OnEnable()
    {
        foreach (UnityEngine.UI.Image child in GetComponentsInChildren<UnityEngine.UI.Image>())
        {
            child.enabled = true;
        }
    }

    private void OnDisable()
    {
        foreach (UnityEngine.UI.Image child in GetComponentsInChildren<UnityEngine.UI.Image>())
        {
            child.enabled = false;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
