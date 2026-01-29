using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [Header("Settings")]
    public float maxDistance = 50f;
    public LayerMask grappleLayer;
    public float grappleSpeed = 20f;
    public float spring = 4.5f;
    public float damper = 7f;
    public float massScale = 4.5f;
    [Range(0, 1)]
    public float maxDistanceMultiplier = 0.8f;
    [Range(0, 1)]
    public float minDistanceMultiplier = 0.25f;
    [Header("References")]
    public LineRenderer lineRenderer;
    public Transform shootPoint;
    
    private Vector3 grapplePoint;
    private bool isGrappling;
    private SpringJoint joint;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }

    public void StartGrapple()
    {
        // Debug.Log("Grapple!");
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance, grappleLayer))
        {
            Debug.Log($"Grapple hit {hit.collider.gameObject.name} at {hit.point}");
            grapplePoint = hit.point;
            isGrappling = true;

            joint = rb.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(rb.position, grapplePoint);

            // ODM Gear feel: tight but bouncy
            joint.maxDistance = distanceFromPoint * maxDistanceMultiplier;
            joint.minDistance = distanceFromPoint * minDistanceMultiplier;

            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;

            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2;
            }
        }
    }

    public void StopGrapple()
    {
        isGrappling = false;
        if (joint != null)
        {
            Destroy(joint);
        }
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (isGrappling && lineRenderer != null)
        {
            lineRenderer.SetPosition(0, shootPoint.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }

    public bool IsGrappling() => isGrappling;
    public Vector3 GetGrapplePoint() => grapplePoint;
}
