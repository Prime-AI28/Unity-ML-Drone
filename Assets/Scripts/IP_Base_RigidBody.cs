using UnityEngine;

namespace prime
{
    [RequireComponent (typeof(Rigidbody))]
    public class IP_Base_RigidBody : MonoBehaviour
    {
        #region Variable
        [Header("Rigidbody Properties")]

        [SerializeField] private float weightInKg = .454f;

        protected Rigidbody rb;

        protected float startDrag;
        protected float startAngularDrag;

        #endregion


        #region Main Methods
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb)
            {
                rb.mass = weightInKg;
                startDrag = rb.drag;
                startAngularDrag = rb.angularDrag;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!rb)
            {
                return;
            }

            HandlePhysics();
        }
        #endregion


        #region Custom Methods

        protected virtual void HandlePhysics()
        {

        }

        #endregion
    }
}
