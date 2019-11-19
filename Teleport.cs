using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;


public class Teleport : MonoBehaviour {
	Vector3 origin, direction;

	
	[TextArea]
	public string GUIDA = "Riempire l'array objToDisable con tutte le nuvole.\nLe nuvole dovranno essere figlie dell'isola corrispondente e avere il tag Nuvola.\nLo script va applicato al controller\nL'oggetto Aim è un oggetto vuoto figlio del controller con coordinate 0, 0, 1\n" +
		"La camera deve avere un piano davanti con materiale PlaneFade_Mat";

	[Header("Curve variables")]
	[Tooltip("Velocità abbassamento curva / distanza che può raggiungere la curva")]
	public Vector3 gravityDirection;

	[Tooltip("Oggetto imparentato al controller (NON TOCCARE)")]
	public GameObject aim;

	[Tooltip("Morbidezza della curva")]
	public int smoothness;

	RaycastHit hitInfo;

	[Tooltip("Numero massimo di Raycast della curva")]
	public float maxDistance;


	//LISTE DI VETTORI PER CURVA
	List<Vector3> points;
	Vector3[] linePointsArray;

	int pointsInList;

	[Space]
	[Tooltip("Punto di destinazione")]
	public GameObject destination;


	[Header("Camera rig")]
	[Tooltip("Caemra Rig")]
	public GameObject cameraRig;


	//TRANSFORM DELLE NUVOLE FIGLIE DELL'ISOLA SU CUI TELETRASPORTARSI
	Transform[] nuvole;

	//POSSO TELETRASPORTARMI
	bool posso = true;

	//MI STO TELETRASPORTANDO
	bool iAmTeleporting = false;

	//TRUE = FADEIN, FALSE = FADE OUT
	bool go_Back = true;

	//INDICE DELL'ARRAY DELLE NUVOLE FIGLIE - ISOLA SELEZIONATA
	int index;

	[Header("Piano davanti alla camera per FadeIn - FadeOut")]
	[Tooltip("Piano davanti alla camera ")]
	public GameObject plane;
	Material planeMat;
	
	
	[Header("Velocità Fade della Camera")]
	[Tooltip("Velocità FadeIn-FadeOut della camera durante il teleport")]
	[Range(0.05f, 0.1f)]
	public float lerpFadeCameraSens;

	//VALORE LERP - CAMBIA NEL TEMPO
	float lerpColor;

	public GameObject[] objToDisable;

	LineRenderer lineRenderer;

	private void Start() {
		lerpColor = lerpFadeCameraSens;
		//objToDisable = GameObject.FindGameObjectsWithTag("Nuvola");
		planeMat = plane.GetComponent<Renderer>().material;
		lineRenderer = GetComponent<LineRenderer>();
		//for (int i = 0; i < objToDisable.Length; i++) {
		//	objToDisable[i].SetActive(false);
		//}
	}


