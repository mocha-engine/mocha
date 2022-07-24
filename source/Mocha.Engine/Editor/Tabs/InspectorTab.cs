using ImGuiNET;
using System.Reflection;
using Veldrid;

namespace Mocha.Engine;

[EditorMenu( "Scene/Inspector" )]
internal class InspectorTab : BaseTab
{
	class ReflectedElement
	{
		public string Name { get; set; }
		public object? Value { get; set; }
		public Action<object> SetValue { get; set; }

		public ReflectedElement( object o, FieldInfo fieldInfo )
		{
			Name = fieldInfo.Name;
			Value = fieldInfo.GetValue( o );
			SetValue = ( v ) => fieldInfo.SetValue( o, v );
		}

		public ReflectedElement( object o, PropertyInfo propertyInfo )
		{
			Name = propertyInfo.Name;
			Value = propertyInfo.GetValue( o );

			if ( propertyInfo.SetMethod != null )
				SetValue = ( v ) => propertyInfo.SetValue( o, v );
		}
	}

	public InspectorTab()
	{
		isVisible = true;
	}

	private void DrawElement( ReflectedElement thing )
	{
		if ( thing.Value is Vector3 vec3 )
		{
			var sysVec3 = vec3.GetSystemVector3();
			if ( EditorHelpers.Vector3Input( $"##thing_{thing.Name}", ref sysVec3 ) )
				thing.SetValue?.Invoke( new Vector3( sysVec3 ) );
		}
		else if ( thing.Value is Matrix4x4 mat4 )
		{
			ImGui.SetNextItemWidth( -1 );
			ImGui.Text( $"{mat4.Column1():0.00}" );
			ImGui.SetNextItemWidth( -1 );
			ImGui.Text( $"{mat4.Column2():0.00}" );
			ImGui.SetNextItemWidth( -1 );
			ImGui.Text( $"{mat4.Column3():0.00}" );
			ImGui.SetNextItemWidth( -1 );
			ImGui.Text( $"{mat4.Column4():0.00}" );
		}
		else if ( thing.Value is string str )
		{
			ImGui.SetNextItemWidth( -1 );
			if ( ImGui.InputText( $"##thing_{thing.Name}", ref str, 256 ) )
				thing.SetValue?.Invoke( str );
		}
		else if ( thing.Value is float f )
		{
			ImGui.SetNextItemWidth( -1 );
			if ( ImGui.DragFloat( $"##thing_{thing.Name}", ref f ) )
				thing.SetValue?.Invoke( f );
		}
		else if ( thing.Value is Rotation r )
		{
			var eulerAngles = r.ToEulerAngles().GetSystemVector3();
			ImGui.SetNextItemWidth( -1 );
			if ( ImGui.DragFloat3( $"##thing_{thing.Name}", ref eulerAngles ) )
				thing.SetValue?.Invoke( Rotation.From( eulerAngles.X, eulerAngles.Y, eulerAngles.Z ) );
		}
		else if ( thing.Value is RgbaFloat col )
		{
			var sysVec4 = col.ToVector4();
			ImGui.SetNextItemWidth( -1 );
			if ( ImGui.ColorEdit4( $"##thing_{thing.Name}", ref sysVec4 ) )
				thing.SetValue?.Invoke( new RgbaFloat( sysVec4 ) );
		}
		else
		{
			ImGui.Text( $"{thing.Value}" );
			ImGui.PushStyleColor( ImGuiCol.Text, OneDark.Generic );
			ImGui.Text( $"[{thing.Value.GetType()}]" );
			ImGui.PopStyleColor();
		}
	}

	public override void Draw()
	{
		ImGui.Begin( $"Inspector", ref isVisible );

		var selectedEntity = OutlinerTab.Instance.selectedEntity;

		//
		// Inspector
		//
		{
			if ( selectedEntity != null )
			{
				var selectedEntityType = selectedEntity.GetType();

				ImGui.PushFont( Editor.SubheadingFont );
				ImGui.Text( $"{EditorHelpers.GetTypeIcon( selectedEntity.GetType() )} {selectedEntity.Name}" );
				ImGui.PopFont();

				ImGui.PushStyleColor( ImGuiCol.Text, OneDark.Generic );
				ImGui.Text( selectedEntity.GetType().Name );
				ImGui.PopStyleColor();

				EditorHelpers.Separator();

				foreach ( var group in selectedEntityType.GetMembers()
					.Where( x => x.GetCustomAttribute<HideInInspectorAttribute>() == null )
					.Where( x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property )
					.GroupBy( x => x.DeclaringType ) )
				{
					if ( group.Key == typeof( object ) )
						continue;

					if ( group.Count() <= 0 )
						continue;

					var str = EditorHelpers.GetTypeDisplayName( group.Key );

					ImGui.Dummy( new System.Numerics.Vector2( -1, 2 ) );
					ImGui.PushFont( Editor.BoldFont );
					ImGui.Text( str );
					ImGui.PopFont();
					ImGui.Dummy( new System.Numerics.Vector2( -1, 2 ) );

					if ( ImGui.BeginTable( $"##table_{group}", 2, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
					{
						ImGui.TableSetupColumn( "Name", ImGuiTableColumnFlags.WidthFixed, 125f );
						ImGui.TableSetupColumn( "Value", ImGuiTableColumnFlags.WidthStretch, 1f );

						foreach ( var item in group )
						{
							ImGui.TableNextRow();
							ImGui.TableNextColumn();
							ImGui.Text( $"{EditorHelpers.GetDisplayName( item.Name )}" );
							ImGui.TableNextColumn();

							if ( item.MemberType == MemberTypes.Field )
							{
								var field = item as FieldInfo;
								var thing = new ReflectedElement( selectedEntity, field );

								DrawElement( thing );
							}
							else if ( item.MemberType == MemberTypes.Property )
							{
								var property = item as PropertyInfo;
								var thing = new ReflectedElement( selectedEntity, property );

								DrawElement( thing );
							}
						}

						ImGui.EndTable();
					}

					EditorHelpers.Separator();
				}
			}
		}

		ImGui.End();
	}
}
