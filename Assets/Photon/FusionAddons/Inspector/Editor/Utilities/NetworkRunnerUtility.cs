﻿namespace Fusion.Addons.Inspector.Editor
{
	using System.Collections.Generic;
	using System.Reflection;

	internal static class NetworkRunnerUtility
	{
		private static FieldInfo _instanceFieldInfo   = typeof(NetworkObjectMeta).GetField("Instance", BindingFlags.Instance | BindingFlags.NonPublic);
		private static FieldInfo _simulationFieldInfo = typeof(NetworkRunner).GetField("_simulation", BindingFlags.Instance | BindingFlags.NonPublic);

		public static void GetAllNetworkObjects(NetworkRunner runner, List<NetworkObject> networkObjects)
		{
			networkObjects.Clear();

			Simulation simulation = _simulationFieldInfo.GetValue(runner) as Simulation;
			if (simulation == null)
				return;

			Dictionary<NetworkId, NetworkObjectMeta> objects = simulation.Objects;
			if (objects == null)
				return;

			foreach (KeyValuePair<NetworkId, NetworkObjectMeta> kvp in objects)
			{
				NetworkObject networkObject = _instanceFieldInfo.GetValue(kvp.Value) as NetworkObject;
				if (networkObject != null)
				{
					networkObjects.Add(networkObject);
				}
			}
		}
	}
}
