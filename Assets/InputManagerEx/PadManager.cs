using UnityEngine;
using System.Collections;
using System;
//using UnityEditor;

/// <summary>
/// パッドマネージャー・パッドコンフィグ付き
/// </summary>
public class PadManager : MonoBehaviour {


	static private PadManager instance;
	static public PadManager GetInstance() { return instance; }

	static private bool IsPadConfig = false; /// パッドコンフィグ開始
	static private int AxisConfigStep = 0;		///
	static private int AxisConfigIndex = 0;

	static private int ActivePadIndex = 1; // アクティブなパッドNo


	void Awake()
	{
		instance = this;
	}

	// 軸定義
	public enum Axis { 
		LeftStick,	/// LStick
		RightStick, /// RStick
		POV			/// POV
	};

	/// <summary>
	/// パッド番号定義
	/// </summary>
	public enum Index { 
		Any, _1P, _2P, _3P, _4P,Num
	};


	string[] RightAxisX = new string[(int)Index.Num];
	string[] RightAxisY = new string[(int)Index.Num];
	string[] PovX = new string[(int)Index.Num];
	string[] PovY = new string[(int)Index.Num];


	/// <summary>
	///  ボタン定義
	/// </summary>
	public enum Button { A, B, Y, X, R, L, RightStick, LeftStick, Back, Start }

	IEnumerator AxisSettingFunc()
	{
		int padIndex = 1;


		// アクティブなパッドチェック
		while(true){

			if (Input.GetButton("Player" + padIndex + "_OK") || Input.GetButton("Player" + padIndex + "_Cancel"))
			{
				AxisConfigStep++;
				AxisConfigIndex = 0;

				Debug.Log("アクティブなパッド決定 " + padIndex);
				ActivePadIndex = padIndex;

				break;
			}

			padIndex++;
			if (padIndex > 4) padIndex = 1;

			yield return new WaitForSeconds(0.5f);

		}

		// 右スティックY軸チェック
		while(true){
			if (AxisConfigIndex == 1) { RightAxisY[padIndex] = "_1"; } else { RightAxisY[padIndex] = ""; }
			// 見つかった
			if (GetAxis(Axis.RightStick, (Index)padIndex).y >= 0.8f)
			{
				Debug.Log("右スティックY軸めっけ");

				AxisConfigStep++;
				AxisConfigIndex = 0;
				break;
			}
			else
			{
				AxisConfigIndex++;
				if (AxisConfigIndex > 1)
				{
					AxisConfigIndex = 0;
				}
			}

			yield return new WaitForSeconds(0.5f);
		}

	

		// 右スティックX軸チェック
		while (true)
		{
			if (AxisConfigIndex == 1) { RightAxisX[padIndex] = "_1"; } else { RightAxisX[padIndex] = ""; }
			// 見つかった
			if (GetAxis(Axis.RightStick, (Index)padIndex).x >= 0.8f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;
				Debug.Log("右スティックX軸めっけ");
				break;

			}
			else
			{
				AxisConfigIndex++;
				if (AxisConfigIndex > 1)
				{
					AxisConfigIndex = 0;
				}
			}

			yield return new WaitForSeconds(0.5f);
		}

		// POV Yチェック
		while (true)
		{
			if (AxisConfigIndex == 1) { PovY[padIndex] = "_1"; } else { PovY[padIndex] = ""; }
			// 見つかった
			if (GetAxis(Axis.POV, (Index)padIndex).y >= 0.8f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;
				Debug.Log("POV Y軸めっけ");
				break;

			}
			else
			{
				AxisConfigIndex++;
				if (AxisConfigIndex > 1)
				{
					AxisConfigIndex = 0;
				}
			}

			yield return new WaitForSeconds(0.5f);
		}


		// POV Xチェック
		while (true)
		{
			if (AxisConfigIndex == 1) { PovX[padIndex] = "_1"; } else { PovX[padIndex] = ""; }
			// 見つかった
			if (GetAxis(Axis.POV, (Index)padIndex).x >= 0.8f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;
				Debug.Log("POV X軸めっけ");
				break;

			}
			else
			{
				AxisConfigIndex++;
				if (AxisConfigIndex > 1)
				{
					AxisConfigIndex = 0;
				}
			}

			yield return new WaitForSeconds(0.5f);
		}


		yield return new WaitForSeconds(1.0f);

		Debug.Log("終わり");

		IsPadConfig =false;

		yield break;
	}




