using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using HarmonyLib;
using BepInEx;
using System.Buffers.Text;
using Cinemachine;
using UnityEngine.XR;

namespace CameraMod
{
    [BepInPlugin("com.Veloc1ty.CameraMod", "Vels Camera", "1.0.0")]

    public class Cam : BaseUnityPlugin
    {
        private GameObject CameraHolder = null;
        private GameObject CameraHolder2 = null;
        private GameObject CameraHolder3 = null;
        private GameObject CamDisplay = null;
        private bool init;
        private float fov;
        private int Mode;
        private bool CanModeChange;
        private bool IsLocked;
        private bool IsPlaced;
        private bool CanParent;
        private bool CanPlace;
        private float LockDelay;
        private static RenderTexture DisplayTex;
        private float Smoothing = 0.05f;
        public static GameObject ThirdPersonCameraGO;
        public static GameObject CMVirtualCameraGO;
        public static GameObject Rhand;
        public static GameObject Lhand;
        public static Material CamMat = new Material(Shader.Find("GorillaTag/UberShader"));
        public static Material CamMat2 = new Material(Shader.Find("GorillaTag/UberShader"));
        public static Material CamMat3 = new Material(Shader.Find("GorillaTag/UberShader"));
        CinemachineVirtualCamera CMVirtualCamera;
        Camera ThirdPersonCamera;

        void LateUpdate()
        {

            if (GorillaLocomotion.Player.Instance != null && init == false)
            {
                ThirdPersonCameraGO = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
                CMVirtualCameraGO = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1");
                CMVirtualCamera = CMVirtualCameraGO.GetComponent<CinemachineVirtualCamera>();
                ThirdPersonCamera = ThirdPersonCameraGO.GetComponent<Camera>();
                Lhand = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/palm.01.L");
                Rhand = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R");
                CanParent = true;
                init = true;

            }

            // camera setup
            CMVirtualCamera.enabled = false;
            ThirdPersonCamera.fieldOfView = fov;
            

           


            if (Mode > 2)
            {
                Mode = 0;
            }


            if (ControllerInputPoller.instance.rightControllerPrimaryButton && CanModeChange == false && IsLocked == false)
            {
                Mode++;
                CanModeChange = true;
            }
            if (!ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                CanModeChange = false;
            }
            if (!ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                CanPlace = false;
            }
            if (ControllerInputPoller.instance.rightControllerSecondaryButton && CanPlace == false && IsLocked == false)
            {
                IsPlaced = !IsPlaced;
                CanPlace = true;
            }



            if (ControllerInputPoller.TriggerFloat(XRNode.RightHand) == 1f && ControllerInputPoller.TriggerFloat(XRNode.LeftHand) == 1f && Time.time > LockDelay + 0.4f)
            {
                IsLocked = !IsLocked;
                LockDelay = Time.time;
                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);
                GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);


            }
            if (ControllerInputPoller.TriggerFloat(XRNode.RightHand) != 1f && ControllerInputPoller.TriggerFloat(XRNode.LeftHand) != 1f)
            {

            }

            if (ControllerInputPoller.instance.rightControllerPrimary2DAxis.y > 0.5f && IsLocked == false && fov > 30)
            {
                fov--;
            }
            if (ControllerInputPoller.instance.rightControllerPrimary2DAxis.y < -0.5f && IsLocked == false && fov < 150)
            {
                fov++;
            }



            if (CameraHolder == null)
            {
                CameraHolder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                CameraHolder.name = "FPC";
                CameraHolder.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                GameObject.Destroy(CameraHolder.GetComponent<Collider>());
                GameObject.Destroy(CameraHolder.GetComponent<BoxCollider>());
                GameObject.Destroy(CameraHolder.GetComponent<Rigidbody>());
                CameraHolder.GetComponent<Renderer>().enabled = false;
                CameraHolder.GetComponent<Renderer>().material = CamMat;
                CamMat.color = GorillaTagger.Instance.offlineVRRig.playerColor;


                CameraHolder2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                CameraHolder2.name = "FPC2";
                CameraHolder2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                GameObject.Destroy(CameraHolder2.GetComponent<Collider>());
                GameObject.Destroy(CameraHolder2.GetComponent<BoxCollider>());
                GameObject.Destroy(CameraHolder2.GetComponent<Rigidbody>());
                CameraHolder2.GetComponent<Renderer>().enabled = false;
                CameraHolder2.GetComponent<Renderer>().material = CamMat;
                CamMat.color = Color.black;

                CameraHolder3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                CameraHolder3.name = "FPC3";
                CameraHolder3.transform.localScale = new Vector3(0.05f, 0.05f, 0.08f);
                GameObject.Destroy(CameraHolder3.GetComponent<Collider>());
                GameObject.Destroy(CameraHolder3.GetComponent<BoxCollider>());
                GameObject.Destroy(CameraHolder3.GetComponent<Rigidbody>());
                CameraHolder3.GetComponent<Renderer>().enabled = true;
                CameraHolder3.GetComponent<Renderer>().material = CamMat2;
                CamMat2.color = Color.white;
            }

            if (CameraHolder != null)
            {
                if (Mode == 0 && IsPlaced == false)
                {
                    CameraHolder.transform.SetParent(null);
                    CameraHolder.transform.LookAt(CameraHolder2.transform);
                    CameraHolder.transform.position = GorillaLocomotion.Player.Instance.headCollider.transform.position;
                   
                    CameraHolder.GetComponent<Renderer>().enabled = false;
                    CameraHolder3.GetComponent<Renderer>().enabled = false;
                }
                if (Mode == 1 && IsPlaced == false)
                {
                    CameraHolder.transform.SetParent(Rhand.transform);
                    CameraHolder.transform.localPosition = new Vector3(-0.1f, 0.0f, 0.0f);
                    CameraHolder.transform.localRotation = Quaternion.Euler(0f, 270f, 90f);
                    CameraHolder.GetComponent<Renderer>().enabled = true;
                    CameraHolder3.GetComponent<Renderer>().enabled = true;
                }

                if (Mode == 2 && IsPlaced == false)
                {
                    CameraHolder.transform.SetParent(Lhand.transform);
                    CameraHolder.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
                    CameraHolder.transform.localPosition = new Vector3(0.0f, 0.0f, 0.1f);
                    CameraHolder.GetComponent<Renderer>().enabled = true;
                    CameraHolder3.GetComponent<Renderer>().enabled = true;
                }
                if(IsPlaced == true)
                {
                    CameraHolder.transform.SetParent(null);
                    CameraHolder.GetComponent<Renderer>().enabled = true;
                    CameraHolder3.GetComponent<Renderer>().enabled = true;
                }
                if (ThirdPersonCameraGO == GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera"))
                {
                    ThirdPersonCameraGO.transform.SetParent(CameraHolder.transform);
                    ThirdPersonCameraGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
                CameraHolder2.transform.SetParent(GorillaLocomotion.Player.Instance.headCollider.transform);
                CameraHolder3.transform.SetParent(CameraHolder.transform);
                CameraHolder3.transform.localPosition = new Vector3(0f, 0, 0.3f);
                ThirdPersonCameraGO.transform.localPosition = Vector3.zero;
                CameraHolder2.transform.localPosition = new Vector3(0,0,1);
 
                
            }


        }


        void Awake()
        {
            fov = 60;
            Mode = 0;
        }
    }
}