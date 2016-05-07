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
using System.Collections.Generic;
using System;
//using UnityEditor;




/// <summary>
/// パッドマネージャー・パッドコンフィグ付き
/// </summary>
public class PadManager : MonoBehaviour {


	static private PadManager instance;
	static public PadManager GetInstance() { return instance; }

	// アクティブなパッドNo
	public int ActivePadIndex { get; set; } 


	/// <summary>
	/// 定数
	/// </summary>
	public const int PadButtonMax = 16; // ボタンの最大数

	const int PadMax = 4;

	// キーリピート
	const float REPEAT_WAIT = 0.5f;
	const float REPEAT_INTERVAL = 0.1f;


	// デッドゾーンしきい値
	const float DEAD_ZONE = 0.08f;

	void Awake()
	{
		instance = this;
	}

	// 軸定義
	public enum Axis { 
		LeftStick,	/// LStick
		RightStick, /// RStick
		POV,			/// POV
		LRTrigger,	/// 360コントローラのLT/RT
					/// 
		MAX,
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
	public enum Button { A, B, X, Y, LB, RB, Back, Start, LS, RS, LT, RT, UP, RIGHT, DOWN,LEFT, MAX }

	/// <summary>
	/// パッドデータ
	/// </summary>
	public class PadData
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

		// スティックの中心
		public Vector2 LAxisOffset;
		public Vector2 RAxisOffset; 

		public int[] ConvTable = new int[(int)Button.MAX]; // 変換テーブル

		public bool[] Prev = new bool[(int)Button.MAX]; // 前のフレームの押下情報
		public bool[] Now = new bool[(int)Button.MAX]; // 現のフレームの押下情報

		public float[] RepeatWait = new float[(int)Button.MAX];
		public bool[] Repeat = new bool[(int)Button.MAX]; // キーリピート



		public Queue<Button> PadHistory = new Queue<Button>();

//#if DEBUG
		// スティック座標デバッグ表示
		public class PosHistory{
			public Queue<Vector2> pos = new Queue<Vector2>();
			public Queue<Vector2> posRaw = new Queue<Vector2>();
		};
		public PosHistory[] posHistory = new PosHistory[(int)Axis.MAX];
//#endif
	};


	PadData[] padData = new PadData[(int)Index.Num];






	// Use this for initialization
	void Start()
	{

		// 設定取得
		ActivePadIndex = PlayerPrefs.GetInt("ActivePad");


		string[] JoyName = Input.GetJoystickNames();

		for (int iPad = 0; iPad < (int)Index.Num; iPad++)
		{

			padData[iPad] = new PadData();
			PadData p = padData[iPad];

			for (int i = 0; i < (int)Axis.MAX - 1; i++)
			{
				p.posHistory[i] = new PadData.PosHistory();
			}


			// とりあえず連番で初期化
			for (int ibtn = 0; ibtn < (int)Button.MAX; ibtn++)
			{
				p.ConvTable[ibtn] = ibtn;
			}

			//try
			{

				if (iPad > JoyName.Length - 1)
				{
					continue;
				}
				p.JoyStickName = JoyName[iPad];



				p.RightAxisX = PlayerPrefs.GetString(p.JoyStickName + "_RightAxisX");
				p.RightAxisY = PlayerPrefs.GetString(p.JoyStickName + "_RightAxisY");
				p.PovX = PlayerPrefs.GetString(p.JoyStickName + "_PovX");
				p.PovY = PlayerPrefs.GetString(p.JoyStickName + "_PovY");

				p.RAxisOffset.x = PlayerPrefs.GetFloat(p.JoyStickName + "_RightAxisX_off");
				p.RAxisOffset.y = PlayerPrefs.GetFloat(p.JoyStickName + "_RightAxisY_off");
				p.LAxisOffset.x = PlayerPrefs.GetFloat(p.JoyStickName + "_LeftAxisX_off");
				p.LAxisOffset.y = PlayerPrefs.GetFloat(p.JoyStickName + "_LeftAxisY_off");

				// 変換テーブル
				if (PlayerPrefs.HasKey(p.JoyStickName + "_" + Enum.GetName(typeof(Button), 0)))
				{
					Debug.Log("パッド変換テーブル見っけ");

					for (int i = 0; i < (int)Button.MAX; i++)
					{
						p.ConvTable[i] = PlayerPrefs.GetInt(p.JoyStickName + "_" + Enum.GetName(typeof(Button), i));
					}
				}


				if (PlayerPrefs.GetInt(p.JoyStickName + "_isXbox") == 0)
				{
					// 初回
					// 360コントローラを名前で判定
					if (p.JoyStickName.IndexOf("XBOX 360") >= 0)
					{
						p.isXbox = true;
					}
				}
				else
				{
					p.isXbox = PlayerPrefs.GetInt(p.JoyStickName + "_isXbox") == 2;
				}




			}
			//catch (Exception ex)
			{
				//	Debug.Log(ex.Message);
			}
		}
	}

