using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VARLab.Navigation.Samples
{
    public class Rotator : MonoBehaviour
    {
        void Update()
        {
            transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
        }
    }
}
