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

using EditorGUIUtils;

public class PadConfig : MonoBehaviour {


	// パッドコンフィグ関連
	static private bool IsPadConfig = false; /// パッドコンフィグ開始
	static private int AxisConfigStep = 0;		///
	static private int AxisConfigIndex = 0;

	static private bool IsButtonConfig = false;
	static private int ButtonConfigStep = 0;
	static private bool IsPushButtonAtConfig = false; // ボタン押した



	//--------------------------



	// Use this for initialization
	void Start()
	{

	}

	/// <summary>
	/// ボタンのコンフィグ
	/// </summary>
	/// <returns></returns>
	IEnumerator ButtonSettingFunc()
	{
		IsPushButtonAtConfig = false;

		PadManager.PadData pad = PadManager.GetPadData((PadManager.Index)PadManager.GetInstance().ActivePadIndex);

		for (int iButton = 0; iButton < (int)PadManager.Button.UP/*Button.Max*/; iButton++)
		{
			Debug.Log(Enum.GetName(typeof(PadManager.Button), iButton) + "のボタン設定");

			ButtonConfigStep = iButton;

			while (true)
			{
				for (int i = 0; i < PadManager.PadButtonMax; i++)
				{

					if (PadManager.GetRawButton(i, (PadManager.Index)PadManager.GetInstance().ActivePadIndex))
					{
						pad.ConvTable[iButton] = i;
						IsPushButtonAtConfig = true;
					}

				}
				if (IsPushButtonAtConfig)
				{
					Debug.Log(Enum.GetName(typeof(PadManager.Button), iButton) + "のボタン設定完了！");
					break;
				}

				yield return new WaitForSeconds(0.1f);
			}

			yield return new WaitForSeconds(1.0f);
			IsPushButtonAtConfig = false;
		}

		// 変換テーブル
		for (int i = 0; i < (int)PadManager.Button.MAX; i++)
		{
			PlayerPrefs.SetInt(pad.JoyStickName + "_" + Enum.GetName(typeof(PadManager.Button), i), pad.ConvTable[i]);
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
		for (int iPad = 0; iPad < (int)PadManager.Index.Num; iPad++)
		{
			//try
			if (iPad < JoyName.Length - 1)
			{
				PadManager.GetPadData((PadManager.Index)iPad).JoyStickName = JoyName[iPad];
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
				if (PadManager.GetRawButton(i, (PadManager.Index)padIndex))
				{
					AxisConfigStep++;
					AxisConfigIndex = 0;

					PadManager.PadData p = PadManager.GetPadData((PadManager.Index)padIndex);

					Debug.Log("アクティブなパッド決定" + padIndex + " " + p.JoyStickName);
					PadManager.GetInstance().ActivePadIndex = padIndex;

					bDecide = true;

					// OKボタンをゼロ番に割当

					p.ConvTable[i] = (int)PadManager.Button.A;
					p.ConvTable[(int)PadManager.Button.A] = i;


					// 一旦スティックのキャリブレーションを初期化
					p.RAxisOffset.x = 0.0f;
					p.RAxisOffset.y = 0.0f;
					p.LAxisOffset.x = 0.0f;
					p.LAxisOffset.y = 0.0f;

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
				if (padIndex > (int)PadManager.Index._4P) padIndex = 0;

				yield return new WaitForSeconds(0.1f);

			}
		}

		yield return new WaitForSeconds(1.0f);

		PadManager.PadData pad = PadManager.GetPadData((PadManager.Index)padIndex);

		//-----------------------------
		// 右スティックY軸チェック
		//-----------------------------
		while (true)
		{


			//PadData pad = instance.padData[(int)padIndex];
			
			if (AxisConfigIndex == 1) { pad.RightAxisY = "_1"; } else { pad.RightAxisY = ""; }
			// 見つかった
			if (PadManager.GetAxis(PadManager.Axis.RightStick, (PadManager.Index)padIndex, true).y <= -0.5f)
			{
				Debug.Log("右スティックY軸めっけ");

				AxisConfigStep++;
				AxisConfigIndex = 0;
				break;
			}
			else
			{

				if (PadManager.GetPress(PadManager.Button.A))
				{
					// 右スティックなし
					AxisConfigStep++;
					pad.RightAxisY = "_none";

					Debug.Log("右スティック無し");
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

		//-----------------------------
		// 真ん中に戻すまで待つ
		//-----------------------------
		while (true)
		{
			if (Mathf.Abs(PadManager.GetAxis(PadManager.Axis.RightStick, (PadManager.Index)padIndex).y) <= 0.5f)
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

			//PadData pad = instance.padData[(int)padIndex];
			if (AxisConfigIndex == 1) { pad.RightAxisX = "_1"; } else { pad.RightAxisX = ""; }
			// 見つかった
			if (PadManager.GetAxis(PadManager.Axis.RightStick, (PadManager.Index)padIndex, true).x >= 0.5f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;
				Debug.Log("右スティックX軸めっけ");
				break;

			}
			else
			{

				if (PadManager.GetPress(PadManager.Button.A))
				{
					// 右スティックなし
					AxisConfigStep++;
					pad.RightAxisX = "_none";

					Debug.Log("右スティック無し");
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

		//-----------------------------
		// POV Yチェック
		//-----------------------------
		while (true)
		{

			//PadData pad = instance.padData[(int)padIndex];
			if (AxisConfigIndex == 1) { pad.PovY = "_1"; } else { pad.PovY = ""; }
			// 見つかった
			if (PadManager.GetAxis(PadManager.Axis.POV, (PadManager.Index)padIndex, true).y <= -0.5f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;
				Debug.Log("POV Y軸めっけ" + PadManager.GetAxis(PadManager.Axis.POV, (PadManager.Index)padIndex, true).y.ToString());
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
			if (Mathf.Abs(PadManager.GetAxis(PadManager.Axis.POV, (PadManager.Index)padIndex).y) <= 0.5f)
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

			//PadData pad = instance.padData[(int)padIndex];
			if (AxisConfigIndex == 1) { pad.PovX = "_1"; } else { pad.PovX = ""; }
			// 見つかった
			if (PadManager.GetAxis(PadManager.Axis.POV, (PadManager.Index)padIndex, true).x >= 0.5f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;
				Debug.Log("POV X軸めっけ" + PadManager.GetAxis(PadManager.Axis.POV, (PadManager.Index)padIndex).x.ToString());
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
			//PadData pad = instance.padData[(int)padIndex];
			if (Mathf.Abs(PadManager.GetAxis(PadManager.Axis.LRTrigger, (PadManager.Index)padIndex, true).x) >= 0.5f)
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
				for (int i = 0; i < PadManager.PadButtonMax; i++)
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

			//PadData pad = instance.padData[(int)padIndex];
			pad.LAxisOffset = PadManager.GetAxis(PadManager.Axis.LeftStick, (PadManager.Index)PadManager.GetInstance().ActivePadIndex, true);
			pad.RAxisOffset = PadManager.GetAxis(PadManager.Axis.RightStick, (PadManager.Index)PadManager.GetInstance().ActivePadIndex, true);

		}




		Debug.Log("終わり");


		//------------------------
		// 設定保存
		//------------------------
		PlayerPrefs.SetInt("ActivePad", PadManager.GetInstance().ActivePadIndex);

		{
			// パッド設定
			//PadData pad = instance.padData[(int)PadManager.GetInstance().ActivePadIndex];
			if (pad.JoyStickName.Length > 0)
			{

				PlayerPrefs.SetString(pad.JoyStickName + "_RightAxisX", pad.RightAxisX);
				PlayerPrefs.SetString(pad.JoyStickName + "_RightAxisY", pad.RightAxisY);
				PlayerPrefs.SetString(pad.JoyStickName + "_PovX", pad.PovX);
				PlayerPrefs.SetString(pad.JoyStickName + "_PovY", pad.PovY);

				// 軸ズレ
				PlayerPrefs.SetFloat(pad.JoyStickName + "_RightAxisX_off", pad.RAxisOffset.x);
				PlayerPrefs.SetFloat(pad.JoyStickName + "_RightAxisY_off", pad.RAxisOffset.y);
				PlayerPrefs.SetFloat(pad.JoyStickName + "_LeftAxisX_off", pad.LAxisOffset.x);
				PlayerPrefs.SetFloat(pad.JoyStickName + "_LeftAxisY_off", pad.LAxisOffset.y);

				PlayerPrefs.SetInt(pad.JoyStickName + "_isXbox", pad.isXbox == false ? 1 : 2);

			}
		}



		IsPadConfig = false;

		yield break;
	}


	// ボタンデバッグ用
	private delegate bool PadPressFunc(PadManager.Button button, PadManager.Index controlIndex);


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
				GUI.Label(new Rect(50, 50, 350, 20), Enum.GetName(typeof(PadManager.Button), ButtonConfigStep) + "のボタン設定");

			}
			else
			{
				GUI.Label(new Rect(50, 50, 350, 20), Enum.GetName(typeof(PadManager.Button), ButtonConfigStep) + "のボタン設定完了!");
			}

		}
		else
		{



			float startY = 100;
			float startX = 100;

			GUI.Box(new Rect(10, startY - 50, 650, 520), "");

			//GUI.Label(new Rect(startX, startY - 50, 100, 20), "左");
			//GUI.Label(new Rect(startX + 50, startY - 50, 100, 20), "右");
			//GUI.Label(new Rect(startX + 100, startY - 50, 100, 20), "POV");

			for (int iPad = 0; iPad < (int)PadManager.Index.Num; iPad++)
			{
				PadManager.PadData pad = PadManager.GetPadData((PadManager.Index)iPad);
				//PadData pad = padData[iPad];

				if (pad.JoyStickName == null)
				{
					continue;
				}

				const int YOffset = 100;
				if (PadManager.GetInstance().ActivePadIndex == iPad)
				{
					GUI.Label(new Rect(startX - 90, startY + YOffset * iPad - 40, 100, 20), "Active↓");
				}


				GUI.Label(new Rect(startX - 90, startY + YOffset * iPad - 30, 300, 20), pad.JoyStickName);



				//-------------------------------
				// スティック
				//-------------------------------
				float centerX = startX - 30;
				float centerY = startY + YOffset * iPad + 20;
				for (int iAxis = 0; iAxis < (int)PadManager.Axis.MAX - 1; iAxis++)
				{

					GUIHelper.DrawRect(new Rect(centerX - size, centerY - size, size * 2, size * 2), Color.white);
					//GUIHelper.DrawCircle(new Vector2(centerX, centerY),size, Color.white);

					GUIHelper.DrawRect(new Rect(centerX, centerY, 1, 1), Color.white);

					GUIHelper.DrawRect(new Rect(centerX + PadManager.GetAxis((PadManager.Axis)iAxis, (PadManager.Index)iPad).x * size,
												centerY + PadManager.GetAxis((PadManager.Axis)iAxis, (PadManager.Index)iPad).y * size, 1, 1), Color.yellow);


					GUIHelper.DrawRect(new Rect(centerX + PadManager.GetAxis((PadManager.Axis)iAxis, (PadManager.Index)iPad, true).x * size,
							centerY + PadManager.GetAxis((PadManager.Axis)iAxis, (PadManager.Index)iPad, true).y * size, 1, 1), Color.cyan);

					// スティック座標デバッグ
					Vector2 laxis = PadManager.GetAxis((PadManager.Axis)iAxis, (PadManager.Index)iPad) * size;
					pad.posHistory[iAxis].pos.Enqueue(laxis);

					Vector2 laxisraw = PadManager.GetAxis((PadManager.Axis)iAxis, (PadManager.Index)iPad, true) * size;
					pad.posHistory[iAxis].posRaw.Enqueue(laxisraw);


					//if (laxis.x > 0.0f)
					//{
					//Debug.Log(laxis.x.ToString());
					//Debug.Log(laxisraw.x.ToString());

					//}



					if (pad.posHistory[iAxis].pos.Count > 120)
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
				GUI.Label(new Rect(startX - 20, startY + YOffset * iPad + 50, 100, 20), PadManager.GetAxis(PadManager.Axis.LeftStick, (PadManager.Index)iPad).x.ToString("0.00"));
				GUI.Label(new Rect(startX - 20, startY + YOffset * iPad + 60, 100, 20), PadManager.GetAxis(PadManager.Axis.LeftStick, (PadManager.Index)iPad).y.ToString("0.00"));
				GUI.Label(new Rect(startX - 20, startY + YOffset * iPad + 70, 100, 20), PadManager.GetAxis(PadManager.Axis.LeftStick, (PadManager.Index)iPad, true).x.ToString("0.00"));
				GUI.Label(new Rect(startX - 20, startY + YOffset * iPad + 80, 100, 20), PadManager.GetAxis(PadManager.Axis.LeftStick, (PadManager.Index)iPad, true).y.ToString("0.00"));


				GUI.Label(new Rect(startX + 30, startY + YOffset * iPad + 50, 100, 20), PadManager.GetAxis(PadManager.Axis.RightStick, (PadManager.Index)iPad).x.ToString("0.00"));
				GUI.Label(new Rect(startX + 30, startY + YOffset * iPad + 60, 100, 20), PadManager.GetAxis(PadManager.Axis.RightStick, (PadManager.Index)iPad).y.ToString("0.00"));
				GUI.Label(new Rect(startX + 30, startY + YOffset * iPad + 70, 100, 20), PadManager.GetAxis(PadManager.Axis.RightStick, (PadManager.Index)iPad, true).x.ToString("0.00"));
				GUI.Label(new Rect(startX + 30, startY + YOffset * iPad + 80, 100, 20), PadManager.GetAxis(PadManager.Axis.RightStick, (PadManager.Index)iPad, true).y.ToString("0.00"));


				// 入力履歴
				int iii = 0;
				foreach (PadManager.Button btn in pad.PadHistory)
				{

					GUI.Label(new Rect(startX  -100 + 50 * (pad.PadHistory.Count - iii), startY + YOffset * iPad - 50, 100, 20), btn.ToString());
					iii++;
				}


				float BtnX = startX + 150;
				float BtnY = startY + YOffset * iPad - 20;



				// ボタンRaw
				for (int button = 0; button < PadManager.PadButtonMax; button++)
				{

					Rect r = new Rect(BtnX + 20 * button, BtnY, 5, 5);
					if (PadManager.GetRawButton(button, (PadManager.Index)iPad))
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
				PadPressFunc[] funcArray = new PadPressFunc[] { PadManager.GetTrigger, PadManager.GetPress, PadManager.GetRepeat, PadManager.GetRelease };
				foreach (PadPressFunc func in funcArray)
				{
					BtnY += 15;
					for (int button = 0; button < (int)PadManager.Button.MAX; button++)
					{

						Rect r = new Rect(BtnX + 20 * button, BtnY, 5, 5);

						if (func((PadManager.Button)button, (PadManager.Index)iPad))
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

	
	// Update is called once per frame
	void Update () {
	
	}
}