	/// <summary>
	///  パッドデータ取得
	/// </summary>
	/// <param name="ind"></param>
	/// <returns></returns>
	static public PadData GetPadData(Index ind) { return instance.padData[(int)ind]; }


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

			for (int button = 0; button < (int)Button.MAX; button++)
			{

				
				pad.Prev[button] = pad.Now[button];

				// ボタンチェック
				int convButton = pad.ConvTable[button];
				if (GetRawButton(convButton, (Index)iPad))
				{
					pad.Now[button] = true;
				}
				else
				{
					pad.Now[button] = false;
				}

				//---------------------------
				// Xbox360コントローラ固有
				//---------------------------
				if (pad.isXbox==true)
				{
					UpdateX360(ref pad, (Index)iPad, (Button)button);

				}


				// 左スティックを方向キーとして使う
				// L2/R2がアナログなので。。。
				const float StickSlide = 0.5f;
				switch ((Button)button)
				{
					case Button.UP:
						if (GetAxis(Axis.LeftStick, (Index)iPad).y <= -StickSlide ||
							 GetAxis(Axis.POV, (Index)iPad).y <= -StickSlide)
						{
							pad.Now[(int)Button.UP] = true;
						}else{
							pad.Now[(int)Button.UP] = false;
						}
						break;
					case Button.DOWN:
						if (GetAxis(Axis.LeftStick, (Index)iPad).y >= StickSlide ||
							GetAxis(Axis.POV, (Index)iPad).y >= StickSlide)
						{
							pad.Now[(int)Button.DOWN] = true;
						}
						else
						{
							pad.Now[(int)Button.DOWN] = false;
						}
						break;
					case Button.LEFT:
						if (GetAxis(Axis.LeftStick, (Index)iPad).x <= -StickSlide ||
							GetAxis(Axis.POV, (Index)iPad).x <= -StickSlide)
						{
							pad.Now[(int)Button.LEFT] = true;

						}
						else
						{
							pad.Now[(int)Button.LEFT] = false;
						}
						break;
					case Button.RIGHT:

						if (GetAxis(Axis.LeftStick, (Index)iPad).x >= StickSlide ||
							GetAxis(Axis.POV, (Index)iPad).x >= StickSlide)
						{
							pad.Now[(int)Button.RIGHT] = true;

						}
						else
						{
							pad.Now[(int)Button.RIGHT] = false;
						}
						break;
				}

				// ボタンを押してたら履歴に入れる
				if (pad.Now[button] == true && pad.Prev[button] != true )
				{
	

					pad.PadHistory.Enqueue((Button)button);
					if (pad.PadHistory.Count > 32)
					{
						pad.PadHistory.Dequeue();
					}
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
					if (GetAxis(Axis.LRTrigger, (Index)iPad).y > 0.5f)
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
					if (GetAxis(Axis.LRTrigger, (Index)iPad).y < -0.5f)
					{

						pad.Now[(int)Button.RT] = true;
					}
					else
					{
						pad.Now[(int)Button.RT] = false;
					}
				}
				break;
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
	public static Vector2 GetAxis(Axis axis, Index controlIndex = Index.Active, bool raw = false)
	{

		if (controlIndex == Index.Active)
		{
			controlIndex = (Index)instance.ActivePadIndex;
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
				if (axis == Axis.POV)
				{
					axisXY.y = -Input.GetAxis(yName);
				}
				else
				{
					axisXY.y = Input.GetAxis(yName);
				}
				// todo:ズレ補正
				/*
				if (axis == Axis.LeftStick)
				{
					axisXY -= pad.LAxisOffset;
				}
				else if (axis == Axis.RightStick)
				{
					axisXY -= pad.RAxisOffset;
				}
				*/

				// デッド・ゾーン
				/*
				if (axis == Axis.LeftStick)
				{
					if (Mathf.Abs(axisXY.x) < Mathf.Abs(pad.LAxisOffset.x) + DEAD_ZONE)
					{
						axisXY.x = 0;
					}
					if (Mathf.Abs(axisXY.y) < Mathf.Abs(pad.LAxisOffset.y) + DEAD_ZONE)
					{
						axisXY.y = 0;
					}
				}
				if (axis == Axis.RightStick)
				{
					if (Mathf.Abs(axisXY.x) < Mathf.Abs(pad.RAxisOffset.x) + DEAD_ZONE)
					{
						axisXY.x = 0;
					}
					if (Mathf.Abs(axisXY.y) < Mathf.Abs(pad.RAxisOffset.y) + DEAD_ZONE)
					{
						axisXY.y = 0;
					}
				}
				*/

				if (axis == Axis.LeftStick || axis == Axis.RightStick)
				{
					// 360パッド等はY軸のUP方向の移動量が少なくて綺麗な円にならないので補正
					//if (pad.isXbox)
					{
						axisXY.y *= 1.2f;
					}

	
					// 極座標変換 + デッドゾーン処理(非線形)
					//http://www.kawaz.org/blogs/isekaf/2013/12/23/564/
					float angle = Mathf.Atan2(axisXY.y, axisXY.x);
					float power = Mathf.Sqrt(Mathf.Pow(axisXY.x, 2) + Mathf.Pow(axisXY.y, 2));

					//power = power * power;
					// デッドゾーン
					if (power < 0.15f)
					{
						power = 0.0f;
					}

					axisXY = new Vector2(power * Mathf.Cos(angle), power * Mathf.Sin(angle));


					
					if (power > 1.0f)
					{
						axisXY /= axisXY.magnitude;
					}

					
				}
			}
			else
			{
				axisXY.x = Input.GetAxis(xName);

				if (axis == Axis.POV)
				{
					axisXY.y = -Input.GetAxis(yName);
				}
				else
				{
					axisXY.y = Input.GetAxis(yName);
				}
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
			controlIndex = (Index)instance.ActivePadIndex;
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
			controlIndex = (Index)instance.ActivePadIndex;
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
			controlIndex = (Index)instance.ActivePadIndex;
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
			controlIndex = (Index)instance.ActivePadIndex;
		}


		PadData pad = instance.padData[(int)controlIndex];
		return (pad.Repeat[(int)button] == true);
	}



	/// <summary>
	/// 番号指定でボタンの状態取得(Update以外では取得不可)
	/// </summary>
	/// <param name="button"></param>
	/// <param name="controlIndex"></param>
	/// <returns></returns>
	public static bool GetRawButtonTrigger(int button, Index controlIndex = Index.Active)
	{

		if (controlIndex == Index.Active)
		{
			controlIndex = (Index)instance.ActivePadIndex;
		}

		return Input.GetButtonDown("Player" + (int)controlIndex + "_Btn" + button);
	}

	public static bool GetRawButton(int button, Index controlIndex = Index.Active)
	{

		if (controlIndex == Index.Active)
		{
			controlIndex = (Index)instance.ActivePadIndex;
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



}
