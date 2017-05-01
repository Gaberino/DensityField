using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour {

	public static HudManager Instance;

	bool[] tStates;
	public Color activeColor;
	public Color inActiveColor;

	public Image[] tetherHudImages;

	// Use this for initialization
	void Start () {
		Instance = this;
		tStates = new bool[10];
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < SpaceBroPlayer.Instance.pTethers.Count; i++){
			tStates[i] = SpaceBroPlayer.Instance.pTethers[i].tetherState;

			if (tStates[i]){
				tetherHudImages[i].color = activeColor;
			}
			else {
				tetherHudImages[i].color = inActiveColor;
			}
		}
	}

	public void RefreshAvailableTethers(){
		for (int i = 0; i < SpaceBroPlayer.Instance.tetherCapacity; i ++){
			if (!tetherHudImages[i].gameObject.activeSelf){
				tetherHudImages[i].gameObject.SetActive(true);
			}
		}
	}
}
