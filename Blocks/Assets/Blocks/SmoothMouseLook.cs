// slightly tweaked from http://wiki.unity3d.com/index.php/SmoothMouseLook
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Blocks
{
    public class SmoothMouseLook : MonoBehaviour
    {

        public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
        public RotationAxes axes = RotationAxes.MouseXAndY;
        public float sensitivityX = 15F;
        public float sensitivityY = 15F;

        public float minimumX = -360F;
        public float maximumX = 360F;

        public float minimumY = -60F;
        public float maximumY = 60F;

        float rotationX = 0F;
        float rotationY = 0F;

        private List<float> rotArrayX = new List<float>();
        float rotAverageX = 0F;

        private List<float> rotArrayY = new List<float>();
        float rotAverageY = 0F;

        public float frameCounter = 20;

        Quaternion originalRotation;
        public bool allowedToCapture = true;
        public bool capturing = false;
        public bool prevCapturing = false;
        void Update()
        {
            capturing = allowedToCapture;

            if (!capturing)
            {
                World.mainWorld.blocksWorld.blockRenderCanvas.transform.Find("Cursor").GetComponent<UnityEngine.UI.Image>().enabled = false;
            }
            else
            {
                World.mainWorld.blocksWorld.blockRenderCanvas.transform.Find("Cursor").GetComponent<UnityEngine.UI.Image>().enabled = true;
            }
            if (!allowedToCapture)
            {
                capturing = false;
                prevCapturing = false;
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                }
                if (!Cursor.visible)
                {
                    Cursor.visible = true;
                }
                return;
            }


            //if (Input.GetMouseButtonDown(0))
            //{
            //    capturing = true;
            //}
            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    capturing = false;
            //}
            if (!capturing)
            {
                if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                return;
            }
            if (capturing)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                }
            }
            if (axes == RotationAxes.MouseXAndY)
            {
                rotAverageY = 0f;
                rotAverageX = 0f;

                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;

                rotationY = ClampAngle(rotationY, minimumY, maximumY);
                rotationX = ClampAngle(rotationX, minimumX, maximumX);
                rotArrayY.Add(rotationY);
                rotArrayX.Add(rotationX);

                if (rotArrayY.Count >= frameCounter)
                {
                    rotArrayY.RemoveAt(0);
                }
                if (rotArrayX.Count >= frameCounter)
                {
                    rotArrayX.RemoveAt(0);
                }

                for (int j = 0; j < rotArrayY.Count; j++)
                {
                    rotAverageY += rotArrayY[j];
                }
                for (int i = 0; i < rotArrayX.Count; i++)
                {
                    rotAverageX += rotArrayX[i];
                }

                rotAverageY /= rotArrayY.Count;
                rotAverageX /= rotArrayX.Count;

                rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
                rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

                Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
                Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

                transform.localRotation = originalRotation * xQuaternion * yQuaternion;
            }
            else if (axes == RotationAxes.MouseX)
            {
                rotAverageX = 0f;

                rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                rotationX = ClampAngle(rotationX, minimumX, maximumX);

                rotArrayX.Add(rotationX);

                if (rotArrayX.Count >= frameCounter)
                {
                    rotArrayX.RemoveAt(0);
                }
                for (int i = 0; i < rotArrayX.Count; i++)
                {
                    rotAverageX += rotArrayX[i];
                }
                rotAverageX /= rotArrayX.Count;

                rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

                Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
                transform.localRotation = originalRotation * xQuaternion;
            }
            else
            {
                rotAverageY = 0f;

                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = ClampAngle(rotationY, minimumY, maximumY);

                rotArrayY.Add(rotationY);

                if (rotArrayY.Count >= frameCounter)
                {
                    rotArrayY.RemoveAt(0);
                }
                for (int j = 0; j < rotArrayY.Count; j++)
                {
                    rotAverageY += rotArrayY[j];
                }
                rotAverageY /= rotArrayY.Count;

                rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);

                Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
                transform.localRotation = originalRotation * yQuaternion;
            }
        }

        void Start()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb)
                rb.freezeRotation = true;
            originalRotation = transform.localRotation;
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            angle = angle % 360;
            if ((angle >= -360F) && (angle <= 360F))
            {
                if (angle < -360F)
                {
                    angle += 360F;
                }
                if (angle > 360F)
                {
                    angle -= 360F;
                }
            }
            return Mathf.Clamp(angle, min, max);
        }
    }
}