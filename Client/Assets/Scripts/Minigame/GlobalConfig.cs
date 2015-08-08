using UnityEngine;
using System.Collections;

public class GlobalConfig : DisposableSingleton<GlobalConfig> {

	//Player Config
	public float gravityAccel = 10f;

	public float maxVerticalSpeed = 25f;
	public float maxHorizontalSpeed = 7f;
	public float walkingAccel = 70f;
	public float groundFrictionHorizontalAccel = 24f;
	public float airFrictionHoizontalAccel = 5f;
	public float airHorizontalAccel = 30f;

	public float airJumpHeight = 1.4f;
	public float maxJumpHeight = 2.6f;
	public float minJumpHeight = 1.4f;

	public float wallFrictionVerticalAcc = 20f;
	public float wallMaxVerticalFallingSpeed = 1.1f;
	public float wallJumpHorizontalSpeed = 5.5f;

	public float attackDelay = 0.4f;

	public float projectileVelocity = 40f;
}
