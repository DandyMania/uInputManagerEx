#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// InputManagerを自動的に設定してくれるクラス
/// </summary>
public class InputSetter
{


	/// <summary>
	/// インプット設定をリセット
	/// </summary>
	[MenuItem("InputTool/Reset Setting")]
	public static void ResetInputManager()
	{
		Debug.Log("create input settings");
		InputSettingGenerator inputSettingGenerator = new InputSettingGenerator();
		inputSettingGenerator.Clear();
		for (int i = 0; i < 4; i++)
		{
			AddPlayerInputSettings(inputSettingGenerator, i);
		}

		Debug.Log("add global settings");
		AddGlobalInputSettings(inputSettingGenerator);

		Debug.Log("finish.");
	}
	
	/// <summary>
	/// Input設定を開く
	/// </summary>
	[MenuItem("InputTool/Open Setting")]
	public static void OpenInputManager()
	{
		EditorApplication.ExecuteMenuItem("Edit/Project Settings/Input");  
	}



	/// <summary>
	/// グローバルな入力設定を追加する（OK、キャンセルなど）
	/// </summary>
	/// <param name="inputSettingGenerator">Input manager generator.</param>
	private static void AddGlobalInputSettings(InputSettingGenerator inputSettingGenerator)
	{

		// LX
		{
			var name = "Player0_LX";
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, 0, 1));
			inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, "a", "d", "left", "right"));
		}
		// LY
		{
			var name = "Player0_LY";
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, 0, 2));
			inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, "s", "w", "down", "up"));
		}
		// RX
		{
			var name = "Player0_RX";
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, 0, 4));
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name + "_1", 0, 3));
			inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, "a", "d", "left", "right"));
		}
		// RY
		{
			var name = "Player0_RY";
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, 0, 5));
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name + "_1", 0, 4));

			inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, "s", "w", "down", "up"));
		}
		// DX
		{
			var name = "Player0_DX";
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, 0, 6));
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name + "_1", 0, 5));
			inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, "a", "d", "left", "right"));
		}
		// DY
		{
			var name = "Player0_DY";
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, 0, 7));
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name + "_1", 0, 6));
			inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, "s", "w", "down", "up"));
		}

		// 決定
		{
			var name = "Player0_OK";
			inputSettingGenerator.AddAxis(InputAxis.CreateButton(name, "z", "joystick button 0"));
		}

		// キャンセル
		{
			var name = "Player0_Cancel";
			inputSettingGenerator.AddAxis(InputAxis.CreateButton(name, "x", "joystick button 1"));
		}

		// ポーズ
		{
			var name = "Player0_Pause";
			inputSettingGenerator.AddAxis(InputAxis.CreateButton(name, "escape", "joystick button 7"));
		}
	}

	/// <summary>
	/// プレイヤーごとの入力設定を追加する
	/// </summary>
	/// <param name="inputSettingGenerator">Input manager generator.</param>
	/// <param name="playerIndex">Player index.</param>
	private static void AddPlayerInputSettings(InputSettingGenerator inputSettingGenerator, int playerIndex)
	{
		if (playerIndex < 0 || playerIndex > 3) Debug.LogError("プレイヤーインデックスの値が不正です。");
		string upKey = "", downKey = "", leftKey = "", rightKey = "", attackKey = "";
		GetAxisKey(out upKey, out downKey, out leftKey, out rightKey, out attackKey, playerIndex);

		int joystickNum = playerIndex + 1;

		// LX
		{
			var name = string.Format("Player{0}_LX", joystickNum);
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 1));
			//inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, leftKey, rightKey, "", ""));
		}

		// LY
		{
			var name = string.Format("Player{0}_LY", joystickNum);
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 2));
			//inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, downKey, upKey, "", ""));
		}
		// RX
		{
			var name = string.Format("Player{0}_RX", joystickNum);
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 4));
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name + "_1", joystickNum, 3));
			//inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, leftKey, rightKey, "", ""));
		}

		// RY
		{
			var name = string.Format("Player{0}_RY", joystickNum);
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 5));
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name + "_1" ,joystickNum, 4));
			//inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, downKey, upKey, "", ""));
		}
		// DX
		{
			var name = string.Format("Player{0}_DX", joystickNum);
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 6));
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name + "_1", joystickNum, 5));
			//inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, leftKey, rightKey, "", ""));
		}

		// DY
		{
			var name = string.Format("Player{0}_DY", joystickNum);
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 7));
			inputSettingGenerator.AddAxis(InputAxis.CreatePadAxis(name + "_1", joystickNum, 6));
			//inputSettingGenerator.AddAxis(InputAxis.CreateKeyAxis(name, downKey, upKey, "", ""));
		}

		// OK
		{
			//var axis = new InputAxis();
			var name = string.Format("Player{0}_OK", joystickNum);
			var button = string.Format("joystick {0} button 0", joystickNum);
			inputSettingGenerator.AddAxis(InputAxis.CreateButton(name, button, attackKey));
		}
		// Cancel
		{
			//var axis = new InputAxis();
			var name = string.Format("Player{0}_Cancel", joystickNum);
			var button = string.Format("joystick {0} button 1", joystickNum);
			inputSettingGenerator.AddAxis(InputAxis.CreateButton(name, button, attackKey));
		}
	}

	/// <summary>
	/// キーボードでプレイした場合、割り当たっているキーを取得する
	/// </summary>
	/// <param name="upKey">Up key.</param>
	/// <param name="downKey">Down key.</param>
	/// <param name="leftKey">Left key.</param>
	/// <param name="rightKey">Right key.</param>
	/// <param name="attackKey">Attack key.</param>
	/// <param name="playerIndex">Player index.</param>
	private static void GetAxisKey(out string upKey, out string downKey, out string leftKey, out string rightKey, out string attackKey, int playerIndex)
	{
		upKey = "";
		downKey = "";
		leftKey = "";
		rightKey = "";
		attackKey = "";

		switch (playerIndex)
		{
			case 0:
				upKey = "w";
				downKey = "s";
				leftKey = "a";
				rightKey = "d";
				attackKey = "e";
				break;
			case 1:
				upKey = "i";
				downKey = "k";
				leftKey = "j";
				rightKey = "l";
				attackKey = "o";
				break;
			case 2:
				upKey = "up";
				downKey = "down";
				leftKey = "left";
				rightKey = "right";
				attackKey = "[0]";
				break;
			case 3:
				upKey = "[8]";
				downKey = "[5]";
				leftKey = "[4]";
				rightKey = "[6]";
				attackKey = "[9]";
				break;
			default:
				Debug.LogError("プレイヤーインデックスの値が不正です。");
				upKey = "";
				downKey = "";
				leftKey = "";
				rightKey = "";
				attackKey = "";
				break;
		}
	}
}

#endif