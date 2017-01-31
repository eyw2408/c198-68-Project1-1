using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckingController : MonoBehaviour {

    GameObject superMario;

	// Use this for initialization
	void Awake () {
        superMario = GameObject.Find("Super Mario");
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.S)) {
            superMario.SetActive(true);
            superMario.transform.position = new Vector3(this.transform.position.x, superMario.transform.position.y);
            Vector3 scale = this.gameObject.transform.localScale;
            scale.x = scale.x * Mathf.Sign(scale.x);
            this.gameObject.transform.localScale = scale;
            this.gameObject.SetActive(false);
        }		
	}
}
