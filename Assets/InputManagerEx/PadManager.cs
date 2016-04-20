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

	const int PadButtonMax = 16;
	static private int[] PadButtonConvTable = new int[PadButtonMax]; // 変換テーブル


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
		Active, _1P, _2P, _3P, _4P,Num
	};


	string[] RightAxisX = new string[(int)Index.Num];
	string[] RightAxisY = new string[(int)Index.Num];
	string[] PovX = new string[(int)Index.Num];
	string[] PovY = new string[(int)Index.Num];

	string[] LRTriggerAxis = new string[(int)Index.Num];

	/// <summary>
	///  ボタン定義
	/// </summary>
	public enum Button { A, B, X, Y, LB, RB, Back, Start, LS, RS, LT, RT,Max }

	IEnumerator AxisSettingFunc()
	{
		int padIndex = 1;


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

					Debug.Log("アクティブなパッド決定＆スティックキャリブレーション " + padIndex);
					ActivePadIndex = padIndex;

					bDecide = true;

					// OKボタンをゼロ番に割当
					PadButtonConvTable[(int)Button.A] = i;
					// とりあえず連番で初期化
					for (int ibtn = 1; ibtn < PadButtonMax; ibtn++)
					{
						PadButtonConvTable[ibtn] = ibtn;
					}

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
				if (padIndex > 4) padIndex = 1;

				yield return new WaitForSeconds(0.1f);

			}
		}

		yield return new WaitForSeconds(1.0f);

		//-----------------------------
		// 右スティックY軸チェック
		//-----------------------------
		while(true){
			if (AxisConfigIndex == 1) { RightAxisY[padIndex] = "_1"; } else { RightAxisY[padIndex] = ""; }
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

				if(GetButton(Button.A)){
					// 右スティックなし
					AxisConfigStep++;
					RightAxisY[padIndex] = "_none";

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

				if(GetButton(Button.A)){
					// 右スティックなし
					AxisConfigStep++;
					RightAxisY[padIndex] = "_none";

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
			if (AxisConfigIndex == 1) { PovY[padIndex] = "_1"; } else { PovY[padIndex] = ""; }
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
			if (AxisConfigIndex == 1) { PovX[padIndex] = "_1"; } else { PovX[padIndex] = ""; }
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
			if ( Mathf.Abs(GetAxis(Axis.LRTrigger, (Index)padIndex).x) >= 0.8f)
			{

				AxisConfigStep++;
				AxisConfigIndex = 0;
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
					LRTriggerAxis[padIndex] = "_none";
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


		float size = 20.0f;

		if (GUI.Button(new Rect(8, 20, 180, 24), "パッドコンフィグ開始"))
		{
			IsPadConfig = true;
			AxisConfigStep = 0;
			AxisConfigIndex = 0;

			StartCoroutine("AxisSettingFunc");

		}



		if(IsPadConfig){

			switch (AxisConfigStep)
			{
				case 0: GUI.Label(new Rect(50, 50, 350, 20), "スティックに触らずに使うパットのOKボタン押して！"); break;
				case 1: GUI.Label(new Rect(50, 50, 300, 20), "右スティックY軸チェック 下に倒して！\n無い場合はOKボタン押して！"); break;
				case 2: GUI.Label(new Rect(50, 50, 300, 20), "右スティックX軸チェック 右に倒して！\n無い場合はOKボタン押して！"); break;
				case 3: GUI.Label(new Rect(50, 50, 300, 20), "十字キー(POV) Y チェック 下押して！"); break;
				case 4: GUI.Label(new Rect(50, 50, 300, 20), "十字キー(POV) X チェック 右押して！"); break;
				case 5: GUI.Label(new Rect(50, 50, 300, 20), "L2/R2どっちか押して！(Xbox360判定)"); break;

			}

			

		}else{


			
			float startY = 100;
			float startX = 100;

			GUI.Box(new Rect(10, startY-50, 650, 300),"");

			GUI.Label(new Rect(startX, startY - 50, 100, 20), "左");
			GUI.Label(new Rect(startX + 50, startY - 50, 100, 20), "右");
			GUI.Label(new Rect(startX + 100, startY - 50, 100, 20), "POV");

			for (int i = 0; i <= (int)Index._4P; i++)
			{

				if (ActivePadIndex == i)
				{
					GUI.Label(new Rect(startX-90, startY + 50 * i, 100, 20), "Active→");
				}


				// スティック
				GUI.Label(new Rect(startX + GetAxis(Axis.LeftStick, (Index)i).x * size, startY + 50 * i - GetAxis(Axis.LeftStick, (Index)i).y * size, 100, 20), "+");
				GUI.Label(new Rect(startX, startY + 50 * i, 100, 20), "+");

				GUI.Label(new Rect(startX + 50 + GetAxis(Axis.RightStick, (Index)i).x * size, startY + 50 * i - GetAxis(Axis.RightStick, (Index)i).y * size, 100, 20), "+");
				GUI.Label(new Rect(startX + 50, startY + 50 * i, 100, 20), "+");


				GUI.Label(new Rect(startX + 100 + GetAxis(Axis.POV, (Index)i).x * size, startY + 50 * i + GetAxis(Axis.POV, (Index)i).y * size, 100, 20), "+");
				GUI.Label(new Rect(startX + 100, startY + 50 * i, 100, 20), "+");

				// ボタンRaw
				for (int button = 0; button < 16; button++)
				{
					if (GetButton(button, (Index)i))
					{

						GUI.Label(new Rect(startX + 150 + 20 * button, startY + 50 * i-20, 100, 20), "O");
					}
					else
					{
						GUI.Label(new Rect(startX + 150 + 20 * button, startY + 50 * i-20, 100, 20), "X");
					}
				}

				// LT/RT
				GUI.Label(new Rect(startX + 150 + 20 * 16, startY + 50 * i - 20, 100, 20), GetAxis(Axis.LRTrigger, (Index)i).y.ToString("0.00"));
				


				// ボタンコンフィグ
				for (int button = 0; button < 12; button++)
				{
					if (GetButton((Button)button, (Index)i))
					{

						GUI.Label(new Rect(startX + 150 + 20 * button, startY + 50 * i, 100, 20), "O");
					}
					else
					{
						GUI.Label(new Rect(startX + 150 + 20 * button, startY + 50 * i, 100, 20), "X");
					}
				}

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
	public static Vector2 GetAxis(Axis axis, Index controlIndex = Index.Active, bool raw = true)
	{

		if (controlIndex == Index.Active)
		{
			controlIndex = (Index)ActivePadIndex;
		}

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
			case Axis.LRTrigger:
				xName = "Player" + (int)controlIndex + "_LRTrigger" + instance.LRTriggerAxis[(int)controlIndex];
				yName = "Player" + (int)controlIndex + "_LRTrigger"+ instance.LRTriggerAxis[(int)controlIndex];
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
	/// ボタン押下
	/// </summary>
	/// <param name="button"></param>
	/// <param name="controlIndex"></param>
	/// <returns></returns>
	public static bool GetButton(Button button, Index controlIndex = Index.Active)
	{
		if (controlIndex == Index.Active)
		{
			controlIndex = (Index)ActivePadIndex;
		}

		string code = GetButtonName(button, controlIndex);
		return Input.GetButton(code);
	}
	public static bool GetButton(int button, Index controlIndex = Index.Active)
	{

		if (controlIndex == Index.Active)
		{
			controlIndex = (Index)ActivePadIndex;
		}

		return Input.GetButton("Player" + (int)controlIndex + "_Btn" + button);
	}


	static string GetButtonName(Button button, Index controlIndex)
	{

		if (button < Button.Max)
		{
			return "Player" + (int)controlIndex + "_Btn" + PadButtonConvTable[(int)button];
		}

		return "none";
	}



	// Use this for initialization
	void Start () {

		// とりあえず連番で初期化
		for (int ibtn = 0; ibtn < PadButtonMax; ibtn++)
		{
			PadButtonConvTable[ibtn] = ibtn;
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