	void OnGUI()
	{

		String pad = "";


		float size = 20.0f;

		if (GUI.Button(new Rect(8, 20, 120, 24), "パッドコンフィグ"))
		{
			IsPadConfig = true;
			AxisConfigStep = 0;
			AxisConfigIndex = 0;

			StartCoroutine("AxisSettingFunc");

		}



		if(IsPadConfig){

			switch (AxisConfigStep)
			{
				case 0: GUI.Label(new Rect(50, 50, 300, 20), "使うパットのボタンなんか押して！"); break;
				case 1: GUI.Label(new Rect(50, 50, 300, 20), "右スティックY軸チェック 上に倒して！"); break;
				case 2: GUI.Label(new Rect(50, 50, 300, 20), "右スティックX軸チェック 右に倒して！"); break;
				case 3: GUI.Label(new Rect(50, 50, 300, 20), "十字キー(POV) Y チェック 下押して！"); break;
				case 4: GUI.Label(new Rect(50, 50, 300, 20), "十字キー(POV) X チェック 右押して！"); break;
			}

			

		}else{


			
			float startY = 100;
			float startX = 150;

			GUI.Box(new Rect(10, startY, 300, 300),"");

			GUI.Label(new Rect(startX, startY , 100, 20), "左");
			GUI.Label(new Rect(startX + 50, startY , 100, 20), "右");
			GUI.Label(new Rect(startX + 100, startY, 100, 20), "POV");

			for (int i = 1; i <= (int)Index._4P; i++)
			{

				if (ActivePadIndex == i)
				{
					GUI.Label(new Rect(50, startY + 50 * i, 100, 20), "アクティブ→");
				}


				// スティック
				GUI.Label(new Rect(startX + GetAxis(Axis.LeftStick, (Index)i).x * size, startY + 50 * i - GetAxis(Axis.LeftStick, (Index)i).y * size, 100, 20), "+");
				GUI.Label(new Rect(startX, startY + 50 * i, 100, 20), "*");

				GUI.Label(new Rect(startX + 50 + GetAxis(Axis.RightStick, (Index)i).x * size, startY + 50 * i - GetAxis(Axis.RightStick, (Index)i).y * size, 100, 20), "+");
				GUI.Label(new Rect(startX + 50, startY + 50 * i, 100, 20), "*");


				GUI.Label(new Rect(startX + 100 + GetAxis(Axis.POV, (Index)i).x * size, startY + 50 * i + GetAxis(Axis.POV, (Index)i).y * size, 100, 20), "+");
				GUI.Label(new Rect(startX + 100, startY + 50 * i, 100, 20), "*");


				// ボタン


			}
		}


	}



	/// <summary>
	/// returns a specified axis
	/// </summary>
	/// <param name="axis">One of the analogue sticks, or the dpad</param>
	/// <param name="controlIndex">The controller number</param>
	/// <param name="raw">if raw is false then the controlIndex will be returned with a deadspot</param>
	/// <returns></returns>
	public static Vector2 GetAxis(Axis axis, Index controlIndex = Index.Any, bool raw = true)
	{

		string xName = "", yName = "";
		switch (axis)
		{
			case Axis.POV:
				xName = "Player" + (int)controlIndex + "_DX"+  instance.PovX[(int)controlIndex];
				yName = "Player" + (int)controlIndex + "_DY" + instance.PovY[(int)controlIndex];
				break;
			case Axis.LeftStick:
				xName = "Player" + (int)controlIndex + "_LX";
				yName = "Player" + (int)controlIndex + "_LY";
				break;
			case Axis.RightStick:
				xName = "Player" + (int)controlIndex + "_RX" + instance.RightAxisX[(int)controlIndex];
				yName = "Player" + (int)controlIndex + "_RY" + instance.RightAxisY[(int)controlIndex];
				break;
		}

		Vector2 axisXY = Vector3.zero;

		try
		{
			if (raw == false)
			{
				axisXY.x = Input.GetAxis(xName);
				axisXY.y = -Input.GetAxis(yName);
			}
			else
			{
				axisXY.x = Input.GetAxisRaw(xName);
				axisXY.y = -Input.GetAxisRaw(yName);
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError(e);
			Debug.LogWarning("Have you set up all axes correctly? \nThe easiest solution is to replace the InputManager.asset with version located in the GamepadInput package. \nWarning: do so will overwrite any existing input");
		}
		return axisXY;
	}


	// Use this for initialization
	void Start () {



	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
