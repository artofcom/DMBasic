using UnityEngine;
using System;
using System.Collections;
using Spine.Unity;
using Spine;

public class CustomSpineBone : SpineAttributeBase 
{
		/// <summary>
		/// Smart popup menu for Spine Bones
		/// </summary>
		/// <param name="startsWith">Filters popup results to elements that begin with supplied string.</param>
		/// /// <param name="includeNone">If true, the dropdown list will include a "none" option which stored as an empty string.</param>
		/// <param name="dataField">If specified, a locally scoped field with the name supplied by in dataField will be used to fill the popup results.
		/// Valid types are SkeletonDataAsset and SkeletonRenderer (and derivatives)
		/// If left empty and the script the attribute is applied to is derived from Component, GetComponent<SkeletonRenderer>() will be called as a fallback.
		/// </param>
	public CustomSpineBone(string startsWith = "", string dataField = "", bool includeNone = true) {
			this.startsWith = startsWith;
			this.dataField = dataField;
			this.includeNone = includeNone;
		}

	public static Spine.Bone GetBone(string boneName, SkeletonRenderer renderer) {
		return renderer.skeleton == null ? null : renderer.skeleton.FindBone(boneName);
	}

	public static Spine.BoneData GetBoneData(string boneName, SkeletonDataAsset skeletonDataAsset) {
		var data = skeletonDataAsset.GetSkeletonData(true);
		return data.FindBone(boneName);
	}
}

[ExecuteInEditMode]
[AddComponentMenu("Spine/CustomBoneFollower")]
public class CustomBoneFollower : MonoBehaviour 
{

	#region Inspector
	public CustomSkeletonRenderer skeletonRenderer;
	public CustomSkeletonRenderer SkeletonRenderer 
	{
		get { return skeletonRenderer; }
		set {
			skeletonRenderer = value;
			Initialize();
		}
	}

	/// <summary>If a bone isn't set in code, boneName is used to find the bone.</summary>
	[CustomSpineBone(dataField: "skeletonRenderer")]
	[SerializeField] public string boneName;

	public bool followZPosition = true;
	public bool followBoneRotation = true;

	[Tooltip("Follows the skeleton's flip state by controlling this Transform's local scale.")]
	public bool followSkeletonFlip = true;

	[Tooltip("Follows the target bone's local scale. BoneFollower cannot inherit world/skewed scale because of UnityEngine.Transform property limitations.")]
	public bool followLocalScale = false;

	[UnityEngine.Serialization.FormerlySerializedAs("resetOnAwake")]
	public bool initializeOnAwake = true;
	#endregion

	[NonSerialized] public bool valid;
	[NonSerialized] public Bone bone;
	Transform skeletonTransform;
	bool skeletonTransformIsParent;

		/// <summary>
		/// Sets the target bone by its bone name. Returns false if no bone was found.</summary>
		public bool SetBone (string name) {
			bone = skeletonRenderer.skeleton.FindBone(name);
			if (bone == null) {
				Debug.LogError("Bone not found: " + name, this);
				return false;
			}
			boneName = name;
			return true;
		}

		public void Awake () {
			if (initializeOnAwake) Initialize();
		}

	public void HandleRebuildRenderer (CustomSkeletonRenderer skeletonRenderer) {
		Initialize();
	}

	public void Initialize () {
		bone = null;
		valid = skeletonRenderer != null && skeletonRenderer.valid;
		if (!valid) return;

		skeletonTransform = skeletonRenderer.transform;
		skeletonRenderer.OnRebuild -= HandleRebuildRenderer;
		skeletonRenderer.OnRebuild += HandleRebuildRenderer;
			skeletonTransformIsParent = Transform.ReferenceEquals(skeletonTransform, transform.parent);

		if (!string.IsNullOrEmpty(boneName))
			bone = skeletonRenderer.skeleton.FindBone(boneName);

#if UNITY_EDITOR
		if (Application.isEditor)
			LateUpdate();
#endif
	}

	void OnDestroy () {
		if (skeletonRenderer != null)
			skeletonRenderer.OnRebuild -= HandleRebuildRenderer;
	}

	public void LateUpdate () 
	{
		if (!valid) 
		{
			Initialize();
			return;
		}

			#if UNITY_EDITOR
			if (!Application.isPlaying)
				skeletonTransformIsParent = Transform.ReferenceEquals(skeletonTransform, transform.parent);
			#endif

			if (bone == null) {
				if (string.IsNullOrEmpty(boneName)) return;
				bone = skeletonRenderer.skeleton.FindBone(boneName);
				if (!SetBone(boneName)) return;
			}
		
		Transform thisTransform = this.transform;
		if (skeletonTransformIsParent)
		{
			// Recommended setup: Use local transform properties if Spine GameObject is the immediate parent
			thisTransform.localPosition = new Vector3(bone.worldX, bone.worldY, followZPosition ? 0f : thisTransform.localPosition.z);
				if (followBoneRotation) thisTransform.localRotation = bone.GetQuaternion();
		}
		else
		{
			// For special cases: Use transform world properties if transform relationship is complicated
			Vector3 targetWorldPosition = skeletonTransform.TransformPoint(new Vector3(bone.worldX, bone.worldY, 0f));
			if (!followZPosition) targetWorldPosition.z = thisTransform.position.z;
				float boneWorldRotation = bone.WorldRotationX;

				Transform transformParent = thisTransform.parent;
				if (transformParent != null) {
					Matrix4x4 m = transformParent.localToWorldMatrix;
					if (m.m00 * m.m11 - m.m01 * m.m10 < 0) // Determinant2D is negative
						boneWorldRotation = -boneWorldRotation;
				}

			if (followBoneRotation) 
			{
				Vector3 worldRotation = skeletonTransform.rotation.eulerAngles;
					#if UNITY_5_6_OR_NEWER
					thisTransform.SetPositionAndRotation(targetWorldPosition, Quaternion.Euler(worldRotation.x, worldRotation.y, skeletonTransform.rotation.eulerAngles.z + boneWorldRotation));
					#else
					thisTransform.position = targetWorldPosition;
					thisTransform.rotation = Quaternion.Euler(worldRotation.x, worldRotation.y, skeletonTransform.rotation.eulerAngles.z + bone.WorldRotationX);
					#endif
				} else {
					thisTransform.position = targetWorldPosition;
			}
		}

		Vector3 localScale = followLocalScale ? new Vector3(bone.scaleX, bone.scaleY, 1f) : Vector3.one;
		if (followSkeletonFlip) localScale.y *= bone.skeleton.flipX ^ bone.skeleton.flipY ? -1f : 1f;
		thisTransform.localScale = localScale;
	}
}