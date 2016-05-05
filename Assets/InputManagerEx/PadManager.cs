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



using EditorGUIUtils;

/// <summary>
/// パッドマネージャー・パッドコンフィグ付き
/// </summary>
public class PadManager : MonoBehaviour {


	static private PadManager instance;
	static public PadManager GetInstance() { return instance; }

	static private bool IsPadConfig = false; /// パッドコンフィグ開始
	static private int AxisConfigStep = 0;		///
	static private int AxisConfigIndex = 0;

	static private bool IsButtonConfig = false;
	static private int ButtonConfigStep = 0;
	static private bool IsPushButtonAtConfig = false; // ボタン押した

	static private int ActivePadIndex = 0; // アクティブなパッドNo

	const int PadButtonMax = 16; // ボタンの最大数

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

		// スティックの中心
		public Vector2 LAxisOffset;
		public Vector2 RAxisOffset; 

		public int[] ConvTable = new int[(int)Button.Max]; // 変換テーブル

		public bool[] Prev = new bool[(int)Button.Max]; // 前のフレームの押下情報
		public bool[] Now = new bool[(int)Button.Max]; // 現のフレームの押下情報

		public float[] RepeatWait = new float[(int)Button.Max];
		public bool[] Repeat = new bool[(int)Button.Max]; // キーリピート


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


