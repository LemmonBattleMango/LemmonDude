using UnityEngine;
using System.Collections;

public class VectorUtils {
	
	public const float DEPTH_ANGLE = 45f;
	public static float DEPTH_FACTOR = Mathf.Sin(90 - DEPTH_ANGLE) / Mathf.Sin (DEPTH_ANGLE);
	
	public static Vector2 GetPosition2D( Transform t )
	{
		return GetPosition2D( t.position );
	}
	
	public static Vector2 GetPosition2D( Vector3 pos )
	{
		return new Vector2( pos.x, pos.y );
	}
	
	public static Vector3 GetPosition3D( Vector2 pos )
	{
		return new Vector3 (pos.x, pos.y, 0);
	}

	public static Vector3 GetPosition3D( Vector3 pos )
	{
		return new Vector3( pos.x, pos.y, 0 );
	}
}