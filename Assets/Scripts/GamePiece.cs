using System.Collections;
using UnityEngine;

public class GamePiece : MonoBehaviour{

	[SerializeField]
	public float _animTime = 2f; //made it public so that the AI can reference this and move after the animation is done
	[SerializeField]
	AnimationCurve _growthCurve;

	private void OnEnable()
	{
		StartCoroutine(SpawnRoutine());
	}

	IEnumerator SpawnRoutine(){
		yield return null;
		for(float t = 0 ; t <= _animTime; t += Time.deltaTime){
			yield return new WaitForFixedUpdate();
			transform.localScale = Vector3.one * _growthCurve.Evaluate( t/_animTime);
		}
	}


}
