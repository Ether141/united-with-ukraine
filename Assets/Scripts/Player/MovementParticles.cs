using UnityEngine;

public class MovementParticles : MonoBehaviour
{
    [SerializeField] private GameObject landParticles;

    private PlayerController controller;

    private void Start()
    {
        controller = GetComponent<PlayerController>();

        controller.OnLand += SpawnLandParticle;
    }

    private void SpawnLandParticle()
    {
        GameObject particle = Instantiate(landParticles, new Vector3(transform.position.x, transform.position.y - controller.StartCharacterBounds.extents.y, 1f), landParticles.transform.rotation);
        Destroy(particle, 2f);
    }
}
