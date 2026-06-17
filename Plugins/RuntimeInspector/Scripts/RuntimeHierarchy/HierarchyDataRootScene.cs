using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuntimeInspectorNamespace
{
	public class HierarchyDataRootScene : HierarchyDataRoot
	{
		public override string Name { get { return Scene.name; } }
		public override int ChildCount { get { return rootObjects.Count; } }

		public Scene Scene { get; private set; }

		private readonly List<GameObject> rootObjects = new List<GameObject>();

		public HierarchyDataRootScene( RuntimeHierarchy hierarchy, Scene target ) : base( hierarchy )
		{
			Scene = target;
		}

		public override void RefreshContent()
		{
			rootObjects.Clear();

			if( !Scene.isLoaded )
				return;

			Scene.GetRootGameObjects( rootObjects );

			// iVP: if a content root is registered for this scene, expose ITS
			// children as the scene's top level (and hide the content root
			// itself). Top-level objects are then non-root transforms, so they
			// can be reordered via SetSiblingIndex at runtime in builds. Any
			// stray genuine scene roots are kept so nothing can vanish from the
			// hierarchy if some path re-roots an object.
			Transform contentRoot = RuntimeInspectorUtils.GetSceneContentRoot( Scene );
			if( contentRoot )
			{
				rootObjects.Remove( contentRoot.gameObject );
				for( int i = 0; i < contentRoot.childCount; i++ )
					rootObjects.Add( contentRoot.GetChild( i ).gameObject );
			}
		}

		public override Transform GetChild( int index )
		{
			GameObject rootObject = rootObjects[index];
			return rootObject ? rootObject.transform : null;
		}

		public override Transform GetNearestRootOf( Transform target )
		{
			if( target.gameObject.scene != Scene )
				return null;

			// iVP: with a content root, the visible "root" of a target is the
			// ancestor that is a direct child of the content root.
			Transform contentRoot = RuntimeInspectorUtils.GetSceneContentRoot( Scene );
			if( contentRoot && target != contentRoot && target.IsChildOf( contentRoot ) )
			{
				Transform current = target;
				while( current.parent != contentRoot )
					current = current.parent;

				return current;
			}

			return target.root;
		}

		public override HierarchyDataTransform FindTransform( Transform target, Transform nextInPath = null )
		{
			// iVP: translate target.root to the content-root child so that
			// selection/reveal works while the content root itself is hidden.
			if( nextInPath == null && RuntimeInspectorUtils.GetSceneContentRoot( Scene ) )
			{
				nextInPath = GetNearestRootOf( target );
				if( !nextInPath )
					return null;
			}

			return base.FindTransform( target, nextInPath );
		}
	}
}
