using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Mediapipe.Unity.PoseTracking
{
    public class MotionProxy
    {
        private static MotionProxy _Instance;

        public static MotionProxy GetInstance() 
        { 
            // If there is an instance, and it's not me, delete myself.
            
            if (_Instance == null) 
            { 
                _Instance = new MotionProxy();
            }
            return _Instance;
        }


        NormalizedLandmarkList poseLandmarks;

        // Start is called before the first frame update
        void Start()
        {
            // 



        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void SetPoseLandmark(NormalizedLandmarkList val)
        {
            if (val == null) return;
            poseLandmarks = val;


        }

        public float GetHorizontalMove()
        {
            if(poseLandmarks == null) return 0.0f;

            float pos = poseLandmarks.Landmark[15].X;

            if (pos >= 0.6f)
            {
                return -1.0f;
            } 
            else if(pos <= 0.4f)
            {
                return +1.0f;
            }
            else
            {
                return 0.0f;
            }



            return 1.0f;


            // 15
        }


    }
}