	public void Update() {

		//SE MI STO TELETRASPORTARMI
		if (iAmTeleporting) {
			
			if (go_Back) {//FADE IN
				lerpColor += lerpFadeCameraSens;
				planeMat.color = new Color(0, 0, 0, Mathf.Lerp(0 ,1, lerpColor));

				if (planeMat.color.a > 0.9f) {
					planeMat.color = new Color(0, 0, 0, 1);

					//TELEPORT
					cameraRig.transform.position = nuvole[index].transform.position;
					go_Back = false;
					lerpColor = lerpFadeCameraSens;
				}
			}
			else {//FADE OUT
				lerpColor += lerpFadeCameraSens;
				planeMat.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, lerpColor ));
				if (planeMat.color.a < 0.1f) {
					planeMat.color = new Color(0, 0, 0, 0);
					iAmTeleporting = false;
					go_Back = true;
				}
			}
		}
		else {
			if (!posso) {//SE NON POSSO TELETRASPORTARMI DISATTIVO IL LINE RENDERER
				lineRenderer.enabled = false;
			}

			//SE SMETTO DI TOCCARE IL TOUCHPAD DOPO IL TELEPORT, POSSO TELETRASPORTARMI DI NUOVO
			if ((Input.GetAxis("TouchPadX") == 0 && Input.GetAxis("TouchPadY") == 0) || (Input.GetAxis("TouchPadViveX") == 0 && Input.GetAxis("TouchPadViveY") == 0)) {
				posso = true;
			}

			//SE POSSO TELETRASPORTARMI VADO A GESTIRE GLI INPUT
			if (posso)
				if (Input.GetButton("Touch")) { // SE SFIORO IL TOUCHPAD CALCOLO LA CURVA

					destination.SetActive(true);
					lineRenderer.enabled = true;
					origin = transform.position;
					direction = aim.transform.position - origin;
					CurveCast(origin, direction, gravityDirection, smoothness, out hitInfo, maxDistance, out points);
					pointsInList = points.Count;

					lineRenderer.positionCount = pointsInList;
					lineRenderer.SetPositions(points.ToArray());

					destination.transform.position = hitInfo.point;

					//NON TOCCARE
					GameObject colpito;
					try {
						colpito = hitInfo.transform.gameObject;
					}
					catch (System.Exception e) {
						hitInfo.point = Vector3.zero;
						colpito = destination;
					}

					//IL NUMERO DI NUVOLE è UGUALE AL NUMERO DI FIGLI DELL'ISOLA COLPITA
					nuvole = new Transform[colpito.transform.childCount];
					

					//PRENDO FIGLI CON TAG NUVOLA
					List<Transform> nuvoleFiglie = new List<Transform>();
					for (int i = 0; i < colpito.transform.childCount; i++) {
						if (colpito.transform.GetChild(i).CompareTag("Nuvola")) {
							nuvoleFiglie.Add(colpito.transform.GetChild(i));
						}
					}
					//CONVERTO LISTA IN ARRAY
					nuvole = nuvoleFiglie.ToArray();


					//DIMENSIONE ARRAY DELLE DISTANZE = DIMENSIONE ARRAY NUVOLE FIGLIE DELL'ISOLA
					float[] distanze = new float[nuvole.Length];

					float minDist = 0;

					index = 0;

					//CALCOLO LE DISTANZE TRA IL PUNTO FINALE DELLA CURVA E LE ISOLA
					for (int i = 0; i < distanze.Length; i++) {
						distanze[i] = Vector3.Distance(hitInfo.point, nuvole[i].transform.position);

						//SE LA DISTANZA NON è ANCORA STATA CALCOLATA PRENDO LA PRIMA DELL'ARRAY
						if (minDist == 0) {
							minDist = distanze[i];
						}
						else {//CALCOLO DISTANZE E PRENDO LA MINORE
							if (distanze[i] < minDist) {
								minDist = distanze[i];
								index = i;
							}
						}
					}


					//objToDisable = GameObject.FindGameObjectsWithTag("Nuvola");

					//DISATTIVO TUTTE LE NUVOLE
					for (int i = 0; i < objToDisable.Length; i++) {
						objToDisable[i].SetActive(false);
					}

					//ATTIVO SOLO LA NUVOLA SU CUI ANDRò A TELETRASPORTARMI
					if (nuvole.Length > 0) {
						nuvole[index].gameObject.SetActive(true);
					}

					//SE PREMO IL TOUCHPAD E HO NUVOLE FIGLIE FACCIO INIZIARE IL FADE DELLA CAMERA CON CONSEGUENTE TELEPORT E FADE OUT
					if (Input.GetButton("TouchPress") && nuvole.Length > 0) {
						if (hitInfo.point != Vector3.zero) {
							posso = false;
							destination.SetActive(false);

							iAmTeleporting = true;
							lerpColor = lerpFadeCameraSens;

						}
					}
				}
				else {
					lineRenderer.enabled = false;
					hitInfo.point = Vector3.zero;
					
					
				}



			//TELEPORT POSSIBILE = CURVA VERDE
			//TELEPORT NON POSSIBILE = CURVA ROSSA
			if (hitInfo.point == Vector3.zero) {
				destination.SetActive(false);
				lineRenderer.SetColors(new Color(255, 0, 0, 0.25f), new Color(255, 0, 0, 0.25f));
			}
			else {
				lineRenderer.SetColors(new Color(0, 255, 0, 0), new Color(0, 255, 0, 0));
			}
		}
		

	}

	//CALCOLO CURVA NON TOCCARE
	public bool CurveCast(Vector3 origin, Vector3 direction, Vector3 gravityDirection, int smoothness, out RaycastHit hitInfo, float maxDistance, out List<Vector3> points) {
		if (maxDistance == Mathf.Infinity) maxDistance = 500;
		Vector3 currPos = origin, hypoPos = origin, hypoVel = direction.normalized / smoothness;
		List<Vector3> v = new List<Vector3>();
		RaycastHit hit;
		float curveCastLength = 0;

		do {
			v.Add(hypoPos);
			currPos = hypoPos;
			hypoPos = currPos + hypoVel + (gravityDirection * Time.fixedDeltaTime / (smoothness * smoothness));
			hypoVel = hypoPos - currPos;
			curveCastLength += hypoVel.magnitude;
		}
		while (UnityEngine.Physics.Raycast(currPos, hypoVel, out hit, hypoVel.magnitude) == false && curveCastLength < maxDistance);
		hitInfo = hit;

		points = v;
		return UnityEngine.Physics.Raycast(currPos, hypoVel, hypoVel.magnitude);
	}

}
