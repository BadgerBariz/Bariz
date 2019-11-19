using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	
	[TextArea]
	public string README = "E' necessario il Tag Terrain da assegnare al piano inclinato posto sotto la mappa, o cambia lo script";
	
	[Space]
	[Space]

	[Tooltip("Camera")]
	public GameObject mainCamera;

	[Tooltip("Velocità lerp (0 = fermo; 1 = no smooth)")]
	public float cameraLerpSensitivity;

	[Tooltip("Limiti schermo per pan con mouse: centro(0,0)")]
    [Header("MOUSE LIMITS")]
    //LIMITE PER SPOSTAMENTO CON MOUSE
    public float screenLimit;


	[Tooltip("Oggetto che muove la camera")]
    [Header("OBJ THAT MOVES CAMERA ")]
    //OGGETTO CAMERA
    public GameObject camera;

	[Tooltip("Limiti di movimento della camera")]
	[Header("MOVEMENT LIMITS")]
    //LIMITI DI MOVIMENTO CAMERA
    public float min_X;
    public float max_X;
    public float min_Z;
    public float max_Z;


	[Header("ZOOM LIMITS")]
	//LIMIT ZOOM
	[Tooltip("Minimo altezza camera")]
	public float minCameraHeight;
	[Tooltip("Massimo altezza camera")]
	public float maxCameraHeight;


    [Header("SPEEDS AND SENSITIVITIES")]
	//VELOCITA' MOVIMENTO
	[Tooltip("Velocità movimento camera")]
	public float panMouseSpeed;


	//QUANTITA' ZOOM
	[Tooltip("Quantità di zoom con uno scatto di rotazione della rotella del mouse")]
	public float zoomJump;
    
    [Tooltip("Velocità lerp camera (POSSIBILMENTE NON TOCCARE)")]
    public float movementSensitivity;

    [Header("DISTANCE FROM TERRAIN AND CENTER OF VIEW")]
	//DISTANZA TERRENO
	[Tooltip("Distanza Terreno - Center of view")]
	public float terrainDistance;

	

    [Header("PAN")]
	[Tooltip("Inversione assi pan (true = come maya)")]
	public bool InvertedPan;
    int panDirection;
	[Tooltip("Velocità pan")]
	public float panSpeed;

	

    [Header("ROTATION")]
	[Tooltip("Inversione assi rotazione")]
	public bool invertedRotation;
    int rotationDirection;
	[Tooltip("Velocità rotazione visuale")]
	public float rotationSpeed;
	


    [Header("ZOOM")]
	[Tooltip("Inversine assi zoom (SOLO CON ALT E TASTO DESTRO)")]
	public bool invertedZoom;
	[Tooltip("Velocità zoom (SOLO CON ALT E TASTO DESTRO)")]
	public float zoomSpeed; //SOLO CON ALT + DX

    // Update is called once per frame ;)
    void Update() {
		
		mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, camera.transform.position, cameraLerpSensitivity);
		mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, camera.transform.rotation, cameraLerpSensitivity);

		//PAN NORMALE O INVERTITO
		if (InvertedPan) {
            panDirection = -1;
        }
        else {
            panDirection = 1;
        }

        //ROTAZIONE NORMALE O INVERTITA
        if (invertedRotation) {
            rotationDirection = -1;
        }
        else {
            rotationDirection = 1;
        }
        

        //RAYCAST PUNTO CHE SEGUE LA CAMERA- TERRENO
        RaycastHit raycastHit;
        Ray ray = new Ray(camera.transform.position, new Vector3(-camera.transform.up.x, -camera.transform.up.y - 20, -camera.transform.up.z));
        
        Debug.DrawRay(camera.transform.position, new Vector3(-camera.transform.up.x, -camera.transform.up.y - 20, -camera.transform.up.z), Color.yellow);
        
        if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity)) {
            if (raycastHit.collider.CompareTag("Terrain")) {
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, raycastHit.point.y + terrainDistance, transform.position.z), movementSensitivity);
            }
        }
		

        //RIMAPPO IN 0 - 1
        Vector2 mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        //RIMAPPO IN -1 - 1
        mousePosition = new Vector2(reMap(mousePosition.x, 0, 1, -1, 1), reMap(mousePosition.y, 0, 1, -1, 1));
        //print(mousePosition.x + "  " + mousePosition.y);


        //ROTAZIONE CON MIDDLE MOUSE BUTTON O ALT + MOUSE SX
        if (!Input.GetKey(KeyCode.LeftAlt) && (Input.GetMouseButton(2))  || (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0))) {
            transform.Rotate(0, Input.GetAxis("Mouse X") * rotationSpeed * rotationDirection, 0);
            cameraLerpSensitivity = 1;
        }
        else {
            cameraLerpSensitivity = 0.2f;
        }


        //ZOOM INVERTITO RETELLA
        if (invertedZoom) {
            //ZOOM IN CON ROTELLINA MOUSE UP
            if (Input.mouseScrollDelta.y == -1 && Vector3.Distance(camera.transform.position, transform.position) > minCameraHeight) { //SCROLL UP --> INDIETRO
                camera.transform.Translate(Vector3.forward * zoomJump);
            }

            //ZOOM OUT CON ROTELLINA MOUSE DOWN
            else if (Input.mouseScrollDelta.y == 1 && Vector3.Distance(camera.transform.position, transform.position) < maxCameraHeight) { //SCROLL DOWN --> AVANTI
                camera.transform.Translate(Vector3.forward * (-zoomJump));
            }
        }

        //ZOOM NORMALE ROTELLA
        else {
            //ZOOM IN CON ROTELLINA MOUSE DOWN
            if (Input.mouseScrollDelta.y == 1 && Vector3.Distance(camera.transform.position, transform.position) > minCameraHeight) { //SCROLL UP --> AVANTI
                camera.transform.Translate(Vector3.forward * zoomJump);
            }

            //ZOOM OUT CON ROTELLINA MOUSE UP
            else if (Input.mouseScrollDelta.y == -1 && Vector3.Distance(camera.transform.position, transform.position) < maxCameraHeight) { //SCROLL DOWN --> INVERTITO

                camera.transform.Translate(Vector3.forward * (-zoomJump));
            }

        }

        //ZOOM INVERTITO CON ALT + TASTO DESTRO
        if (invertedZoom) {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(1) && Input.mouseScrollDelta.y == 0) {
                if (Input.GetAxis("Mouse X") > 0 && Vector3.Distance(camera.transform.position, transform.position) < maxCameraHeight) {
                    camera.transform.Translate(Vector3.forward * Input.GetAxis("Mouse X") * (-zoomSpeed));
                }
                else if (Input.GetAxis("Mouse X") < 0 && Vector3.Distance(camera.transform.position, transform.position) > minCameraHeight) {
                    camera.transform.Translate(Vector3.forward * Input.GetAxis("Mouse X") * (-zoomSpeed));
                }
            }
        }

        //ZOOM NORMALE CON ALT + TASTO DESTRO
        else {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(1) && Input.mouseScrollDelta.y == 0) {
                if (Input.GetAxis("Mouse X") < 0 && Vector3.Distance(camera.transform.position, transform.position) < maxCameraHeight) {
                    camera.transform.Translate(Vector3.forward * Input.GetAxis("Mouse X") * zoomSpeed);
                }
                else if (Input.GetAxis("Mouse X") > 0 && Vector3.Distance(camera.transform.position, transform.position) > minCameraHeight) {
                    camera.transform.Translate(Vector3.forward * Input.GetAxis("Mouse X") * zoomSpeed);
                }
            }
        }
        
		
        //SPOSTAMENTO CON ALT + MIDDLE MOUSE BUTTON

        /*else*/ if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(2)) {
            transform.Translate(panMouseSpeed * Time.deltaTime * Input.GetAxis("Mouse X") * panDirection * panSpeed, 0, panMouseSpeed * Time.deltaTime * Input.GetAxis("Mouse Y") * panDirection * panSpeed);
        }

        //SPOSTAMENTO CON MOUSE(NESSUN TASTO PREMUTO)
        else {
            if (!Input.GetMouseButton(2) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetMouseButton(0)) {
                
                if (mousePosition.x < -screenLimit) {
                    transform.Translate(-panMouseSpeed * Time.deltaTime, 0, 0);
                }
                if (mousePosition.x > screenLimit) {
                    transform.Translate(panMouseSpeed * Time.deltaTime, 0, 0);
                }
                if (mousePosition.y < -screenLimit) {
                    transform.Translate(0, 0, -panMouseSpeed * Time.deltaTime);
                }
                if (mousePosition.y > screenLimit) {
                    transform.Translate(0, 0, panMouseSpeed * Time.deltaTime);
                }

                transform.position = new Vector3(Mathf.Clamp(transform.position.x, min_X, max_X), transform.position.y, Mathf.Clamp(transform.position.z, min_Z, max_Z));
            }
                
        }
		
    }


    
    float reMap(float s, float a1, float a2, float b1, float b2) {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
