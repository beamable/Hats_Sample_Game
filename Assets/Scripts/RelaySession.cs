using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Experimental.Api.Sim;
using UnityEngine;

public class RelaySession : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public async Task JoinGame(string roomId)
    {
        var beam = await API.Instance;
        var sim = new SimClient(new SimNetworkEventStream(roomId), 20, 4);

        
    }
}
