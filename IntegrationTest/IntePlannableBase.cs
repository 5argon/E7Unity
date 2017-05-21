using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IntegrationLevel {
	Quick,
	Timed,
	Full
}

public abstract class IntePlannableBase<T> : E7InteBase where T : class {

    [Header("Plannable Settings")]
	public IntegrationLevel integrationLevel;
	public float timedIntegrationLength;

	protected abstract IEnumerator BeforeEach();
	protected abstract IEnumerator Unit(T config);
	protected abstract IEnumerator AfterEach();

	protected abstract T[] AllConfigs();

	protected abstract float CalculateTime(T config);

	protected T RandomConfig
	{
		get{
			T[] allConfigs = AllConfigs();
			int random = Mathf.RoundToInt(Random.value * (allConfigs.Length-1));
			return allConfigs[random];
		}
	}

	protected T RandomOtherConfig(T currentConfig)
	{
        T random = RandomConfig;
        while (random == currentConfig)
        {
            random = RandomConfig;
        }
        return random;
    }

	protected override IEnumerator InteRoutine()
	{
        T[] allConfigs = AllConfigs();

		switch(integrationLevel)
		{
			case IntegrationLevel.Quick:
			{
				int random = Mathf.RoundToInt(Random.value * (allConfigs.Length-1));
				//Debug.Log(random);
				//Debug.Log(allConfigs.Length);
				yield return RunUnit(allConfigs[random]);
				break;
			}
			case IntegrationLevel.Timed:
			{
				break;
			}
			case IntegrationLevel.Full:
			{
				foreach(T config in allConfigs)
				{
					yield return RunUnit(config);
				}
				break;
			}
		}
		IntegrationTest.Pass();
		yield break;
	}

	private IEnumerator RunUnit(T config)
	{
		yield return StartCoroutine(BeforeEach());
		yield return StartCoroutine(Unit(config));
		yield return StartCoroutine(AfterEach());
	}
}
