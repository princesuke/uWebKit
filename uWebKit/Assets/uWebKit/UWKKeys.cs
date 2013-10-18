/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class to map keyboard input from Unity to Web core
/// </summary>
public class UWKKeys
{
	public static Dictionary<KeyCode, QtKey> KeyMap = new Dictionary<KeyCode, QtKey> ();
	public static Dictionary<KeyCode, string> AsciiMap = new Dictionary<KeyCode, string> ();

	static UWKKeys ()
	{
		
		KeyMap[KeyCode.Backspace] = QtKey.Key_Backspace;
		KeyMap[KeyCode.Delete] = QtKey.Key_Delete;
		KeyMap[KeyCode.Tab] = QtKey.Key_Tab;
		KeyMap[KeyCode.Clear] = QtKey.Key_Clear;
		KeyMap[KeyCode.Return] = QtKey.Key_Return;
		KeyMap[KeyCode.Pause] = QtKey.Key_Pause;
		KeyMap[KeyCode.Escape] = QtKey.Key_Escape;
		KeyMap[KeyCode.Space] = QtKey.Key_Space;
		
		KeyMap[KeyCode.Keypad0] = QtKey.Key_0;
		KeyMap[KeyCode.Keypad1] = QtKey.Key_1;
		KeyMap[KeyCode.Keypad2] = QtKey.Key_2;
		KeyMap[KeyCode.Keypad3] = QtKey.Key_3;
		KeyMap[KeyCode.Keypad4] = QtKey.Key_4;
		KeyMap[KeyCode.Keypad5] = QtKey.Key_5;
		KeyMap[KeyCode.Keypad6] = QtKey.Key_6;
		KeyMap[KeyCode.Keypad7] = QtKey.Key_7;
		KeyMap[KeyCode.Keypad8] = QtKey.Key_8;
		KeyMap[KeyCode.Keypad9] = QtKey.Key_9;
		
		KeyMap[KeyCode.KeypadPeriod] = QtKey.Key_Period;
		KeyMap[KeyCode.KeypadDivide] = QtKey.Key_Slash;
		KeyMap[KeyCode.KeypadMultiply] = QtKey.Key_Asterisk;
		KeyMap[KeyCode.KeypadMinus] = QtKey.Key_Minus;
		KeyMap[KeyCode.KeypadEnter] = QtKey.Key_Enter;
		KeyMap[KeyCode.KeypadEquals] = QtKey.Key_Equal;
		KeyMap[KeyCode.UpArrow] = QtKey.Key_Up;
		KeyMap[KeyCode.DownArrow] = QtKey.Key_Down;
		KeyMap[KeyCode.LeftArrow] = QtKey.Key_Left;
		KeyMap[KeyCode.RightArrow] = QtKey.Key_Right;
		
		KeyMap[KeyCode.Insert] = QtKey.Key_Insert;
		KeyMap[KeyCode.Home] = QtKey.Key_Home;
		KeyMap[KeyCode.End] = QtKey.Key_End;
		KeyMap[KeyCode.PageUp] = QtKey.Key_PageUp;
		KeyMap[KeyCode.PageDown] = QtKey.Key_PageDown;
		KeyMap[KeyCode.F1] = QtKey.Key_F1;
		KeyMap[KeyCode.F2] = QtKey.Key_F2;
		KeyMap[KeyCode.F3] = QtKey.Key_F3;
		KeyMap[KeyCode.F4] = QtKey.Key_F4;
		KeyMap[KeyCode.F5] = QtKey.Key_F5;
		KeyMap[KeyCode.F6] = QtKey.Key_F6;
		KeyMap[KeyCode.F7] = QtKey.Key_F7;
		KeyMap[KeyCode.F8] = QtKey.Key_F8;
		KeyMap[KeyCode.F9] = QtKey.Key_F9;
		KeyMap[KeyCode.F10] = QtKey.Key_F10;
		KeyMap[KeyCode.F11] = QtKey.Key_F11;
		KeyMap[KeyCode.F12] = QtKey.Key_F12;
		KeyMap[KeyCode.F13] = QtKey.Key_F13;
		KeyMap[KeyCode.F14] = QtKey.Key_F14;
		KeyMap[KeyCode.F15] = QtKey.Key_F15;
		
		KeyMap[KeyCode.Exclaim] = QtKey.Key_Exclaim;
		KeyMap[KeyCode.DoubleQuote] = QtKey.Key_QuoteDbl;
		KeyMap[KeyCode.Hash] = QtKey.Key_NumberSign;
		KeyMap[KeyCode.Dollar] = QtKey.Key_Dollar;
		KeyMap[KeyCode.Ampersand] = QtKey.Key_Ampersand;
		KeyMap[KeyCode.Quote] = QtKey.Key_Apostrophe;
		KeyMap[KeyCode.LeftParen] = QtKey.Key_ParenLeft;
		KeyMap[KeyCode.RightParen] = QtKey.Key_ParenRight;
		
		KeyMap[KeyCode.Asterisk] = QtKey.Key_Asterisk;
		KeyMap[KeyCode.Plus] = QtKey.Key_Plus;
		KeyMap[KeyCode.Comma] = QtKey.Key_Comma;
		KeyMap[KeyCode.Minus] = QtKey.Key_Minus;
		KeyMap[KeyCode.Period] = QtKey.Key_Period;
		KeyMap[KeyCode.Slash] = QtKey.Key_Slash;
		KeyMap[KeyCode.Colon] = QtKey.Key_Colon;
		KeyMap[KeyCode.Semicolon] = QtKey.Key_Semicolon;
		KeyMap[KeyCode.Less] = QtKey.Key_Less;
		KeyMap[KeyCode.Equals] = QtKey.Key_Equal;
		KeyMap[KeyCode.Greater] = QtKey.Key_Greater;
		KeyMap[KeyCode.Question] = QtKey.Key_Question;
		KeyMap[KeyCode.At] = QtKey.Key_Ampersand;
		KeyMap[KeyCode.LeftBracket] = QtKey.Key_BracketLeft;
		KeyMap[KeyCode.Backslash] = QtKey.Key_Backslash;
		KeyMap[KeyCode.RightBracket] = QtKey.Key_BracketRight;
		KeyMap[KeyCode.Caret] = QtKey.Key_AsciiCircum;
		KeyMap[KeyCode.Underscore] = QtKey.Key_Underscore;
		KeyMap[KeyCode.BackQuote] = QtKey.Key_Agrave;
		
		KeyMap[KeyCode.Numlock] = QtKey.Key_NumLock;
		KeyMap[KeyCode.CapsLock] = QtKey.Key_CapsLock;
		KeyMap[KeyCode.ScrollLock] = QtKey.Key_ScrollLock;
		KeyMap[KeyCode.RightShift] = QtKey.Key_Shift;
		//keyMap[KeyCode.RightWindows] = Qt.QtKey;
		KeyMap[KeyCode.AltGr] = QtKey.Key_AltGr;
		KeyMap[KeyCode.Help] = QtKey.Key_Help;
		KeyMap[KeyCode.Print] = QtKey.Key_Print;
		KeyMap[KeyCode.SysReq] = QtKey.Key_SysReq;
		//keyMap[KeyCode.Break] = Qt.QtKey;
		
		KeyMap[KeyCode.Menu] = QtKey.Key_Menu;
		
		KeyMap[KeyCode.A] = QtKey.Key_A;
		KeyMap[KeyCode.B] = QtKey.Key_B;
		KeyMap[KeyCode.C] = QtKey.Key_C;
		KeyMap[KeyCode.D] = QtKey.Key_D;
		KeyMap[KeyCode.E] = QtKey.Key_E;
		KeyMap[KeyCode.F] = QtKey.Key_F;
		KeyMap[KeyCode.G] = QtKey.Key_G;
		KeyMap[KeyCode.H] = QtKey.Key_H;
		KeyMap[KeyCode.I] = QtKey.Key_I;
		KeyMap[KeyCode.J] = QtKey.Key_J;
		KeyMap[KeyCode.K] = QtKey.Key_K;
		KeyMap[KeyCode.L] = QtKey.Key_L;
		KeyMap[KeyCode.M] = QtKey.Key_M;
		KeyMap[KeyCode.N] = QtKey.Key_N;
		KeyMap[KeyCode.O] = QtKey.Key_O;
		KeyMap[KeyCode.P] = QtKey.Key_P;
		KeyMap[KeyCode.Q] = QtKey.Key_Q;
		KeyMap[KeyCode.R] = QtKey.Key_R;
		KeyMap[KeyCode.S] = QtKey.Key_S;
		KeyMap[KeyCode.T] = QtKey.Key_T;
		KeyMap[KeyCode.U] = QtKey.Key_U;
		KeyMap[KeyCode.V] = QtKey.Key_V;
		KeyMap[KeyCode.W] = QtKey.Key_W;
		KeyMap[KeyCode.X] = QtKey.Key_X;
		KeyMap[KeyCode.Y] = QtKey.Key_Y;
		KeyMap[KeyCode.Z] = QtKey.Key_Z;
		
		KeyMap[KeyCode.Alpha0] = QtKey.Key_0;
		KeyMap[KeyCode.Alpha1] = QtKey.Key_1;
		KeyMap[KeyCode.Alpha2] = QtKey.Key_2;
		KeyMap[KeyCode.Alpha3] = QtKey.Key_3;
		KeyMap[KeyCode.Alpha4] = QtKey.Key_4;
		KeyMap[KeyCode.Alpha5] = QtKey.Key_5;
		KeyMap[KeyCode.Alpha6] = QtKey.Key_6;
		KeyMap[KeyCode.Alpha7] = QtKey.Key_7;
		KeyMap[KeyCode.Alpha8] = QtKey.Key_8;
		KeyMap[KeyCode.Alpha9] = QtKey.Key_9;
		
		AsciiMap[KeyCode.Space] = " ";
		
		AsciiMap[KeyCode.Keypad0] = "0";
		AsciiMap[KeyCode.Keypad1] = "1";
		AsciiMap[KeyCode.Keypad2] = "2";
		AsciiMap[KeyCode.Keypad3] = "3";
		AsciiMap[KeyCode.Keypad4] = "4";
		AsciiMap[KeyCode.Keypad5] = "5";
		AsciiMap[KeyCode.Keypad6] = "6";
		AsciiMap[KeyCode.Keypad7] = "7";
		AsciiMap[KeyCode.Keypad8] = "8";
		AsciiMap[KeyCode.Keypad9] = "9";
		
		AsciiMap[KeyCode.KeypadPeriod] = ".";
		AsciiMap[KeyCode.KeypadDivide] = "\\";
		AsciiMap[KeyCode.KeypadMultiply] = "*";
		AsciiMap[KeyCode.KeypadMinus] = "-";
		
		AsciiMap[KeyCode.KeypadEquals] = "=";
		
		AsciiMap[KeyCode.Exclaim] = "!";
		AsciiMap[KeyCode.DoubleQuote] = "\"";
		AsciiMap[KeyCode.Hash] = "#";
		AsciiMap[KeyCode.Dollar] = "$";
		AsciiMap[KeyCode.Ampersand] = "&";
		AsciiMap[KeyCode.Quote] = "'";
		AsciiMap[KeyCode.LeftParen] = "[";
		AsciiMap[KeyCode.RightParen] = "]";
		
		AsciiMap[KeyCode.Asterisk] = "*";
		AsciiMap[KeyCode.Plus] = "+";
		AsciiMap[KeyCode.Comma] = ",";
		AsciiMap[KeyCode.Minus] = "-";
		AsciiMap[KeyCode.Period] = ".";
		AsciiMap[KeyCode.Slash] = "/";
		AsciiMap[KeyCode.Colon] = ":";
		AsciiMap[KeyCode.Semicolon] = ";";
		AsciiMap[KeyCode.Less] = "<";
		AsciiMap[KeyCode.Equals] = "=";
		AsciiMap[KeyCode.Greater] = ">";
		AsciiMap[KeyCode.Question] = "?";
		AsciiMap[KeyCode.At] = "@";
		AsciiMap[KeyCode.LeftBracket] = "{";
		AsciiMap[KeyCode.Backslash] = "\\";
		AsciiMap[KeyCode.RightBracket] = "}";
		AsciiMap[KeyCode.Caret] = "^";
		AsciiMap[KeyCode.Underscore] = "_";
		AsciiMap[KeyCode.BackQuote] = "`";
		
		AsciiMap[KeyCode.A] = "A";
		AsciiMap[KeyCode.B] = "B";
		AsciiMap[KeyCode.C] = "C";
		AsciiMap[KeyCode.D] = "D";
		AsciiMap[KeyCode.E] = "E";
		AsciiMap[KeyCode.F] = "F";
		AsciiMap[KeyCode.G] = "G";
		AsciiMap[KeyCode.H] = "H";
		AsciiMap[KeyCode.I] = "I";
		AsciiMap[KeyCode.J] = "J";
		AsciiMap[KeyCode.K] = "K";
		AsciiMap[KeyCode.L] = "L";
		AsciiMap[KeyCode.M] = "M";
		AsciiMap[KeyCode.N] = "N";
		AsciiMap[KeyCode.O] = "O";
		AsciiMap[KeyCode.P] = "P";
		AsciiMap[KeyCode.Q] = "Q";
		AsciiMap[KeyCode.R] = "R";
		AsciiMap[KeyCode.S] = "S";
		AsciiMap[KeyCode.T] = "T";
		AsciiMap[KeyCode.U] = "U";
		AsciiMap[KeyCode.V] = "V";
		AsciiMap[KeyCode.W] = "W";
		AsciiMap[KeyCode.X] = "X";
		AsciiMap[KeyCode.Y] = "Y";
		AsciiMap[KeyCode.Z] = "Z";
		
		AsciiMap[KeyCode.Alpha0] = "0";
		AsciiMap[KeyCode.Alpha1] = "1";
		AsciiMap[KeyCode.Alpha2] = "2";
		AsciiMap[KeyCode.Alpha3] = "3";
		AsciiMap[KeyCode.Alpha4] = "4";
		AsciiMap[KeyCode.Alpha5] = "5";
		AsciiMap[KeyCode.Alpha6] = "6";
		AsciiMap[KeyCode.Alpha7] = "7";
		AsciiMap[KeyCode.Alpha8] = "8";
		AsciiMap[KeyCode.Alpha9] = "9";
	}


