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
        private float LockDelay;
        public static GameObject ThirdPersonCameraGO;
        public static GameObject CMVirtualCameraGO;
        public static Material CamMat = new Material (Shader.Find("GorillaTag/UberShader"));
        CinemachineVirtualCamera CMVirtualCamera;
        Camera ThirdPersonCamera;

        void LateUpdate()
        {

            if(GorillaLocomotion.Player.Instance != null)
            {
                ThirdPersonCameraGO = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
                CMVirtualCameraGO = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1");
                CMVirtualCamera = CMVirtualCameraGO.GetComponent<CinemachineVirtualCamera>();
                ThirdPersonCamera = ThirdPersonCameraGO.GetComponent<Camera>();
            }

            // camera setup
            CMVirtualCamera.enabled = false;
            ThirdPersonCamera.fieldOfView = fov;

            init = false;


            if (Mode > 1)
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
                if (Mode == 0)
                {
                    CameraHolder.transform.rotation = GorillaLocomotion.Player.Instance.headCollider.transform.rotation;
                    CameraHolder.transform.position = GorillaLocomotion.Player.Instance.headCollider.transform.position;
                    ThirdPersonCameraGO.transform.position = CameraHolder.transform.position;
                    ThirdPersonCameraGO.transform.rotation = CameraHolder.transform.rotation;
                }
                if (Mode == 1)
                {
                    CameraHolder.GetComponent<Renderer>().enabled = true;
                }
                CameraHolder2.transform.position = CameraHolder.transform.position + new Vector3(0.3f, 0, 0);
                CameraHolder2.transform.rotation = CameraHolder.transform.rotation;

            }
        

    }


        void Awake()
        {
            fov = 60;
            Mode = 0;
        }
    }
}
