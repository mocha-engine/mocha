﻿using ImGuiNET;
using System.Numerics;
using System.Reflection;

namespace Mocha;

[EditorMenu( "Scene/Outliner" )]
internal class SceneTab : BaseTab
{
	private Entity? selectedEntity;

	class ReflectedThing
	{
		public string Name { get; set; }
		public object? Value { get; set; }
		public Action<object> SetValue { get; set; }

		public ReflectedThing( object o, FieldInfo fieldInfo )
		{
			Name = fieldInfo.Name;
			Value = fieldInfo.GetValue( o );
			SetValue = ( v ) => fieldInfo.SetValue( o, v );
		}

		public ReflectedThing( object o, PropertyInfo propertyInfo )
		{
			Name = propertyInfo.Name;
			Value = propertyInfo.GetValue( o );

			if ( propertyInfo.SetMethod != null )
				SetValue = ( v ) => propertyInfo.SetValue( o, v );
		}
	}

	private void DrawElement( ReflectedThing thing )
	{
		if ( thing.Value is Vector3 vec3 )
		{
			var sysVec3 = vec3.GetSystemVector3();
			ImGui.SetNextItemWidth( -1 );
			if ( ImGui.DragFloat3( $"##thing_{thing.Name}", ref sysVec3 ) )
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
		ImGui.Begin( "Scene", ref visible );

		//
		// Hierarchy
		//
		{
			ImGui.SetNextItemWidth( -1 );
			ImGui.BeginListBox( "##hierarchy" );

			foreach ( var entity in Entity.All )
			{
				var startPos = ImGui.GetCursorPos();

				if ( ImGui.Selectable( entity.Name ) )
				{
					selectedEntity = entity;
				}

				ImGui.Separator();
			}

			ImGui.EndListBox();
		}

		ImGui.Separator();

		//
		// Inspector
		//
		{
			if ( selectedEntity != null )
			{
				var selectedEntityType = selectedEntity.GetType();

				foreach ( var group in selectedEntityType.GetMembers().GroupBy( x => x.DeclaringType ) )
				{
					if ( group.Key == typeof( object ) )
						continue;

					ImGui.Text( $"{group.Key}:" );

					if ( ImGui.BeginTable( $"##table_{group}", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
					{
						ImGui.TableSetupColumn( "Name", ImGuiTableColumnFlags.WidthFixed, 100f );
						ImGui.TableSetupColumn( "Value", ImGuiTableColumnFlags.WidthStretch, 1f );

						ImGui.TableNextColumn();
						ImGui.TableHeader( "Name" );
						ImGui.TableNextColumn();
						ImGui.TableHeader( "Value" );

						foreach ( var item in group )
						{
							if ( item.MemberType != MemberTypes.Field && item.MemberType != MemberTypes.Property )
								continue;

							ImGui.TableNextRow();
							ImGui.TableNextColumn();
							ImGui.Text( $"{item.Name}" );
							ImGui.TableNextColumn();

							if ( item.MemberType == MemberTypes.Field )
							{
								var field = item as FieldInfo;
								var thing = new ReflectedThing( selectedEntity, field );

								DrawElement( thing );
							}
							else if ( item.MemberType == MemberTypes.Property )
							{
								var property = item as PropertyInfo;
								var thing = new ReflectedThing( selectedEntity, property );

								DrawElement( thing );
							}
						}

						ImGui.EndTable();
					}
				}
			}
		}

		ImGui.End();
	}
}