	public static uint MapUnityKey (KeyCode k)
	{
		return 0;
	}

	public enum KeyboardModifier : uint
	{
		NoModifier = 0x00000000,
		ShiftModifier = 0x02000000,
		ControlModifier = 0x04000000,
		AltModifier = 0x08000000,
		MetaModifier = 0x10000000,
		KeypadModifier = 0x20000000,
		GroupSwitchModifier = 0x40000000,
		KeyboardModifierMask = 0xfe000000
	}
	public enum Modifier : uint
	{
		META = KeyboardModifier.MetaModifier,
		SHIFT = KeyboardModifier.ShiftModifier,
		CTRL = KeyboardModifier.ControlModifier,
		ALT = KeyboardModifier.AltModifier,
		MODIFIER_MASK = KeyboardModifier.KeyboardModifierMask,
		UNICODE_ACCEL = 0x00000000
	}
	public enum MouseButton
	{
		NoButton = 0x00000000,
		LeftButton = 0x00000001,
		RightButton = 0x00000002,
		MidButton = 0x00000004,
		XButton1 = 0x00000008,
		XButton2 = 0x00000010,
		MouseButtonMask = 0x000000ff
	}
	public enum FocusPolicy
	{
		NoFocus = 0,
		TabFocus = 0x1,
		ClickFocus = 0x2,
		StrongFocus = TabFocus | ClickFocus | 0x8,
		WheelFocus = StrongFocus | 0x4
	}

