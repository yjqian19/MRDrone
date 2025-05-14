using UnityEngine;
using System.Text.RegularExpressions;

public class Functions : MonoBehaviour
{
    // Singleton reference
    public static Functions Instance { get; private set; }

    public Transform redCubeTransform;
    public Transform droneTransform;

    void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
            }

    public void RotateDroneToFace(Transform userTransform)
    {
        Vector3 direction = userTransform.position - droneTransform.position;
        if (direction != Vector3.zero)
        {
            direction.y = 0; // Ignore vertical rotation
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            droneTransform.rotation = lookRotation;
        }
    }

    public void MoveDroneToFront(Transform userTransform)
    {
        Vector3 newPosition = userTransform.position + userTransform.forward * 2f;
        droneTransform.position = newPosition;
    }

    public void MoveDrone(Vector3 targetPosition)
    {
        droneTransform.position = targetPosition;
    }

    public Transform GetDroneTransform()
    {
        return droneTransform;
    }

    public Transform GetCubeTransform()
    {
        return redCubeTransform;
    }
}
