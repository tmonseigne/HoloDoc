using UnityEngine;
using UnityEngine.XR.WSA;

public class SpatialMapping : MonoBehaviour {

	public static SpatialMapping Instance { private set; get; }

	[HideInInspector] public static int PhysicsRaycastMask;

	[Tooltip("The material to use when rendering Spatial Mapping data.")]
	public Material DrawMaterial;

	[Tooltip("If true, the Spatial Mapping data will be rendered.")]
	public bool drawVisualMeshes = false;

	// If true, Spatial Mapping will be enabled. 
	private bool _mappingEnabled = true;

	// The layer to use for spatial mapping collisions.
	private int _physicsLayer = 31;

	// Handles rendering of spatial mapping meshes.
	private SpatialMappingRenderer _spatialMappingRenderer;

	// Creates/updates environment colliders to work with physics.
	private SpatialMappingCollider _spatialMappingCollider;

	/// <summary>
	/// Determines if the spatial mapping meshes should be rendered.
	/// </summary>
	public bool DrawVisualMeshes {
		get { return drawVisualMeshes; }
		set {
			drawVisualMeshes = value;

			if (drawVisualMeshes) {
				_spatialMappingRenderer.visualMaterial = DrawMaterial;
				_spatialMappingRenderer.renderState = SpatialMappingRenderer.RenderState.Visualization;
			}
			else {
				_spatialMappingRenderer.renderState = SpatialMappingRenderer.RenderState.None;
			}
		}
	}

	/// <summary>
	/// Enables/disables spatial mapping rendering and collision.
	/// </summary>
	public bool MappingEnabled {
		get { return _mappingEnabled; }
		set {
			_mappingEnabled = value;
			_spatialMappingCollider.freezeUpdates = !_mappingEnabled;
			_spatialMappingRenderer.freezeUpdates = !_mappingEnabled;
			gameObject.SetActive(_mappingEnabled);
		}
	}

	void Awake() {
		Instance = this;
	}

	// Use this for initialization
	void Start() {
		_spatialMappingRenderer = gameObject.GetComponent<SpatialMappingRenderer>();
		_spatialMappingRenderer.surfaceParent = this.gameObject;
		_spatialMappingCollider = gameObject.GetComponent<SpatialMappingCollider>();
		_spatialMappingCollider.surfaceParent = this.gameObject;
		_spatialMappingCollider.layer = _physicsLayer;
		PhysicsRaycastMask = 1 << _physicsLayer;
		DrawVisualMeshes = drawVisualMeshes;
		MappingEnabled = _mappingEnabled;
	}
}