	public enum QtKey
	{
		Key_Escape = 0x01000000,
		Key_Tab = 0x01000001,
		Key_Backtab = 0x01000002,
		Key_Backspace = 0x01000003,
		Key_Return = 0x01000004,
		Key_Enter = 0x01000005,
		Key_Insert = 0x01000006,
		Key_Delete = 0x01000007,
		Key_Pause = 0x01000008,
		Key_Print = 0x01000009,
		Key_SysReq = 0x0100000a,
		Key_Clear = 0x0100000b,
		Key_Home = 0x01000010,
		Key_End = 0x01000011,
		Key_Left = 0x01000012,
		Key_Up = 0x01000013,
		Key_Right = 0x01000014,
		Key_Down = 0x01000015,
		Key_PageUp = 0x01000016,
		Key_PageDown = 0x01000017,
		Key_Shift = 0x01000020,
		Key_Control = 0x01000021,
		Key_Meta = 0x01000022,
		Key_Alt = 0x01000023,
		Key_CapsLock = 0x01000024,
		Key_NumLock = 0x01000025,
		Key_ScrollLock = 0x01000026,
		Key_F1 = 0x01000030,
		Key_F2 = 0x01000031,
		Key_F3 = 0x01000032,
		Key_F4 = 0x01000033,
		Key_F5 = 0x01000034,
		Key_F6 = 0x01000035,
		Key_F7 = 0x01000036,
		Key_F8 = 0x01000037,
		Key_F9 = 0x01000038,
		Key_F10 = 0x01000039,
		Key_F11 = 0x0100003a,
		Key_F12 = 0x0100003b,
		Key_F13 = 0x0100003c,
		Key_F14 = 0x0100003d,
		Key_F15 = 0x0100003e,
		Key_F16 = 0x0100003f,
		Key_F17 = 0x01000040,
		Key_F18 = 0x01000041,
		Key_F19 = 0x01000042,
		Key_F20 = 0x01000043,
		Key_F21 = 0x01000044,
		Key_F22 = 0x01000045,
		Key_F23 = 0x01000046,
		Key_F24 = 0x01000047,
		Key_F25 = 0x01000048,
		Key_F26 = 0x01000049,
		Key_F27 = 0x0100004a,
		Key_F28 = 0x0100004b,
		Key_F29 = 0x0100004c,
		Key_F30 = 0x0100004d,
		Key_F31 = 0x0100004e,
		Key_F32 = 0x0100004f,
		Key_F33 = 0x01000050,
		Key_F34 = 0x01000051,
		Key_F35 = 0x01000052,
		Key_Super_L = 0x01000053,
		Key_Super_R = 0x01000054,
		Key_Menu = 0x01000055,
		Key_Hyper_L = 0x01000056,
		Key_Hyper_R = 0x01000057,
		Key_Help = 0x01000058,
		Key_Direction_L = 0x01000059,
		Key_Direction_R = 0x01000060,
		Key_Space = 0x20,
		Key_Any = Key_Space,
		Key_Exclaim = 0x21,
		Key_QuoteDbl = 0x22,
		Key_NumberSign = 0x23,
		Key_Dollar = 0x24,
		Key_Percent = 0x25,
		Key_Ampersand = 0x26,
		Key_Apostrophe = 0x27,
		Key_ParenLeft = 0x28,
		Key_ParenRight = 0x29,
		Key_Asterisk = 0x2a,
		Key_Plus = 0x2b,
		Key_Comma = 0x2c,
		Key_Minus = 0x2d,
		Key_Period = 0x2e,
		Key_Slash = 0x2f,
		Key_0 = 0x30,
		Key_1 = 0x31,
		Key_2 = 0x32,
		Key_3 = 0x33,
		Key_4 = 0x34,
		Key_5 = 0x35,
		Key_6 = 0x36,
		Key_7 = 0x37,
		Key_8 = 0x38,
		Key_9 = 0x39,
		Key_Colon = 0x3a,
		Key_Semicolon = 0x3b,
		Key_Less = 0x3c,
		Key_Equal = 0x3d,
		Key_Greater = 0x3e,
		Key_Question = 0x3f,
		Key_At = 0x40,
		Key_A = 0x41,
		Key_B = 0x42,
		Key_C = 0x43,
		Key_D = 0x44,
		Key_E = 0x45,
		Key_F = 0x46,
		Key_G = 0x47,
		Key_H = 0x48,
		Key_I = 0x49,
		Key_J = 0x4a,
		Key_K = 0x4b,
		Key_L = 0x4c,
		Key_M = 0x4d,
		Key_N = 0x4e,
		Key_O = 0x4f,
		Key_P = 0x50,
		Key_Q = 0x51,
		Key_R = 0x52,
		Key_S = 0x53,
		Key_T = 0x54,
		Key_U = 0x55,
		Key_V = 0x56,
		Key_W = 0x57,
		Key_X = 0x58,
		Key_Y = 0x59,
		Key_Z = 0x5a,
		Key_BracketLeft = 0x5b,
		Key_Backslash = 0x5c,
		Key_BracketRight = 0x5d,
		Key_AsciiCircum = 0x5e,
		Key_Underscore = 0x5f,
		Key_QuoteLeft = 0x60,
		Key_BraceLeft = 0x7b,
		Key_Bar = 0x7c,
		Key_BraceRight = 0x7d,
		Key_AsciiTilde = 0x7e,
		Key_nobreakspace = 0x0a0,
		Key_exclamdown = 0x0a1,
		Key_cent = 0x0a2,
		Key_sterling = 0x0a3,
		Key_currency = 0x0a4,
		Key_yen = 0x0a5,
		Key_brokenbar = 0x0a6,
		Key_section = 0x0a7,
		Key_diaeresis = 0x0a8,
		Key_copyright = 0x0a9,
		Key_ordfeminine = 0x0aa,
		Key_guillemotleft = 0x0ab,
		Key_notsign = 0x0ac,
		Key_hyphen = 0x0ad,
		Key_registered = 0x0ae,
		Key_macron = 0x0af,
		Key_degree = 0x0b0,
		Key_plusminus = 0x0b1,
		Key_twosuperior = 0x0b2,
		Key_threesuperior = 0x0b3,
		Key_acute = 0x0b4,
		Key_mu = 0x0b5,
		Key_paragraph = 0x0b6,
		Key_periodcentered = 0x0b7,
		Key_cedilla = 0x0b8,
		Key_onesuperior = 0x0b9,
		Key_masculine = 0x0ba,
		Key_guillemotright = 0x0bb,
		Key_onequarter = 0x0bc,
		Key_onehalf = 0x0bd,
		Key_threequarters = 0x0be,
		Key_questiondown = 0x0bf,
		Key_Agrave = 0x0c0,
		Key_Aacute = 0x0c1,
		Key_Acircumflex = 0x0c2,
		Key_Atilde = 0x0c3,
		Key_Adiaeresis = 0x0c4,
		Key_Aring = 0x0c5,
		Key_AE = 0x0c6,
		Key_Ccedilla = 0x0c7,
		Key_Egrave = 0x0c8,
		Key_Eacute = 0x0c9,
		Key_Ecircumflex = 0x0ca,
		Key_Ediaeresis = 0x0cb,
		Key_Igrave = 0x0cc,
		Key_Iacute = 0x0cd,
		Key_Icircumflex = 0x0ce,
		Key_Idiaeresis = 0x0cf,
		Key_ETH = 0x0d0,
		Key_Ntilde = 0x0d1,
		Key_Ograve = 0x0d2,
		Key_Oacute = 0x0d3,
		Key_Ocircumflex = 0x0d4,
		Key_Otilde = 0x0d5,
		Key_Odiaeresis = 0x0d6,
		Key_multiply = 0x0d7,
		Key_Ooblique = 0x0d8,
		Key_Ugrave = 0x0d9,
		Key_Uacute = 0x0da,
		Key_Ucircumflex = 0x0db,
		Key_Udiaeresis = 0x0dc,
		Key_Yacute = 0x0dd,
		Key_THORN = 0x0de,
		Key_ssharp = 0x0df,
		Key_division = 0x0f7,
		Key_ydiaeresis = 0x0ff,
		Key_AltGr = 0x01001103,
		Key_Multi_key = 0x01001120,
		Key_Codeinput = 0x01001137,
		Key_SingleCandidate = 0x0100113c,
		Key_MultipleCandidate = 0x0100113d,
		Key_PreviousCandidate = 0x0100113e,
		Key_Mode_switch = 0x0100117e,
		Key_Kanji = 0x01001121,
		Key_Muhenkan = 0x01001122,
		Key_Henkan = 0x01001123,
		Key_Romaji = 0x01001124,
		Key_Hiragana = 0x01001125,
		Key_Katakana = 0x01001126,
		Key_Hiragana_Katakana = 0x01001127,
		Key_Zenkaku = 0x01001128,
		Key_Hankaku = 0x01001129,
		Key_Zenkaku_Hankaku = 0x0100112a,
		Key_Touroku = 0x0100112b,
		Key_Massyo = 0x0100112c,
		Key_Kana_Lock = 0x0100112d,
		Key_Kana_Shift = 0x0100112e,
		Key_Eisu_Shift = 0x0100112f,
		Key_Eisu_toggle = 0x01001130,
		Key_Hangul = 0x01001131,
		Key_Hangul_Start = 0x01001132,
		Key_Hangul_End = 0x01001133,
		Key_Hangul_Hanja = 0x01001134,
		Key_Hangul_Jamo = 0x01001135,
		Key_Hangul_Romaja = 0x01001136,
		Key_Hangul_Jeonja = 0x01001138,
		Key_Hangul_Banja = 0x01001139,
		Key_Hangul_PreHanja = 0x0100113a,
		Key_Hangul_PostHanja = 0x0100113b,
		Key_Hangul_Special = 0x0100113f,
		Key_Dead_Grave = 0x01001250,
		Key_Dead_Acute = 0x01001251,
		Key_Dead_Circumflex = 0x01001252,
		Key_Dead_Tilde = 0x01001253,
		Key_Dead_Macron = 0x01001254,
		Key_Dead_Breve = 0x01001255,
		Key_Dead_Abovedot = 0x01001256,
		Key_Dead_Diaeresis = 0x01001257,
		Key_Dead_Abovering = 0x01001258,
		Key_Dead_Doubleacute = 0x01001259,
		Key_Dead_Caron = 0x0100125a,
		Key_Dead_Cedilla = 0x0100125b,
		Key_Dead_Ogonek = 0x0100125c,
		Key_Dead_Iota = 0x0100125d,
		Key_Dead_Voiced_Sound = 0x0100125e,
		Key_Dead_Semivoiced_Sound = 0x0100125f,
		Key_Dead_Belowdot = 0x01001260,
		Key_Dead_Hook = 0x01001261,
		Key_Dead_Horn = 0x01001262,
		Key_Back = 0x01000061,
		Key_Forward = 0x01000062,
		Key_Stop = 0x01000063,
		Key_Refresh = 0x01000064,
		Key_VolumeDown = 0x01000070,
		Key_VolumeMute = 0x01000071,
		Key_VolumeUp = 0x01000072,
		Key_BassBoost = 0x01000073,
		Key_BassUp = 0x01000074,
		Key_BassDown = 0x01000075,
		Key_TrebleUp = 0x01000076,
		Key_TrebleDown = 0x01000077,
		Key_MediaPlay = 0x01000080,
		Key_MediaStop = 0x01000081,
		Key_MediaPrevious = 0x01000082,
		Key_MediaNext = 0x01000083,
		Key_MediaRecord = 0x01000084,
		Key_HomePage = 0x01000090,
		Key_Favorites = 0x01000091,
		Key_Search = 0x01000092,
		Key_Standby = 0x01000093,
		Key_OpenUrl = 0x01000094,
		Key_LaunchMail = 0x010000a0,
		Key_LaunchMedia = 0x010000a1,
		Key_Launch0 = 0x010000a2,
		Key_Launch1 = 0x010000a3,
		Key_Launch2 = 0x010000a4,
		Key_Launch3 = 0x010000a5,
		Key_Launch4 = 0x010000a6,
		Key_Launch5 = 0x010000a7,
		Key_Launch6 = 0x010000a8,
		Key_Launch7 = 0x010000a9,
		Key_Launch8 = 0x010000aa,
		Key_Launch9 = 0x010000ab,
		Key_LaunchA = 0x010000ac,
		Key_LaunchB = 0x010000ad,
		Key_LaunchC = 0x010000ae,
		Key_LaunchD = 0x010000af,
		Key_LaunchE = 0x010000b0,
		Key_LaunchF = 0x010000b1,
		Key_MediaLast = 0x0100ffff,
		Key_Select = 0x01010000,
		Key_Yes = 0x01010001,
		Key_No = 0x01010002,
		Key_Cancel = 0x01020001,
		Key_Printer = 0x01020002,
		Key_Execute = 0x01020003,
		Key_Sleep = 0x01020004,
		Key_Play = 0x01020005,
		Key_Zoom = 0x01020006,
		Key_Context1 = 0x01100000,
		Key_Context2 = 0x01100001,
		Key_Context3 = 0x01100002,
		Key_Context4 = 0x01100003,
		Key_Call = 0x01100004,
		Key_Hangup = 0x01100005,
		Key_Flip = 0x01100006,
		Key_unknown = 0x01ffffff
	}
	
	
}

