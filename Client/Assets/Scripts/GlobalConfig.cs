using UnityEngine;
using System.Collections;

public class GlobalConfig : DisposableSingleton<GlobalConfig> {

	//Player Config
	public float gravityAccel = 10f;
	public int initialHP = 10;
	public float respawnDelay = 2f;
	public float inmortalTime = 0.0f;

	public float maxVerticalSpeed = 25f;
	public float maxHorizontalSpeed = 7f;
	public float walkingAccel = 70f;
	public float groundFrictionAccel = 24f;
	public float airFrictionAccel = 5f;
	public float airHorizontalAccel = 30f;

	public float airJumpHeight = 1.4f;
	public float maxJumpHeight = 2.6f;
	public float minJumpHeight = 1.4f;

	public float wallFrictionAcc = 20f;
	public float wallMaxVeticalSpeed = 1.1f;
	public float wallJumpHorizontalSpeed = 5.5f;

	public float delayBetweenProjectiles = 2.0f;
	public float projectileThrowingValue = 0.22f;


	public float attackSpeed = 2.3f;

	public float projectileVelocity = 40f;
}
