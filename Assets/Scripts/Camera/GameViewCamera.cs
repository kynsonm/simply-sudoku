using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class GameViewCamera : MonoBehaviour
{
    [System.Serializable]
    public class CameraClass {
        public Camera camera;
        public float nearPlaneDistance;
        public float farPlaneDistance;
    }


    public GameObject MainCameraObject;
    public GameObject SizeReferenceMenu;
    RectTransform menuRect;

    public List<CameraClass> cameras;
    float nearestDistance = int.MaxValue, farestDIstance = int.MinValue;

    public bool AlwaysUpdate;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        SetUpCameras();
    }

    void SetUpCameras() {
        GetObjects();
        GetCameraSize();
        StartCoroutine(GetCameraSize());
    }

    void GetObjects() {
        MainCameraObject = gameObject;
        menuRect = SizeReferenceMenu.GetComponent<RectTransform>();
        foreach (CameraClass cam in cameras) {
            if (cam.nearPlaneDistance < nearestDistance) { nearestDistance = cam.nearPlaneDistance; }
            if (cam.farPlaneDistance > farestDIstance)   { farestDIstance = cam.farPlaneDistance; }
        }
        nearestDistance = (nearestDistance < -1000) ? -1000 : nearestDistance;
        farestDIstance = (farestDIstance > 3000) ? 3000 : farestDIstance;
    }

    IEnumerator GetCameraSize() {
        // Set position of the camera
        Vector3 pos = MainCameraObject.GetComponent<Transform>().position;
        pos.x = Screen.width / 2;
        pos.y = Screen.height / 2;
        pos.z = 0;
        MainCameraObject.GetComponent<Transform>().position = pos;

        Camera cam = MainCameraObject.GetComponent<Camera>();
        cam.nearClipPlane = nearestDistance;
        cam.farClipPlane = farestDIstance;

        // Set orthographic size of the camera
        float orthoSize;
        Debug.Log("Size ref size == " + menuRect.sizeDelta.ToString());
        orthoSize = Mathf.Abs(menuRect.sizeDelta.y / 2f);
        Camera.main.orthographicSize = orthoSize;

        foreach (CameraClass cameraClass in cameras) {
            Camera camera = cameraClass.camera;
            if (!camera.orthographic) { continue; }
            camera.nearClipPlane = cameraClass.nearPlaneDistance;
            camera.farClipPlane = cameraClass.farPlaneDistance;
            camera.orthographicSize = orthoSize;
        }

        yield return new WaitForSeconds(1f);
    }
}
