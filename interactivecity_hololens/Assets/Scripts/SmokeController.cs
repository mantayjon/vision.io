using Unity.VisualScripting;
using UnityEngine;

public class SmokeController : MonoBehaviour
{
    public ParticleSystem whiteSmokeParticles;
    public ParticleSystem blackSmokeParticles;
    public HoloDebugger debug;

    private bool isSmokeWhite = true; 
    private int smokeRateOverTime;
    public int maxSmokeRate;

    public EnergySectorController energySector;
    public IndustrySectorController industrySector;
 

    void Start()
    {
       
        blackSmokeParticles.Play();
        whiteSmokeParticles.Stop();
    }
   
    void Update()
    {
        //SetSmokeRateOverTime();
        UpdateSmokeState();
    }

    private void SetSmokeRateOverTime()
    {
        if (energySector != null)
        {
            smokeRateOverTime = (int)((energySector.GetSliderValue() * maxSmokeRate));
            var emission = whiteSmokeParticles.emission;
            emission.rateOverTime = smokeRateOverTime;
        }
        

    }
  
    private void UpdateSmokeState()
    {
        if (industrySector != null)
        {
            isSmokeWhite = industrySector.GetButtonState();

            if (isSmokeWhite)
            {
                whiteSmokeParticles.Play();
                blackSmokeParticles.Stop();
            }
            else
            {
                blackSmokeParticles.Play();
                whiteSmokeParticles.Stop();
            }
        }

    }


}
