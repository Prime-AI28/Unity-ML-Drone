using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

namespace prime
{
    [RequireComponent(typeof(IP_Drone_Inputs))]
    public class IP_Drone_Agent : Agent
    {
        #region Variables

        [Header("Control Properties")]
        [SerializeField] private float minMaxPitch = 30f;
        [SerializeField] private float minMaxRoll = 30f;
        [SerializeField] private float yawPower = 4.0f;
        [SerializeField] private float lerpSpeed = 2f;

        [Header("Start and End Positions")]
        [SerializeField] private Transform startTransform;
        [SerializeField] private Transform endTransform;
        [SerializeField] private List<GameObject> targetObjects = new List<GameObject>(); // New list for target objects

        [SerializeField] private Material win;
        [SerializeField] private Material loose;
        [SerializeField] private MeshRenderer floor;

        [Header("Reward Parameters")]
        public RewardParameters rewardParams;

        [SerializeField] private float targetReachedThreshold = 1f;   // Distance to consider target reached
        [SerializeField] private float maxDistanceBeforePenalty = 100f; // Distance before applying outOfBoundsPenalty

        private IP_Drone_Inputs input;
        private List<IP_Drone_Engine> engines = new List<IP_Drone_Engine>();
        private float finalYaw;
        private float finalPitch;
        private float finalRoll;
        private Rigidbody rb;

        private float initialDistanceToTarget; // Store initial distance
        private float previousDistanceToTarget; // Store previous distance to target
        private TrailManager trailManager;
        #endregion

        #region Unity Methods

        public override void Initialize()
        {
            trailManager = GetComponent<TrailManager>();
            rb = GetComponent<Rigidbody>();
            input = GetComponent<IP_Drone_Inputs>();
            engines = GetComponentsInChildren<IP_Drone_Engine>().ToList();
        }

        public override void OnEpisodeBegin()
        {
            // Reset drone to starting position
            if (startTransform != null)
            {
                transform.position = startTransform.position;
                transform.rotation = startTransform.rotation;
            }
            else
            {
                Debug.LogWarning("Start Transform is not assigned. Using default position.");
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
            }

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            endTransform = null;
            // Select random target and assign it to endTransform
            SelectRandomTarget();

            // Randomize target position and calculate initial distance
            if (endTransform != null)
            {
                //float randomX = Random.Range(-40f, 40f);
                //float randomY = Random.Range(5f, 80f);
                //float randomZ = Random.Range(-40f, 40f);
                //endTransform.position = new Vector3(randomX, randomY, randomZ);
                initialDistanceToTarget = Vector3.Distance(transform.position, endTransform.position);
                previousDistanceToTarget = initialDistanceToTarget;
            }
            else
            {
                initialDistanceToTarget = 0f;
                previousDistanceToTarget = 0f;
                Debug.LogWarning("End Transform is not assigned. Initial distance cannot be calculated.");
            }
            trailManager.ResetTrail();
        }

