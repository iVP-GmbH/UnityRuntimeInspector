using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeInspectorNamespace
{
	public class BoolField : InspectorField<bool>
	{
#pragma warning disable 0649
		[SerializeField]
		private Image toggleBackground;

		[SerializeField]
		private Toggle input;

		[SerializeField]
		private Image multiValueImage;
#pragma warning restore 0649

		public override void Initialize()
		{
			base.Initialize();
			input.onValueChanged.AddListener( OnValueChanged );
		}

		private void OnValueChanged( bool input )
		{
			BoundValues = new bool[] { input }.AsReadOnly();
			Inspector.RefreshDelayed();
		}

		protected override void OnBound( MemberInfo variable )
		{
			base.OnBound( variable );
			bool single;
			if( BoundValues.TryGetSingle( out single ) )
			{
				input.SetIsOnWithoutNotify( single );
			}
		}

		protected override void OnSkinChanged()
		{
			base.OnSkinChanged();

			toggleBackground.color = Skin.InputFieldNormalBackgroundColor;
			input.graphic.color = Skin.ToggleCheckmarkColor;
			multiValueImage.color = Skin.ToggleCheckmarkColor;

			Vector2 rightSideAnchorMin = new Vector2( Skin.LabelWidthPercentage, 0f );
			variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
			( (RectTransform) input.transform ).anchorMin = rightSideAnchorMin;
		}

		private void SwitchMarks( bool hasMultipleValues )
		{
			input.graphic.gameObject.SetActive( !hasMultipleValues );
			multiValueImage.enabled = hasMultipleValues;
		}

		public override void Refresh()
		{
			base.Refresh();

			bool single;
			if( BoundValues.TryGetSingle( out single ) )
			{
				// Without notify: Refresh only mirrors the bound value into the
				// UI. A plain isOn-assignment fired onValueChanged when a pooled
				// toggle still carried the previous panel's state, running the
				// bound SETTER during panel build.
				input.SetIsOnWithoutNotify( single );
				SwitchMarks( false );
			}
			else
			{
				SwitchMarks( true );
			}
		}

		protected override void OnIsInteractableChanged()
		{
			base.OnIsInteractableChanged();
			input.interactable = IsInteractable;
			input.graphic.color = this.GetTextColor();
		}
	}
}
