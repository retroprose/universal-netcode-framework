using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebugHit : MonoBehaviour
{
    public int Index;
    //public Games.Hurkle.Cp Components;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*ref var body = ref Components.Bodies[Index];
        ref var fixture = ref Components.Fixtures[Index];

        transform.localPosition = new UnityEngine.Vector3((float)(body.position.x + fixture.position.x), (float)(body.position.y + fixture.position.y), 0.0f);
        transform.localScale = new UnityEngine.Vector3((float)fixture.size.x, (float)fixture.size.y, 1.0f);*/
    }

    private void OnDrawGizmos()
    {
       /* Gizmos.color = Color.red;
        //Gizmos.DrawCube(transform.position, transform.lossyScale);

        var p = transform.position;
        var s = transform.lossyScale;

        //var s = new Vector3(32, 32, 0);

        Gizmos.DrawLine(new Vector3(p.x - s.x, p.y - s.y, 0.0f), new Vector3(p.x + s.x, p.y - s.y, 0.0f));
        Gizmos.DrawLine(new Vector3(p.x - s.x, p.y - s.y, 0.0f), new Vector3(p.x - s.x, p.y + s.y, 0.0f));

        Gizmos.DrawLine(new Vector3(p.x + s.x, p.y + s.y, 0.0f), new Vector3(p.x + s.x, p.y - s.y, 0.0f));
        Gizmos.DrawLine(new Vector3(p.x + s.x, p.y + s.y, 0.0f), new Vector3(p.x - s.x, p.y + s.y, 0.0f));*/


        //Debug.Log($"{transform.position.x}, {transform.position.y} - {transform.lossyScale.x}, {transform.lossyScale.y}");
    }
}