        private void SelectRandomTarget()
        {
            if (targetObjects.Count == 0)
            {
                Debug.LogWarning("Target objects list is empty. Cannot select a target.");
                return;
            }

            // Disable all targets first
            foreach (GameObject target in targetObjects)
            {
                target.SetActive(false);
            }

            // Select a random target
            int randomIndex = Random.Range(0, targetObjects.Count);
            GameObject selectedTarget = targetObjects[randomIndex];

            // Enable the selected target and assign its transform
            selectedTarget.SetActive(true);
            endTransform = selectedTarget.transform;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(transform.position);  // Drone's world position
            sensor.AddObservation(transform.rotation);  // Drone's rotation
            sensor.AddObservation(rb.velocity);        // Linear velocity
            sensor.AddObservation(rb.angularVelocity);  // Angular velocity

            // Normalized velocity observations
            sensor.AddObservation(Normalization.Sigmoid(rb.velocity, 0.25f));
            sensor.AddObservation(Normalization.Sigmoid(rb.angularVelocity));

            if (endTransform != null)
            {
                // Relative position to target (normalized)
                Vector3 relativePosition = transform.InverseTransformPoint(endTransform.position);
                sensor.AddObservation(Normalization.Sigmoid(relativePosition, 5f));
            }
            else
            {
                Debug.LogWarning("End Transform is not assigned. Using zero vector.");
                sensor.AddObservation(Vector3.zero);
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // Process continuous actions
            float pitchInput = actions.ContinuousActions[0] * minMaxPitch;
            float rollInput = -actions.ContinuousActions[1] * minMaxRoll;
            float yawInput = actions.ContinuousActions[2] * yawPower;
            float throttleInput = actions.ContinuousActions[3];

            input.SetThrottle(throttleInput);

            // Smoothly update rotation
            finalYaw += yawInput * Time.deltaTime;
            finalPitch = Mathf.Lerp(finalPitch, pitchInput, Time.deltaTime * lerpSpeed);
            finalRoll = Mathf.Lerp(finalRoll, rollInput, Time.deltaTime * lerpSpeed);

            Quaternion rotation = Quaternion.Euler(finalPitch, finalYaw, finalRoll);
            rb.MoveRotation(rotation); // Physics-based rotation

            // Update engines
            foreach (var engine in engines)
            {
                engine.UpdateEngine(rb, input);
            }

            CalculateRewards();

            if (Input.GetKeyDown(KeyCode.Y))
            {
                SetReward(-100f);
                Debug.Log("Episode manually terminated with -100 penalty");
                floor.material = loose;
                EndEpisode();
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActions = actionsOut.ContinuousActions;
            continuousActions[0] = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f; // Pitch
            continuousActions[1] = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f; // Roll
            continuousActions[2] = Input.GetKey(KeyCode.LeftArrow) ? -1f : Input.GetKey(KeyCode.RightArrow) ? 1f : 0f; // Yaw
            continuousActions[3] = Input.GetKey(KeyCode.UpArrow) ? 1f : Input.GetKey(KeyCode.DownArrow) ? -1f : 0f; // Throttle
        }

        private void OnCollisionEnter(Collision collision)
        {
            SetReward(rewardParams.collisionPenalty);
            Debug.Log("Crashed");
            floor.material = loose;
            EndEpisode();
        }

        #endregion

        #region Reward Calculation

        private void CalculateRewards()
        {
            if (endTransform == null) return;

            float currentDistanceToTarget = Vector3.Distance(transform.position, endTransform.position);

            // Distance reward
            float distanceReduction = previousDistanceToTarget - currentDistanceToTarget;
            float distanceReward = 0;
            if (distanceReduction > 0) distanceReward = distanceReduction * rewardParams.distanceRewardMultiplier;
            else distanceReward = distanceReduction * rewardParams.distancePenaltyMultiplier;
            AddReward(distanceReward);

            // Time penalty
            AddReward(rewardParams.timePenalty);

            // Orientation reward
            float orientationReward = transform.up.y * rewardParams.orientationRewardScale;
            AddReward(orientationReward);

            // Velocity penalty
            float velocityPenalty = rb.velocity.magnitude * rewardParams.velocityDampingPenaltyScale;
            AddReward(velocityPenalty);

            // Angular velocity penalty
            float angularVelocityPenalty = rb.angularVelocity.magnitude * rewardParams.angularVelocityPenaltyScale;
            AddReward(angularVelocityPenalty);

            // Alignment reward
            Vector3 directionToTarget = (endTransform.position - transform.position).normalized;
            float alignmentReward = Vector3.Dot(transform.forward, directionToTarget) * rewardParams.alignmentRewardMultiplier;
            AddReward(alignmentReward);

            // Check for target reached
            if (currentDistanceToTarget < targetReachedThreshold)
            {
                SetReward(rewardParams.reachedTargetReward);
                Debug.Log("Mission completed");
                floor.material = win;
                EndEpisode();
            }
            // Check for out of bounds
            else if (currentDistanceToTarget > maxDistanceBeforePenalty)
            {
                SetReward(rewardParams.outOfBoundsPenalty);
                EndEpisode();
            }

            // Height penalty
            float h = transform.position.y;
            if (h < rewardParams.desiredHeightMin)
            {
                float deviation = rewardParams.desiredHeightMin - h;
                float penalty = -rewardParams.heightPenaltyScale * Mathf.Exp(rewardParams.heightPenaltyExponent * deviation);
                AddReward(penalty);
            }
            else if (h > rewardParams.desiredHeightMax)
            {
                float deviation = h - rewardParams.desiredHeightMax;
                float penalty = -rewardParams.heightPenaltyScale * Mathf.Exp(rewardParams.heightPenaltyExponent * deviation);
                AddReward(penalty);
            }

            // Update previous distance
            previousDistanceToTarget = currentDistanceToTarget;
        }

        #endregion
    }

    [System.Serializable]
    public class RewardParameters
    {
        public float distanceRewardMultiplier = 0.5f;       // Reward for reducing distance
        public float distancePenaltyMultiplier = 1.5f;      // Reward for reducing distance
        public float reachedTargetReward = 100.0f;         // Reward for reaching target
        public float collisionPenalty = -10.0f;            // Penalty for collisions
        public float timePenalty = -0.01f;                 // Penalty per step
        public float outOfBoundsPenalty = -5.0f;           // Penalty for going too far
        public float orientationRewardScale = 0.1f;        // Reward for staying upright
        public float angularVelocityPenaltyScale = -0.05f; // Penalty for spinning
        public float velocityDampingPenaltyScale = -0.01f; // Penalty for high speed
        public float alignmentRewardMultiplier = 0.1f;     // Reward for facing target

        // New height-related parameters
        public float desiredHeightMin = 60f;               // Minimum preferred height
        public float desiredHeightMax = 70f;               // Maximum preferred height
        public float heightPenaltyScale = 0.01f;           // Scaling factor for height penalty
        public float heightPenaltyExponent = 0.05f;        // Exponent for exponential penalty
    }
}