using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TallyParticleEffect : MonoBehaviour
{
    public Vector2 targetPosition;            //it will move toward the visual tally 

    private void Update()
    {
        if (targetPosition != null)
        {
            this.transform.position = Vector2.MoveTowards(this.transform.position, targetPosition, 50 * Time.deltaTime);
            if (Vector2.Distance(this.transform.position, targetPosition) < .2f) Destroy(this.gameObject);
        }
    }
}
