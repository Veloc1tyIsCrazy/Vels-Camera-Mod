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
    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public class Cam : BaseUnityPlugin
    {
        private GameObject CameraHolder = null;
        private GameObject CameraHolder2 = null;
        private bool init = true;
        private float fov;
        private int Mode;
        private bool CanModeChange;
        private bool IsLocked;
        private bool IsPlaced;
        private bool CanPlace;
        private float LockDelay;
        public static GameObject ThirdPersonCameraGO;
        public static GameObject CMVirtualCameraGO;
        public static GameObject Rhand;
        public static GameObject Lhand;
        public static Material CamMat = new Material(Shader.Find("GorillaTag/UberShader"));
        CinemachineVirtualCamera CMVirtualCamera;
        Camera ThirdPersonCamera;

        void LateUpdate()
        {

            if (GorillaLocomotion.Player.Instance != null)
            {
                ThirdPersonCameraGO = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
                CMVirtualCameraGO = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1");
                CMVirtualCamera = CMVirtualCameraGO.GetComponent<CinemachineVirtualCamera>();
                ThirdPersonCamera = ThirdPersonCameraGO.GetComponent<Camera>();
                Lhand = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/palm.01.L");
                Rhand = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R");
            }

            // camera setup
            CMVirtualCamera.enabled = false;
            ThirdPersonCamera.fieldOfView = fov;

            init = false;


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

            if (ControllerInputPoller.instance.rightControllerPrimary2DAxis.y > 0.5f && IsLocked == false)
            {
                fov--;
            }
            if (ControllerInputPoller.instance.rightControllerPrimary2DAxis.y < -0.5f && IsLocked == false)
            {
                fov++;
            }



            if (CameraHolder == null)
            {
                CameraHolder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                CameraHolder.name = "FPC";
                CameraHolder.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                GameObject.Destroy(CameraHolder.GetComponent<Collider>());
                GameObject.Destroy(CameraHolder.GetComponent<BoxCollider>());
                GameObject.Destroy(CameraHolder.GetComponent<Rigidbody>());
                CameraHolder.GetComponent<Renderer>().enabled = false;
                CameraHolder.GetComponent<Renderer>().material = CamMat;
                CamMat.color = Color.black;
            }

            if (CameraHolder != null)
            {
                if (Mode == 0 && IsPlaced == false)
                {
                    CameraHolder.transform.SetParent(null);
                    CameraHolder.transform.rotation = GorillaLocomotion.Player.Instance.headCollider.transform.rotation;
                    CameraHolder.transform.position = GorillaLocomotion.Player.Instance.headCollider.transform.position;
                    ThirdPersonCameraGO.transform.position = CameraHolder.transform.position;
                    ThirdPersonCameraGO.transform.rotation = CameraHolder.transform.rotation;
                    CameraHolder.GetComponent<Renderer>().enabled = false;
                }
                if (Mode == 1 && IsPlaced == false)
                {
                    CameraHolder.transform.SetParent(Rhand.transform);
                    CameraHolder.transform.localPosition = new Vector3(-0.1f, 0.0f, 0.0f);
                    CameraHolder.transform.localRotation = Quaternion.Euler(0f, 270f, 90f);
                    ThirdPersonCameraGO.transform.position = CameraHolder.transform.position;
                    ThirdPersonCameraGO.transform.rotation = CameraHolder.transform.rotation;
                    CameraHolder.GetComponent<Renderer>().enabled = true;
                }

                if (Mode == 2 && IsPlaced == false)
                {
                    CameraHolder.transform.SetParent(Lhand.transform);
                    CameraHolder.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
                    CameraHolder.transform.localPosition = new Vector3(0.0f, 0.0f, 0.1f);
                    ThirdPersonCameraGO.transform.position = CameraHolder.transform.position;
                    ThirdPersonCameraGO.transform.rotation = CameraHolder.transform.rotation;
                    CameraHolder.GetComponent<Renderer>().enabled = true;
                }
                if(IsPlaced == true)
                {
                    CameraHolder.transform.SetParent(null);
                    CameraHolder.GetComponent<Renderer>().enabled = true;
                }
                
            }


        }


        void Awake()
        {
            fov = 60;
            Mode = 0;
        }
    }
}