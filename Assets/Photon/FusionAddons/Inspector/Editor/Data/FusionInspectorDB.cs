namespace Fusion.Addons.Inspector.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Profiling;
	using UnityEditor;
	using Fusion.Statistics;

	internal sealed class FusionInspectorDB : ScriptableObject
	{
		public int                 RTTMs;
		public TimeSpan            SessionTime;
		public float               InBandwidth;
		public float               OutBandwidth;
		public PlayerRef           LocalPlayer;
		public NetworkRunner       CurrentRunner;
		public NetworkRunner       PendingRunner;
		public List<NetworkRunner> Runners      = new List<NetworkRunner>();
		public Components          Components   = new Components();
		public SceneObjects        SceneObjects = new SceneObjects();
		public Prefabs             Prefabs      = new Prefabs();

		public int CurrentSection => _currentSection;

		[SerializeField]
		private int                 _currentSection;
		private int                 _lastFrameCount;
		private float               _sessionTime;
		private NetworkRunner       _sessionRunner;
		private List<NetworkObject> _tempObjects = new List<NetworkObject>();

		public void Update()
		{
			bool synchronize = _lastFrameCount != Time.frameCount;

			Profiler.BeginSample($"{nameof(FusionInspectorDB)}.{nameof(Update)}");

			UpdateRunners();
			UpdateSession();
			UpdateComponents(synchronize);
			UpdateSceneObjects(synchronize);
			UpdatePrefabs(synchronize);

			if (synchronize == true)
			{
				Components.ProcessAccumulatedStateChanges();
			}

			Profiler.EndSample();

			_lastFrameCount = Time.frameCount;
		}

		public void ResetSession()
		{
			RTTMs        = default;
			SessionTime  = default;
			InBandwidth  = default;
			OutBandwidth = default;

			_sessionTime   = Time.unscaledTime;
			_sessionRunner = CurrentRunner;

			foreach (ComponentInfo component in Components.All)
			{
				component.ClearStateChanges();
			}

			foreach (SceneObjectInfo sceneObject in SceneObjects.All)
			{
				sceneObject.ClearStateChanges();
			}
		}

		public void SetCurrentSection(int currentSection)
		{
			_currentSection = currentSection;

			EditorPrefs.SetInt($"{nameof(FusionInspectorDB)}.{nameof(_currentSection)}", currentSection);
		}

		private void Awake()
		{
			_currentSection = EditorPrefs.GetInt($"{nameof(FusionInspectorDB)}.{nameof(_currentSection)}", 0);

			Components.LoadEditorPrefs();
			SceneObjects.LoadEditorPrefs();
			Prefabs.LoadEditorPrefs();
		}

		private void UpdateRunners()
		{
			LocalPlayer = PlayerRef.None;

			Runners.Clear();

			List<NetworkRunner>.Enumerator runnersEnumerator = NetworkRunner.GetInstancesEnumerator();
			while (runnersEnumerator.MoveNext() == true)
			{
				NetworkRunner runner = runnersEnumerator.Current;
				if (runner.IsRunning == true)
				{
					Runners.Add(runnersEnumerator.Current);
				}
			}

			if (PendingRunner != default)
			{
				CurrentRunner = PendingRunner;
				PendingRunner = default;
			}

			if (Runners.Contains(CurrentRunner) == false)
			{
				CurrentRunner = Runners.Count > 0 ? Runners[0] : default;
			}

			if (CurrentRunner != null && CurrentRunner.IsRunning == true)
			{
				LocalPlayer = CurrentRunner.LocalPlayer;
			}
		}

		private void UpdateSession()
		{
			if (_sessionRunner != CurrentRunner)
			{
				ResetSession();
			}

			if (_sessionRunner == null)
				return;

			RTTMs       = (int)(_sessionRunner.GetPlayerRtt(_sessionRunner.LocalPlayer) * 1000.0);
			SessionTime = TimeSpan.FromSeconds(Time.unscaledTime - _sessionTime);

			if (_sessionRunner.TryGetFusionStatistics(out FusionStatisticsManager statisticsManager) == true && statisticsManager.CompleteSnapshot != null)
			{
				InBandwidth  += statisticsManager.CompleteSnapshot.InBandwidth;
				OutBandwidth += statisticsManager.CompleteSnapshot.OutBandwidth;
			}
		}

		private void UpdateComponents(bool synchronize)
		{
			if (synchronize == true)
			{
				Components.Synchronize();
			}

			Components.Refresh();
		}

		private void UpdateSceneObjects(bool synchronize)
		{
			if (synchronize == true)
			{
				_tempObjects.Clear();

				if (CurrentRunner != null)
				{
					NetworkRunnerUtility.GetAllNetworkObjects(CurrentRunner, _tempObjects);
				}
				else
				{
					UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsByType(typeof(NetworkObject), FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
					for (int i = 0, count = objects.Length; i < count; ++i)
					{
						_tempObjects.Add((NetworkObject)objects[i]);
					}
				}

				Vector3 distanceOrigin    = Vector3.zero;
				bool    hasDistanceOrigin = false;

				if (Application.isPlaying == true)
				{
					if (LocalPlayer.IsRealPlayer == true && CurrentRunner != null)
					{
						if (CurrentRunner.TryGetPlayerObject(LocalPlayer, out NetworkObject playerObject) == true)
						{
							distanceOrigin    = playerObject.transform.position;
							hasDistanceOrigin = true;
						}
					}

					if (hasDistanceOrigin == false)
					{
						Camera camera = Camera.main;
						if (camera != null)
						{
							distanceOrigin    = camera.transform.position;
							hasDistanceOrigin = true;
						}
					}
				}

				if (hasDistanceOrigin == false)
				{
					SceneView lastActiveSceneView = SceneView.lastActiveSceneView;
					if (lastActiveSceneView != null)
					{
						Camera camera = lastActiveSceneView.camera;
						if (camera != null)
						{
							distanceOrigin    = camera.transform.position;
							hasDistanceOrigin = true;
						}
					}
				}

				SceneObjects.Synchronize(Components, _tempObjects, LocalPlayer, distanceOrigin, hasDistanceOrigin, Application.isPlaying);

				_tempObjects.Clear();
			}

			SceneObjects.Refresh(Components);
		}

		private void UpdatePrefabs(bool synchronize)
		{
			if (synchronize == true)
			{
				_tempObjects.Clear();

				NetworkPrefabTable prefabTable = NetworkProjectConfig.Global.PrefabTable;
				for (int i = 0, count = prefabTable.Prefabs.Count; i < count; ++i)
				{
					_tempObjects.Add(prefabTable.Load(NetworkPrefabId.FromIndex(i), true));
				}

				Prefabs.Synchronize(Components, _tempObjects);

				_tempObjects.Clear();
			}

			Prefabs.Refresh(Components);
		}
	}
}
