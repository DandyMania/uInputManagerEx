/*
 * Copyright (c) 2016 DandyMania
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 * Latest version: https://github.com/DandyMania/uInputManagerEx/
*/

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

	static private int ActivePadIndex = 0; // アクティブなパッドNo

	const int PadButtonMax = 16; // ボタンの最大数

	const int PadMax = 4;

	// キーリピート
	const float REPEAT_WAIT = 0.5f;
	const float REPEAT_INTERVAL = 0.1f;

	void Awake()
	{
		instance = this;
	}

	// 軸定義
	public enum Axis { 
		LeftStick,	/// LStick
		RightStick, /// RStick
		POV,			/// POV
		LRTrigger	/// 360コントローラのLT/RT
	};

	/// <summary>
	/// パッド番号定義
	/// </summary>
	public enum Index {
		_1P, _2P, _3P, _4P, Num, Any, Active,
	};


	/// <summary>
	///  ボタン定義
	/// </summary>
	public enum Button { A, B, X, Y, LB, RB, Back, Start, LS, RS, LT, RT, UP, RIGHT, DOWN,LEFT, Max }

	/// <summary>
	/// パッドデータ
	/// </summary>
	class PadData
	{
		// パッド名
		public string JoyStickName;
		public bool isXbox; // 360コントローラ(XInput系)

		// 軸タイプ
		public string RightAxisX;
		public string RightAxisY;
		public string PovX;
		public string PovY;
		public string LRTriggerAxis;

		public int[] ConvTable = new int[(int)Button.Max]; // 変換テーブル

		public bool[] Prev = new bool[(int)Button.Max]; // 前のフレームの押下情報
		public bool[] Now = new bool[(int)Button.Max]; // 現のフレームの押下情報

		public float[] RepeatWait = new float[(int)Button.Max];
		public bool[] Repeat = new bool[(int)Button.Max]; // キーリピート


	};

	PadData[] padData = new PadData[(int)Index.Num];

	
	IEnumerator AxisSettingFunc()
	{
		int padIndex = 0;

		AxisConfigStep = 0;
		AxisConfigIndex = 0;
		ActivePadIndex = 0;

		//-----------------------------
		// アクティブなパッドチェック
		//-----------------------------
		while(true){


			bool bDecide = false;
			for (int i = 0; i < 6; i++)
			{
				if (Input.GetButton("Player" + padIndex + "_Btn" + i) )
				{
					AxisConfigStep++;
					AxisConfigIndex = 0;

					PadData pad = instance.padData[(int)padIndex];

					Debug.Log("アクティブなパッド決定＆スティックキャリブレーション " + padIndex + " " + pad.JoyStickName);
					ActivePadIndex = padIndex;

					bDecide = true;

					// OKボタンをゼロ番に割当
					
					pad.ConvTable[i] = (int)Button.A;
					pad.ConvTable[(int)Button.A] = i;
					

					break;
				}
			}
			if (bDecide)
			{
				break;
			}
			else
			{
				padIndex++;
				if (padIndex > (int)Index._4P) padIndex = 0;

				yield return new WaitForSeconds(0.1f);

			}
		}

		yield return new WaitForSeconds(1.0f);

		//-----------------------------
		// 右スティックY軸チェック
		//-----------------------------
		while(true){


			PadData pad = instance.padData[(int)padIndex];
			if (AxisConfigIndex == 1) { pad.RightAxisY = "_1"; } else { pad.RightAxisY = ""; }
			// 見つかった
			if (GetAxis(Axis.RightStick, (Index)padIndex).y <= -0.8f)
			{
				Debug.Log("右スティックY軸めっけ");

				AxisConfigStep++;
				AxisConfigIndex = 0;
				break;
			}
			else
			{

				if(GetTrigger(Button.A)){
					// 右スティックなし
					AxisConfigStep++;
					pad.RightAxisY = "_none";

					Debug.Log("右スティック無し");
				}else{
					AxisConfigIndex++;
					if (AxisConfigIndex > 1)
					{
						AxisConfigIndex = 0;
					}
				}
			}

			yield return new WaitForSeconds(0.2f);
		}


		yield return new WaitForSeconds(1.0f);

		//-----------------------------
		// 真ん中に戻すまで待つ
		//-----------------------------
		while (true)
		{
			if ( Mathf.Abs(GetAxis(Axis.RightStick, (Index)padIndex).y) <= 0.8f)
			{
				break;
			}

			yield return new WaitForSeconds(0.5f);
		}


		//-----------------------------
		// 右スティックX軸チェック
		//-----------------------------
		while (true)
		{

			PadData pad = instance.padData[(int)padIndex];
			if (AxisConfigIndex == 1) { pad.RightAxisX= "_1"; } else { pad.RightAxisX = ""; }
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

				if (GetTrigger(Button.A))
				{
					// 右スティックなし
					AxisConfigStep++;
					pad.RightAxisY = "_none";

					Debug.Log("右スティック無し");
				}else{
					AxisConfigIndex++;
					if (AxisConfigIndex > 1)
					{
						AxisConfigIndex = 0;
					}
				}
			}

			yield return new WaitForSeconds(0.2f);
		}

		//-----------------------------
		// POV Yチェック
		//-----------------------------
		while (true)
		{

			PadData pad = instance.padData[(int)padIndex];
			if (AxisConfigIndex == 1) { pad.PovY = "_1"; } else { pad.PovY = ""; }
			// 見つかった
			if (GetAxis(Axis.POV, (Index)padIndex).y >= 0.8f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;
				Debug.Log("POV Y軸めっけ" + GetAxis(Axis.POV, (Index)padIndex).y.ToString());
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

			yield return new WaitForSeconds(0.2f);
		}

		yield return new WaitForSeconds(1.0f);

		// 真ん中に戻すまで待つ
		while (true)
		{
			if (Mathf.Abs(GetAxis(Axis.POV, (Index)padIndex).y) <= 0.8f)
			{
				break;
			}

			yield return new WaitForSeconds(0.5f);
		}

		//-----------------------------
		// POV Xチェック
		//-----------------------------
		while (true)
		{

			PadData pad = instance.padData[(int)padIndex];
			if (AxisConfigIndex == 1) { pad.PovX = "_1"; } else { pad.PovX = ""; }
			// 見つかった
			if (GetAxis(Axis.POV, (Index)padIndex).x >= 0.8f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;
				Debug.Log("POV X軸めっけ" + GetAxis(Axis.POV, (Index)padIndex).x.ToString());
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

			yield return new WaitForSeconds(0.2f);
		}


		yield return new WaitForSeconds(1.0f);

		//-----------------------------
		// L2/R2の判定
		//-----------------------------
		while (true)
		{

			// 360かも
			PadData pad = instance.padData[(int)padIndex];
			if ( Mathf.Abs(GetAxis(Axis.LRTrigger, (Index)padIndex).x) >= 0.8f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;

				pad.isXbox = true; // 360コントローラ

				Debug.Log("L2/R2がアナログっぽいので360コントローラかも");
				break;

			}
			else
			{

				bool bDecide = false;
				for (int i = 0; i < PadButtonMax; i++){
					if (Input.GetButton("Player" + padIndex + "_Btn" + i) ){
						bDecide= true;
					}
				}

				if(bDecide){

					AxisConfigStep++;
					AxisConfigIndex = 0;
					
					pad.LRTriggerAxis = "_none";
					

					break;
				}else{
					AxisConfigIndex++;
					if (AxisConfigIndex > 1)
					{
						AxisConfigIndex = 0;
					}
				}
			}

			yield return new WaitForSeconds(0.2f);
		}


		yield return new WaitForSeconds(1.0f);

		Debug.Log("終わり");

		IsPadConfig =false;

		yield break;
	}




	void OnGUI()
	{

		//String pad = "";


		{// FPS
			float fps = 1f / Time.deltaTime;
			GUI.Label(new Rect(0, 0, 300, 20), fps.ToString("#.#") + "fps");

		}


		float size = 20.0f;

		if (GUI.Button(new Rect(8, 20, 180, 24), "パッドコンフィグ開始"))
		{
			IsPadConfig = true;
			AxisConfigStep = 0;
			AxisConfigIndex = 0;

			StartCoroutine("AxisSettingFunc");

		}



		if (IsPadConfig)
		{

			switch (AxisConfigStep)
			{
				case 0: GUI.Label(new Rect(50, 50, 350, 20), "スティックに触らずに使うパットのOKボタン押して！"); break;
				case 1: GUI.Label(new Rect(50, 50, 300, 20), "右スティックY軸チェック 下に倒して！\n無い場合はOKボタン押して！"); break;
				case 2: GUI.Label(new Rect(50, 50, 300, 20), "右スティックX軸チェック 右に倒して！\n無い場合はOKボタン押して！"); break;
				case 3: GUI.Label(new Rect(50, 50, 300, 20), "十字キー(POV) Y チェック 下押して！"); break;
				case 4: GUI.Label(new Rect(50, 50, 300, 20), "十字キー(POV) X チェック 右押して！"); break;
				case 5: GUI.Label(new Rect(50, 50, 300, 20), "L2/R2どっちか押して！(Xbox360判定)"); break;

			}



		}
		else
		{



			float startY = 100;
			float startX = 100;

			GUI.Box(new Rect(10, startY - 50, 650, 450), "");

			GUI.Label(new Rect(startX, startY - 50, 100, 20), "左");
			GUI.Label(new Rect(startX + 50, startY - 50, 100, 20), "右");
			GUI.Label(new Rect(startX + 100, startY - 50, 100, 20), "POV");

			for (int iPad = 0; iPad < (int)Index.Num; iPad++)
			{

				const int YOffset = 100;
				if (ActivePadIndex == iPad)
				{
					GUI.Label(new Rect(startX - 90, startY + YOffset * iPad, 100, 20), "Active→");
				}


				GUI.Label(new Rect(startX - 90, startY + YOffset * iPad - 20, 100, 20), padData[iPad].JoyStickName);


				// スティック
				GUI.Label(new Rect(startX + GetAxis(Axis.LeftStick, (Index)iPad).x * size, startY + YOffset * iPad - GetAxis(Axis.LeftStick, (Index)iPad).y * size, 100, 20), "+");
				GUI.Label(new Rect(startX, startY + YOffset * iPad, 100, 20), "+");

				GUI.Label(new Rect(startX + 50 + GetAxis(Axis.RightStick, (Index)iPad).x * size, startY + YOffset * iPad - GetAxis(Axis.RightStick, (Index)iPad).y * size, 100, 20), "+");
				GUI.Label(new Rect(startX + 50, startY + YOffset * iPad, 100, 20), "+");


				GUI.Label(new Rect(startX + 100 + GetAxis(Axis.POV, (Index)iPad).x * size, startY + YOffset * iPad + GetAxis(Axis.POV, (Index)iPad).y * size, 100, 20), "+");
				GUI.Label(new Rect(startX + 100, startY + YOffset * iPad, 100, 20), "+");

				// ボタンRaw
				for (int button = 0; button < PadButtonMax; button++)
				{
					if (GetRawButton(button, (Index)iPad))
					{

						GUI.Label(new Rect(startX + 150 + 20 * button, startY + YOffset * iPad - 20, 100, 20), "O");
					}
					else
					{
						GUI.Label(new Rect(startX + 150 + 20 * button, startY + YOffset * iPad - 20, 100, 20), "X");
					}
				}

				// LT/RT
				GUI.Label(new Rect(startX + 150 + 20 * 16, startY + YOffset * iPad - 20, 100, 20), GetAxis(Axis.LRTrigger, (Index)iPad).y.ToString("0.00"));


				// トリガー
				for (int button = 0; button < (int)Button.Max; button++)
				{
					if (GetTrigger((Button)button, (Index)iPad))
					{

						GUI.Label(new Rect(startX + 150 + 20 * button, startY + YOffset * iPad, 100, 20), "O");
					}
					else
					{
						GUI.Label(new Rect(startX + 150 + 20 * button, startY + YOffset * iPad, 100, 20), "X");
					}
				}


				// Press
				for (int button = 0; button < (int)Button.Max; button++)
				{
					if (GetPress((Button)button, (Index)iPad))
					{

						GUI.Label(new Rect(startX + 150 + 20 * button, startY + YOffset * iPad + 15, 100, 20), "O");
					}
					else
					{
						GUI.Label(new Rect(startX + 150 + 20 * button, startY + YOffset * iPad + 15, 100, 20), "X");
					}
				}

				// リピート
				for (int button = 0; button < (int)Button.Max; button++)
				{
					if (GetRepeat((Button)button, (Index)iPad))
					{

						GUI.Label(new Rect(startX + 150 + 20 * button, startY + YOffset * iPad + 30, 100, 20), "O");
					}
					else
					{
						GUI.Label(new Rect(startX + 150 + 20 * button, startY + YOffset * iPad + 30, 100, 20), "X");
					}
				}

				// リリース
				for (int button = 0; button < (int)Button.Max; button++)
				{
					if (GetRelease((Button)button, (Index)iPad))
					{

						GUI.Label(new Rect(startX + 150 + 20 * button, startY + YOffset * iPad + 45, 100, 20), "O");
					}
					else
					{
						GUI.Label(new Rect(startX + 150 + 20 * button, startY + YOffset * iPad + 45, 100, 20), "X");
					}
				}

			}


		}


	}




	/// <summary>
	/// 固定フレーム更新
	/// </summary>
	public void FixedUpdate() 
	{

		//Debug.Log("Fix Update");

	}

	/// <summary>
	/// 定期更新
	/// </summary>
	public void Update()
	{



		for (int iPad = 0; iPad < (int)Index.Num; iPad++)
		{

			PadData pad = padData[(int)iPad];

			for (int button = 0; button < (int)Button.Max; button++)
			{

				
				pad.Prev[pad.ConvTable[(int)button]] = pad.Now[pad.ConvTable[(int)button]];

				// ボタンチェック
				if (GetRawButton(button, (Index)iPad))
				{
					pad.Now[pad.ConvTable[(int)button]] = true;
				}
				else
				{
					pad.Now[pad.ConvTable[(int)button]] = false;
				}

				//---------------------------
				// Xbox360コントローラ固有
				//---------------------------
				if (pad.isXbox==true)
				{
					UpdateX360(ref pad, (Index)iPad, (Button)button);

				}

				//---------------------------
				// キーリピート処理
				//---------------------------
				pad.Repeat[button] = false;
				if (pad.Prev[button] == false && pad.Now[button] == true)
				{
					pad.RepeatWait[button] = REPEAT_WAIT;
					pad.Repeat[button] = true;
				}
				else if (pad.Prev[button] == true && pad.Now[button] == true)
				{

					pad.RepeatWait[button] -= Time.deltaTime;
					if (pad.RepeatWait[button] < 0.0f)
					{
						pad.RepeatWait[button] = REPEAT_INTERVAL;
						pad.Repeat[button] = true;
					}

				}
				else if (pad.Prev[button] == false && pad.Now[button] == false)
				{
					pad.RepeatWait[button] = 0;
					pad.Repeat[button] = false;
				}

			}
			
		}


		//Debug.Log("Update");

	}


	private void UpdateX360(ref PadData pad, Index iPad, Button button)
	{
		// L2/R2がアナログなので。。。
		switch (button)
		{
			case Button.LT:
				{
					if (GetAxis(Axis.LRTrigger, (Index)iPad).y < -0.5f)
					{

						pad.Now[(int)Button.LT] = true;

					}
					else
					{
						pad.Now[(int)Button.LT] = false;
					}
				}
				break;
			case Button.RT:
				{
					if (GetAxis(Axis.LRTrigger, (Index)iPad).y > 0.5f)
					{

						pad.Now[(int)Button.RT] = true;
					}
					else
					{
						pad.Now[(int)Button.RT] = false;
					}
				}
				break;
			case Button.UP:
				{

					if (GetAxis(Axis.POV, (Index)iPad).y <= -0.8f)
					{
						pad.Now[(int)Button.UP] = true;

					}
					else
					{
						pad.Now[(int)Button.UP] = false;
					}
				}
				break;
			case Button.DOWN:
				{
					{

						if (GetAxis(Axis.POV, (Index)iPad).y >= 0.8f)
						{
							pad.Now[(int)Button.DOWN] = true;

						}
						else
						{
							pad.Now[(int)Button.DOWN] = false;
						}
					}
				}
				break;
			case Button.LEFT:
				{

					if (GetAxis(Axis.POV, (Index)iPad).x <= -0.8f)
					{
						pad.Now[(int)Button.LEFT] = true;

					}
					else
					{
						pad.Now[(int)Button.LEFT] = false;
					}
				}
				break;
			case Button.RIGHT:
				{
					{

						if (GetAxis(Axis.POV, (Index)iPad).x >= 0.8f)
						{
							pad.Now[(int)Button.RIGHT] = true;

						}
						else
						{
							pad.Now[(int)Button.RIGHT] = false;
						}
					}
					break;
				}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public void LateUpdate()
	{
		//Debug.Log("Late Update");

	}



	/// <summary>
	/// returns a specified axis
	/// </summary>
	/// <param name="axis">One of the analogue sticks, or the dpad</param>
	/// <param name="controlIndex">The controller number</param>
	/// <param name="raw">if raw is false then the controlIndex will be returned with a deadspot</param>
	/// <returns></returns>
	public static Vector2 GetAxis(Axis axis, Index controlIndex = Index.Active, bool raw = true)
	{

		if (controlIndex == Index.Active)
		{
			controlIndex = (Index)ActivePadIndex;
		}


		PadData pad = instance.padData[(int)controlIndex];

		string xName = "", yName = "";
		switch (axis)
		{
			case Axis.POV:
				xName = "Player" + (int)controlIndex + "_DX" + pad.PovX;
				yName = "Player" + (int)controlIndex + "_DY" + pad.PovY;
				break;
			case Axis.LeftStick:
				xName = "Player" + (int)controlIndex + "_LX";
				yName = "Player" + (int)controlIndex + "_LY";
				break;
			case Axis.RightStick:
				xName = "Player" + (int)controlIndex + "_RX" + pad.RightAxisX;
				yName = "Player" + (int)controlIndex + "_RY" + pad.RightAxisY;
				break;
			case Axis.LRTrigger:
				xName = "Player" + (int)controlIndex + "_LRTrigger" + pad.LRTriggerAxis;
				yName = "Player" + (int)controlIndex + "_LRTrigger"+ pad.LRTriggerAxis;
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


	/// <summary>
	/// ボタン押下取得
	/// </summary>
	/// <param name="button"></param>
	/// <param name="controlIndex"></param>
	/// <returns></returns>
	public static bool GetPress(Button button, Index controlIndex = Index.Active)
	{
		if (controlIndex == Index.Any)
		{
			
			for (int iPad = 0; iPad < (int)Index.Num; iPad++){
				PadData p = instance.padData[(int)iPad];
				if (p.Now[(int)button] == true)
				{
					return true;
				}
			}
			return false;

		}else if (controlIndex == Index.Active)
		{
			controlIndex = (Index)ActivePadIndex;
		}

		//string code = GetButtonName(button, controlIndex);
		//return Input.GetButton(code);

		PadData pad = instance.padData[(int)controlIndex];
		return (pad.Now[(int)button] == true);
	}

	/// <summary>
	/// ボタントリガー取得(押し始め)
	/// </summary>
	/// <param name="button"></param>
	/// <param name="controlIndex"></param>
	/// <returns></returns>
	public static bool GetTrigger(Button button, Index controlIndex = Index.Active)
	{
		if (controlIndex == Index.Any)
		{

			for (int iPad = 0; iPad < (int)Index.Num; iPad++)
			{
				PadData p = instance.padData[(int)iPad];
				if (p.Prev[(int)button] == false && p.Now[(int)button] == true)
				{
					return true;
				}
			}
			return false;

		}
		else if (controlIndex == Index.Active)
		{
			controlIndex = (Index)ActivePadIndex;
		}

		PadData pad = instance.padData[(int)controlIndex];
		return (pad.Prev[(int)button] == false && pad.Now[(int)button] == true);
	}

	/// <summary>
	/// ボタンリリース取得(押し終わり)
	/// </summary>
	/// <param name="button"></param>
	/// <param name="controlIndex"></param>
	/// <returns></returns>
	public static bool GetRelease(Button button, Index controlIndex = Index.Active)
	{
		if (controlIndex == Index.Any)
		{

			for (int iPad = 0; iPad < (int)Index.Num; iPad++)
			{
				PadData p = instance.padData[(int)iPad];
				if (p.Prev[(int)button] == true && p.Now[(int)button] == false)
				{
					return true;
				}
			}
			return false;

		}
		else if (controlIndex == Index.Active)
		{
			controlIndex = (Index)ActivePadIndex;
		}


		PadData pad = instance.padData[(int)controlIndex];
		return (pad.Prev[(int)button] == true && pad.Now[(int)button] == false);
	}

	/// <summary>
	/// ボタンリピート取得
	/// </summary>
	/// <param name="button"></param>
	/// <param name="controlIndex"></param>
	/// <returns></returns>
	public static bool GetRepeat(Button button, Index controlIndex = Index.Active)
	{
		if (controlIndex == Index.Any)
		{

			for (int iPad = 0; iPad < (int)Index.Num; iPad++)
			{
				PadData p = instance.padData[(int)iPad];
				if (p.Repeat[(int)button] == true)
				{
					return true;
				}
			}
			return false;

		}
		else if (controlIndex == Index.Active)
		{
			controlIndex = (Index)ActivePadIndex;
		}


		PadData pad = instance.padData[(int)controlIndex];
		return (pad.Repeat[(int)button] == true);
	}




	public static bool GetRawButton(int button, Index controlIndex = Index.Active)
	{

		if (controlIndex == Index.Active)
		{
			controlIndex = (Index)ActivePadIndex;
		}

		return Input.GetButton("Player" + (int)controlIndex + "_Btn" + button);
	}

	/*
	static string GetButtonName(Button button, Index controlIndex)
	{

		if (button < Button.Max)
		{
			return "Player" + (int)controlIndex + "_Btn" + instance.padData[(int)controlIndex].ConvTable[(int)button];
		}

		return "none";
	}*/



	// Use this for initialization
	void Start () {


		for (int iPad = 0; iPad <(int)Index.Num; iPad++)
		{
			
			padData[iPad] = new PadData();
			
			// とりあえず連番で初期化
			for (int ibtn = 0; ibtn <(int) Button.Max; ibtn++)
			{
				padData[iPad].ConvTable[ibtn] = ibtn;

			
			}
			padData[iPad].isXbox = false;

			try
			{
				padData[iPad].JoyStickName = Input.GetJoystickNames()[iPad];


				// 360コントローラを名前で判定
				if (padData[iPad].JoyStickName.IndexOf("XBOX 360") >= 0)
				{
					padData[iPad].isXbox = true;
				}

			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message);
			}
		}
	}

}
