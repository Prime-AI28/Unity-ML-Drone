using UnityEngine;
using UnityEngine.Rendering;

namespace prime
{
    [RequireComponent(typeof(BoxCollider))]
    public class IP_Drone_Engine : MonoBehaviour, IEngine
    {
        #region Variables

        [Header("Engine Properties")]
        [SerializeField] private float maxPower = 4f;

        [Header("Propeller Properties")]
        [SerializeField] private Transform propeller;
        [SerializeField] private float propRotSpeed = 100;

        #endregion

        #region Interface Methods
        public void InitEngine()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateEngine(Rigidbody rb, IP_Drone_Inputs input)
        {
            //Debug.Log("Running Eninge: " + gameObject.name);
            Vector3 upVec = transform.up;

            upVec.x = 0f;
            upVec.z = 0f;

            float diff = 1 - upVec.magnitude;
            float finalDiff = Physics.gravity.magnitude * diff;

            Vector3 engineforce = Vector3.zero;
            engineforce = transform.up * ((rb.mass*Physics.gravity.magnitude + finalDiff) + (input.Throttle * maxPower)) / 4f;

            rb.AddForce(engineforce, ForceMode.Force);
            HandlePropellers();
        }

        void HandlePropellers()
        {
            if (!propeller)
            {
                return;
            }

            propeller.Rotate(Vector3.forward, propRotSpeed);
        }
        #endregion
    }
}