	/// <summary>
	/// ボタンのコンフィグ
	/// </summary>
	/// <returns></returns>
	IEnumerator ButtonSettingFunc()
	{
		IsPushButtonAtConfig = false;

		PadData pad = instance.padData[ActivePadIndex];

		for (int iButton = 0; iButton < (int)Button.UP/*Button.Max*/; iButton++)
		{
			Debug.Log(Enum.GetName(typeof(Button), iButton) + "のボタン設定");

			ButtonConfigStep = iButton;

			while (true)
			{
				for (int i = 0; i < PadButtonMax; i++)
				{

					if (GetRawButton(i, (Index)ActivePadIndex))
					{
						pad.ConvTable[iButton] = i;
						IsPushButtonAtConfig = true;
					}
				}
				if (IsPushButtonAtConfig)
				{
					Debug.Log(Enum.GetName(typeof(Button), iButton) + "のボタン設定完了！");
					break;
				}

				yield return new WaitForSeconds(0.1f);
			}
			
			yield return new WaitForSeconds(1.0f);
			IsPushButtonAtConfig = false;
		}

		// 変換テーブル
		for (int i = 0; i < (int)Button.Max; i++)
		{
			PlayerPrefs.SetInt(pad.JoyStickName + "_" + Enum.GetName(typeof(Button), i), pad.ConvTable[i]);
		}


		Debug.Log("設定終わり");
		IsButtonConfig = false;

		yield break;


	}

	
	/// <summary>
	/// パッドコンフィグ
	/// </summary>
	/// <returns></returns>
	IEnumerator AxisSettingFunc()
	{
		int padIndex = 0;

		AxisConfigStep = 0;
		AxisConfigIndex = 0;
		//ActivePadIndex = 0;

		// パッド名初期化

		string[] JoyName = Input.GetJoystickNames();
		for (int iPad = 0; iPad < (int)Index.Num; iPad++)
		{
			//try
			if (iPad < JoyName.Length-1){
				padData[iPad].JoyStickName = JoyName[iPad];
			}
			//catch (Exception ex)
			//{
			//	Debug.Log(ex.Message);
			//}
		}

		//-----------------------------
		// アクティブなパッドチェック
		//-----------------------------
		while (true)
		{


			bool bDecide = false;
			for (int i = 0; i < 6; i++)
			{
				//if (Input.GetButton("Player" + padIndex + "_Btn" + i))
				if (GetRawButton(i, (Index)padIndex))
				{
					AxisConfigStep++;
					AxisConfigIndex = 0;

					PadData pad = instance.padData[(int)padIndex];

					Debug.Log("アクティブなパッド決定" + padIndex + " " + pad.JoyStickName);
					ActivePadIndex = padIndex;

					bDecide = true;

					// OKボタンをゼロ番に割当

					pad.ConvTable[i] = (int)Button.A;
					pad.ConvTable[(int)Button.A] = i;


					// 一旦スティックのキャリブレーションを初期化
					pad.RAxisOffset.x = 0.0f;
					pad.RAxisOffset.y = 0.0f;
					pad.LAxisOffset.x = 0.0f;
					pad.LAxisOffset.y = 0.0f;

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
		while (true)
		{


			PadData pad = instance.padData[(int)padIndex];
			if (AxisConfigIndex == 1) { pad.RightAxisY = "_1"; } else { pad.RightAxisY = ""; }
			// 見つかった
			if (GetAxis(Axis.RightStick, (Index)padIndex,true).y <= -0.8f)
			{
				Debug.Log("右スティックY軸めっけ");

				AxisConfigStep++;
				AxisConfigIndex = 0;
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
				}
				else
				{
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
			if (Mathf.Abs(GetAxis(Axis.RightStick, (Index)padIndex).y) <= 0.8f)
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
			if (AxisConfigIndex == 1) { pad.RightAxisX = "_1"; } else { pad.RightAxisX = ""; }
			// 見つかった
			if (GetAxis(Axis.RightStick, (Index)padIndex, true).x >= 0.8f)
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
				}
				else
				{
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
			if (GetAxis(Axis.POV, (Index)padIndex, true).y <= -0.8f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;
				Debug.Log("POV Y軸めっけ" + GetAxis(Axis.POV, (Index)padIndex, true).y.ToString());
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
			if (GetAxis(Axis.POV, (Index)padIndex, true).x >= 0.8f)
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
			if (Mathf.Abs(GetAxis(Axis.LRTrigger, (Index)padIndex, true).x) >= 0.8f)
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
				for (int i = 0; i < PadButtonMax; i++)
				{
					if (Input.GetButton("Player" + padIndex + "_Btn" + i))
					{
						bDecide = true;
					}
				}

				if (bDecide)
				{

					AxisConfigStep++;
					AxisConfigIndex = 0;

					pad.LRTriggerAxis = "_none";


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
			}

			yield return new WaitForSeconds(0.2f);
		}


		yield return new WaitForSeconds(1.0f);


		{// スティックキャリブレーション

			PadData pad = instance.padData[(int)padIndex];
			pad.LAxisOffset = GetAxis(Axis.LeftStick, (Index)ActivePadIndex,true);
			pad.RAxisOffset = GetAxis(Axis.RightStick, (Index)ActivePadIndex,true);

		}




		Debug.Log("終わり");


		//------------------------
		// 設定保存
		//------------------------
		PlayerPrefs.SetInt("ActivePad", ActivePadIndex);

		{
			// パッド設定
			PadData p = instance.padData[(int)ActivePadIndex];
			if (p.JoyStickName.Length > 0)
			{

				PlayerPrefs.SetString(p.JoyStickName + "_RightAxisX", p.RightAxisX);
				PlayerPrefs.SetString(p.JoyStickName + "_RightAxisY", p.RightAxisY);
				PlayerPrefs.SetString(p.JoyStickName + "_PovX", p.PovX);
				PlayerPrefs.SetString(p.JoyStickName + "_PovY", p.PovY);

				// 軸ズレ
				PlayerPrefs.SetFloat(p.JoyStickName + "_RightAxisX_off", p.RAxisOffset.x);
				PlayerPrefs.SetFloat(p.JoyStickName + "_RightAxisY_off", p.RAxisOffset.y);
				PlayerPrefs.SetFloat(p.JoyStickName + "_LeftAxisX_off", p.LAxisOffset.x);
				PlayerPrefs.SetFloat(p.JoyStickName + "_LeftAxisY_off", p.LAxisOffset.y);

				PlayerPrefs.SetInt(p.JoyStickName + "_isXbox", p.isXbox == false ? 1 : 2);

			}
		}



		IsPadConfig = false;

		yield break;
	}


	// ボタンデバッグ用
	private delegate bool PadPressFunc(Button button, Index controlIndex);


	/// <summary>
	/// デバッグ
	/// </summary>
	void OnGUI()
	{

		//String pad = "";


		{// FPS
			float fps = 1f / Time.deltaTime;
			GUI.Label(new Rect(0, 0, 300, 20), fps.ToString("#.#") + "fps");

		}


		float size = 30.0f;

		if (GUI.Button(new Rect(8, 20, 180, 24), "パッドコンフィグ開始"))
		{
			IsPadConfig = true;
			AxisConfigStep = 0;
			AxisConfigIndex = 0;

			StartCoroutine("AxisSettingFunc");

		}
		
		if (GUI.Button(new Rect(400, 20, 180, 24), "ボタンコンフィグ開始"))
		{

			IsButtonConfig = true;
			ButtonConfigStep = 0;
			StartCoroutine("ButtonSettingFunc");

		}



		


		if (IsPadConfig)
		{

			switch (AxisConfigStep)
			{
				case 0: GUI.Label(new Rect(50, 50, 350, 20), "使うパットのOKボタン押して！"); break;
				case 1: GUI.Label(new Rect(50, 50, 300, 20), "右スティックY軸チェック 上に倒して！\n無い場合はOKボタン押して！"); break;
				case 2: GUI.Label(new Rect(50, 50, 300, 20), "右スティックX軸チェック 右に倒して！\n無い場合はOKボタン押して！"); break;
				case 3: GUI.Label(new Rect(50, 50, 300, 20), "十字キー(POV) Y チェック 上押して！"); break;
				case 4: GUI.Label(new Rect(50, 50, 300, 20), "十字キー(POV) X チェック 右押して！"); break;
				case 5: GUI.Label(new Rect(50, 50, 300, 20), "スティックに触らずにL2/R2どっちか押して！\n(Xbox360判定)"); break;

			}



		}
		else if (IsButtonConfig)
		{

			if (IsPushButtonAtConfig == false)
			{
				GUI.Label(new Rect(50, 50, 350, 20), Enum.GetName(typeof(Button), ButtonConfigStep) + "のボタン設定");

			}
			else
			{
				GUI.Label(new Rect(50, 50, 350, 20), Enum.GetName(typeof(Button), ButtonConfigStep) + "のボタン設定完了!");
			}

		}else
		{



			float startY = 100;
			float startX = 100;

			GUI.Box(new Rect(10, startY - 50, 650, 520), "");

			//GUI.Label(new Rect(startX, startY - 50, 100, 20), "左");
			//GUI.Label(new Rect(startX + 50, startY - 50, 100, 20), "右");
			//GUI.Label(new Rect(startX + 100, startY - 50, 100, 20), "POV");

			for (int iPad = 0; iPad < (int)Index.Num; iPad++)
			{

				PadData pad = padData[iPad];

				if (pad.JoyStickName == null)
				{
					continue;
				}

				const int YOffset = 100;
				if (ActivePadIndex == iPad)
				{
					GUI.Label(new Rect(startX - 90, startY + YOffset * iPad - 40, 100, 20), "Active↓");
				}


				GUI.Label(new Rect(startX - 90, startY + YOffset * iPad - 30, 100, 20), pad.JoyStickName);



				//-------------------------------
				// スティック
				//-------------------------------
				float centerX = startX-30;
				float centerY = startY + YOffset * iPad + 20;
				for (int iAxis = 0; iAxis <(int)Axis.MAX-1; iAxis++)
				{

					GUIHelper.DrawRect(new Rect(centerX - size, centerY - size, size * 2, size * 2), Color.white);
					//GUIHelper.DrawCircle(new Vector2(centerX, centerY),size, Color.white);

					GUIHelper.DrawRect(new Rect(centerX, centerY, 1, 1), Color.white);

					GUIHelper.DrawRect(new Rect(centerX + GetAxis((Axis)iAxis, (Index)iPad).x * size,
												centerY + GetAxis((Axis)iAxis, (Index)iPad).y * size, 1, 1), Color.yellow);


					GUIHelper.DrawRect(new Rect(centerX + GetAxis((Axis)iAxis, (Index)iPad,true).x * size,
							centerY + GetAxis((Axis)iAxis, (Index)iPad, true).y * size, 1, 1), Color.cyan);
					
					// スティック座標デバッグ
					Vector2 laxis = GetAxis((Axis)iAxis, (Index)iPad) * size;
					pad.posHistory[iAxis].pos.Enqueue(laxis);

					Vector2 laxisraw = GetAxis((Axis)iAxis, (Index)iPad, true) * size;
					pad.posHistory[iAxis].posRaw.Enqueue(laxisraw);


					//if (laxis.x > 0.0f)
					//{
					//Debug.Log(laxis.x.ToString());
					//Debug.Log(laxisraw.x.ToString());

					//}



					if (pad.posHistory[iAxis].pos.Count > 60)
					{
						pad.posHistory[iAxis].pos.Dequeue();

						pad.posHistory[iAxis].posRaw.Dequeue();
					}



					{// 生データ
						Vector2 center = new Vector2(centerX, centerY);
						Vector2 prev = pad.posHistory[iAxis].posRaw.Peek();
						foreach (Vector2 pos in pad.posHistory[iAxis].posRaw)
						{

							//GUI.Label(new Rect(startX + 50 + pos.x * size, startY + YOffset * iPad - pos.y * size, 100, 20), "+");

							Vector2 start = new Vector2(prev.x, prev.y);
							Vector2 end = new Vector2(pos.x, pos.y);
							GUIHelper.DrawLine(center + start, center + end, Color.cyan);

							prev = pos;
						}
					}

					{ // 加工後データ
						Vector2 center = new Vector2(centerX, centerY);
						Vector2 prev = pad.posHistory[iAxis].pos.Peek();
						foreach (Vector2 pos in pad.posHistory[iAxis].pos)
						{

							//GUI.Label(new Rect(startX + 50 + pos.x * size, startY + YOffset * iPad - pos.y * size, 100, 20), "+");

							Vector2 start = new Vector2(prev.x, prev.y);
							Vector2 end = new Vector2(pos.x, pos.y);
							GUIHelper.DrawLine(center + start, center + end, Color.yellow);

							prev = pos;
						}
					}

					


					

					centerX += 70;

				}



				// アナログ値
				GUI.Label(new Rect(startX - 20, startY + YOffset * iPad + 50, 100, 20), GetAxis(Axis.LeftStick, (Index)iPad).x.ToString("0.00"));
				GUI.Label(new Rect(startX - 20, startY + YOffset * iPad + 60, 100, 20), GetAxis(Axis.LeftStick, (Index)iPad).y.ToString("0.00"));
				GUI.Label(new Rect(startX - 20, startY + YOffset * iPad + 70, 100, 20), GetAxis(Axis.LeftStick, (Index)iPad, true).x.ToString("0.00"));
				GUI.Label(new Rect(startX - 20, startY + YOffset * iPad + 80, 100, 20), GetAxis(Axis.LeftStick, (Index)iPad, true).y.ToString("0.00"));


				GUI.Label(new Rect(startX + 30, startY + YOffset * iPad + 50, 100, 20), GetAxis(Axis.RightStick, (Index)iPad).x.ToString("0.00"));
				GUI.Label(new Rect(startX + 30, startY + YOffset * iPad + 60, 100, 20), GetAxis(Axis.RightStick, (Index)iPad).y.ToString("0.00"));
				GUI.Label(new Rect(startX + 30, startY + YOffset * iPad + 70, 100, 20), GetAxis(Axis.RightStick, (Index)iPad, true).x.ToString("0.00"));
				GUI.Label(new Rect(startX + 30, startY + YOffset * iPad + 80, 100, 20), GetAxis(Axis.RightStick, (Index)iPad, true).y.ToString("0.00"));



				float BtnX = startX + 150;
				float BtnY = startY + YOffset * iPad - 20;


			
				// ボタンRaw
				for (int button = 0; button < PadButtonMax; button++)
				{

					Rect r = new Rect(BtnX + 20 * button, BtnY, 5, 5);
					if (GetRawButton(button, (Index)iPad))
					{
						GUIHelper.DrawRect(r, Color.yellow, 3);
					}
					else
					{
						GUIHelper.DrawRect(r, Color.white);
					}
				}

				// LT/RT
				//GUI.Label(new Rect(startX + 150 + 20 * 16, startY + YOffset * iPad - 20, 100, 20), GetAxis(Axis.LRTrigger, (Index)iPad).y.ToString("0.00"));



				// 加工後のボタン押下情報
				PadPressFunc[] funcArray = new PadPressFunc[] { GetTrigger, GetPress, GetRepeat, GetRelease };
				foreach (PadPressFunc func in funcArray)
				{
					BtnY += 15;
					for (int button = 0; button < (int)Button.Max; button++)
					{

						Rect r = new Rect(BtnX + 20 * button, BtnY, 5, 5);

						if (func((Button)button, (Index)iPad))
						{
							GUIHelper.DrawRect(r, Color.yellow, 3);
						}
						else
						{
							GUIHelper.DrawRect(r, Color.white);
						}
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
				switch ((Button)button)
				{
					case Button.UP:
						if (GetAxis(Axis.LeftStick, (Index)iPad).y <= -0.8f ||
							 GetAxis(Axis.POV, (Index)iPad).y <= -0.8f)
						{
							pad.Now[(int)Button.UP] = true;
						}else{
							pad.Now[(int)Button.UP] = false;
						}
						break;
					case Button.DOWN:
						if (GetAxis(Axis.LeftStick, (Index)iPad).y >= 0.8f ||
							GetAxis(Axis.POV, (Index)iPad).y >= 0.8f)
						{
							pad.Now[(int)Button.DOWN] = true;
						}
						else
						{
							pad.Now[(int)Button.DOWN] = false;
						}
						break;
					case Button.LEFT:
						if (GetAxis(Axis.LeftStick, (Index)iPad).x <= -0.8f ||
							GetAxis(Axis.POV, (Index)iPad).x <= -0.8f)
						{
							pad.Now[(int)Button.LEFT] = true;

						}
						else
						{
							pad.Now[(int)Button.LEFT] = false;
						}
						break;
					case Button.RIGHT:

						if (GetAxis(Axis.LeftStick, (Index)iPad).x >= 0.8f ||
							GetAxis(Axis.POV, (Index)iPad).x >= 0.8f)
						{
							pad.Now[(int)Button.RIGHT] = true;

						}
						else
						{
							pad.Now[(int)Button.RIGHT] = false;
						}
						break;
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
					if (pad.isXbox)
					{
						axisXY.y *= 2.0f;
					}

	
					// 極座標変換 + デッドゾーン処理(非線形)
					//http://www.kawaz.org/blogs/isekaf/2013/12/23/564/
					float angle = Mathf.Atan2(axisXY.y, axisXY.x);
					float power = Mathf.Sqrt(Mathf.Pow(axisXY.x, 2) + Mathf.Pow(axisXY.y, 2));

					//power = power * power;
					
					

					axisXY = new Vector2(power * Mathf.Cos(angle), power * Mathf.Sin(angle));

					if (power > 1.0f)
					{
						axisXY /= axisXY.magnitude;
					}

					
				}
			}
			else
			{
				axisXY.x = Input.GetAxisRaw(xName);

				if (axis == Axis.POV)
				{
					axisXY.y = -Input.GetAxisRaw(yName);
				}
				else
				{
					axisXY.y = Input.GetAxisRaw(yName);
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
			controlIndex = (Index)ActivePadIndex;
		}

		return Input.GetButtonDown("Player" + (int)controlIndex + "_Btn" + button);
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

		// 設定取得
		ActivePadIndex = PlayerPrefs.GetInt("ActivePad");


		string[] JoyName = Input.GetJoystickNames();

		for (int iPad = 0; iPad <(int)Index.Num; iPad++)
		{
			
			padData[iPad] = new PadData();
			PadData p = padData[iPad];

			for (int i = 0; i < (int)Axis.MAX-1; i++)
			{
				p.posHistory[i] = new PadData.PosHistory();
			}


			// とりあえず連番で初期化
			for (int ibtn = 0; ibtn <(int) Button.Max; ibtn++)
			{
				p.ConvTable[ibtn] = ibtn;
			}

			//try
			{

				if (iPad > JoyName.Length-1)
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

					for (int i = 0; i < (int)Button.Max; i++)
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

}
